using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using log4net;
using Neo.Core;
using Neo.Core.Util;
using Neo.Framework;


namespace Neo.SqlClient
{
	public class SqlDataStore : IDataStore
	{
		//--------------------------------------------------------------------------------------
		//	Static fields and constructor
		//--------------------------------------------------------------------------------------

		protected static ILog logger = null;
	
		static SqlDataStore()
		{
			if(logger == null)
				logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString());
		}
	

		//--------------------------------------------------------------------------------------
		//	Fields and constructor
		//--------------------------------------------------------------------------------------

		private SqlConnection		connection;
		private SqlTransaction		transaction;
		private ArrayList			processedRows;

		public SqlDataStore() : this(null)
		{
		}

		public SqlDataStore(string connectionString)
		{
			logger.Debug("Created new SqlDataStore.");

			if(connectionString == null)
			{
				NameValueCollection	config = (NameValueCollection)ConfigurationSettings.GetConfig("neo.sqlclient");
				if(config != null)
					connectionString = config["connectionstring"];
			}
			connection = new SqlConnection(connectionString);
		}


		//--------------------------------------------------------------------------------------
		//	Opening and closing the connection
		//--------------------------------------------------------------------------------------

		protected virtual void EnsureOpen()
		{
			if(connection.State != ConnectionState.Open)
			{
				logger.Debug("Opening connection to \"" + GetConnectionStringForLogging() + "\"");
				connection.Open();
			}
		}

		protected virtual void Close()
		{
			if(connection.State != ConnectionState.Closed && transaction == null)
			{
				connection.Close();
				logger.Debug("Closed connection to \"" + GetConnectionStringForLogging() + "\"");
			}
		}

		protected string GetConnectionStringForLogging()
		{
			Regex rpwd = new Regex("(?<key>.*[Pp]ass.*?)=.*");
			ArrayList partList = new ArrayList();
			foreach(string part in connection.ConnectionString.Split(';'))
			{
				Match pwdmatch = rpwd.Match(part);
				if(pwdmatch.Success)
					partList.Add(pwdmatch.Result("${key}") + "=******");
				else
					partList.Add(part);
			}
			return String.Join(";", (string[])partList.ToArray(typeof(string)));
		}
		

		//--------------------------------------------------------------------------------------
		//	Explicit transactions
		//--------------------------------------------------------------------------------------

		public virtual void BeginTransaction()
		{
			EnsureOpen();
			transaction = connection.BeginTransaction();
			
			processedRows = new ArrayList();
		}

		public virtual void CommitTransaction()
		{
			transaction.Commit();
			transaction = null;
			Close();
			
			foreach(DataRow r in processedRows)
				r.AcceptChanges();
			processedRows = null;
		}

		public virtual void RollbackTransaction()
		{
			if(transaction != null)
				transaction.Rollback();
			transaction = null;
			Close();

			processedRows = null;
		}

		
		//--------------------------------------------------------------------------------------
		//	IDataStore impl (fetching rows)
		//--------------------------------------------------------------------------------------

		public DataTable FetchRows(IFetchSpecification fetchSpec)
		{
			DataSet ds = new DataSet();
			ds.EnforceConstraints = false;
			fetchSpec.EntityMap.UpdateSchemaInDataSet(ds, SchemaUpdate.Basic | SchemaUpdate.Relations);
			DataTable table = ds.Tables[fetchSpec.EntityMap.TableName];

			StringBuilder builder = new StringBuilder();

			builder.Append("SELECT ");

			if(fetchSpec.FetchLimit != -1)
			{
				builder.Append("TOP ");
				builder.Append(fetchSpec.FetchLimit);
				builder.Append(" ");
			}

			WriteColumns(builder, table, table.Columns);

			builder.Append(" FROM ");
			builder.Append(table.TableName);

			Qualifier qualifier = fetchSpec.Qualifier; 

			if(qualifier != null)
			{
				builder.Append(" WHERE ");

				ArrayList parameters = new ArrayList();
				WriteQualifier(table, fetchSpec.EntityMap, qualifier, builder, parameters);

				FillTable(table, builder.ToString(), parameters);
			}
			else
			{
				FillTable(table, builder.ToString(), null);
			}

			return table;
		}

		public void FillTable(DataTable table, string sqlCommand, IList parameters)
		{
			SqlCommand cmd = CreateCommand(sqlCommand);

			if(parameters != null)
			{
				foreach(SqlParameter param in parameters)
					cmd.Parameters.Add(param);
			}

			EnsureOpen();

			if(logger.IsDebugEnabled) 
			{				
				logger.Debug(cmd.CommandText);
				foreach(SqlParameter p in cmd.Parameters)
					logger.Debug("    " + p.ParameterName + " = " + p.Value);
			}

			SqlDataAdapter adapter = new SqlDataAdapter();
			adapter.SelectCommand = cmd;
			adapter.Fill(table);

			if(logger.IsDebugEnabled) 
				logger.Debug(String.Format("returned {0} rows", table.Rows.Count));
			
			Close();
		}

		
		//--------------------------------------------------------------------------------------

		protected virtual void WriteQualifier(DataTable table, IEntityMap emap, Qualifier q, StringBuilder builder, IList parameters)
		{
			if(q is ColumnQualifier)
			{
				WriteColumnQualifier(table, emap, (ColumnQualifier)q, builder, parameters);
			}
			else if(q is PropertyQualifier)
			{
				q = new ColumnQualifier((PropertyQualifier)q, emap);
				WriteColumnQualifier(table, emap, (ColumnQualifier)q, builder, parameters);
			}
			else if(q is ClauseQualifier)
			{
				WriteClauseQualifier(table, emap, (ClauseQualifier)q, builder, parameters);
			}
			else if(q is PathQualifier)
			{
				WritePathQualifier(table, emap, (PathQualifier)q, 0, builder, parameters);
			}
			else
			{
				throw new ArgumentException(String.Format("Invalid qualifier class {0}", q.GetType()));
			}
		}


		protected virtual void WriteColumnQualifier(DataTable table, IEntityMap emap, ColumnQualifier q, StringBuilder builder, IList parameters)
		{
			builder.Append(q.Column);

			if(q.Predicate.Value == DBNull.Value)
			{
				if(q.Predicate is EqualsPredicate)
					builder.Append(" IS NULL ");
				else if(q.Predicate is NotEqualPredicate)
					builder.Append(" IS NOT NULL ");
				else
					throw new ArgumentException("Invalid predicate with null value; found " + q.Predicate.GetType().FullName);
			}
			else
			{
				if(q.Predicate is EqualsPredicate)
					builder.Append("=");
				else if(q.Predicate is NotEqualPredicate)
					builder.Append("<>");
				else if(q.Predicate is LessThanPredicate)
					builder.Append("<");
				else if(q.Predicate is LessOrEqualPredicate)
					builder.Append("<=");
				else if(q.Predicate is GreaterThanPredicate)
					builder.Append(">");
				else if(q.Predicate is GreaterOrEqualPredicate)
					builder.Append(">=");
				else if(q.Predicate is LikePredicate)
					builder.Append(" LIKE ");
				else
					throw new ArgumentException("Invalid predicate in qualifier; found " + q.Predicate.GetType().FullName);
				
				// add the current parameter count as suffix to ensure names are unique
				SqlParameter param = GetParameter(table.Columns[q.Column], parameters.Count.ToString(), q.Predicate.Value);
				builder.Append(param.ParameterName);
				parameters.Add(param);
			}
		}


		protected virtual void WriteClauseQualifier(DataTable table, IEntityMap emap, ClauseQualifier q, StringBuilder builder, IList parameters)
		{
			string conjunctor;

			if(q is AndQualifier)
				conjunctor = " AND ";
			else if(q is OrQualifier)
				conjunctor = " OR ";
			else
				throw new ArgumentException("Invalid conjunctor qualifier; found " + q.GetType().FullName);

			bool isFirstChild = true;
			builder.Append("(");
			foreach(Qualifier child in q.Qualifiers)
			{
				if(isFirstChild == false)
					builder.Append(conjunctor);
				isFirstChild = false;
				WriteQualifier(table, emap, child, builder, parameters);
			}
			builder.Append(")");
		}


		protected virtual void WritePathQualifier(DataTable table, IEntityMap emap, PathQualifier q, int idx, StringBuilder builder, IList parameters)
		{
			PropertyInfo	propInfo;
			FieldInfo		fieldInfo;
			Type			destType;
			IEntityMap		leftEmap, rightEmap, newEmap;
			DataRelation	rel;

			if((propInfo = emap.ObjectType.GetProperty(q.PathElements[idx])) != null)
				destType = propInfo.PropertyType;
			else if((fieldInfo = emap.ObjectType.GetField(q.PathElements[idx])) != null)
				destType = fieldInfo.FieldType;
			else
				throw new InvalidPropertyException(String.Format("{0} is not a valid property/field for class {1}", q.PathElements[idx], emap.ObjectType), null);

			if(typeof(ObjectCollectionBase).IsAssignableFrom(destType))
			{
				destType = destType.GetProperty("Item").PropertyType;
				leftEmap = emap;
				rightEmap = newEmap = emap.Factory.GetMap(destType);
			}
			else
			{
				leftEmap = newEmap = emap.Factory.GetMap(destType);
				rightEmap = emap;
			}

			if((rel = table.DataSet.Relations[leftEmap.TableName + "." + rightEmap.TableName]) == null)
				throw new NeoException("Can't to convert write PathQualifier; did not find relation " + leftEmap.TableName + "." + rightEmap.TableName);

			builder.Append((emap == rightEmap) ? rel.ChildColumns[0].ColumnName : rel.ParentColumns[0].ColumnName);
			builder.Append(" IN ( SELECT ");
			builder.Append((emap == rightEmap) ? rel.ParentColumns[0].ColumnName : rel.ChildColumns[0].ColumnName);
			builder.Append(" FROM ");
			builder.Append(newEmap.TableName);
			builder.Append(" WHERE ");

			newEmap.UpdateSchemaInDataSet(table.DataSet, SchemaUpdate.Basic | SchemaUpdate.Relations);
			if(idx < q.PathElements.Length - 1)
				WritePathQualifier(table.DataSet.Tables[newEmap.TableName], newEmap, q, idx + 1, builder, parameters);
			else
				WriteQualifier(table.DataSet.Tables[newEmap.TableName], newEmap, q.Qualifier, builder, parameters);

			builder.Append(" )");
		}


		//--------------------------------------------------------------------------------------
		//	IDataStore impl (saving changes)
		//--------------------------------------------------------------------------------------

		public ICollection SaveChangesInObjectContext(ObjectContext context)
		{
			ArrayList pkChangeTableList = new ArrayList();

			try
			{
				EnsureOpen();
				
				BeginTransaction();

				ArrayList parentFirstTables = new ArrayList();

				foreach(DataTable t in context.DataSet.Tables)
				{
					parentFirstTables.Add(new OrderTable(t));
				}
				
				AssignOrderToInsertTables(parentFirstTables);
				SortOrderOfInsertTables(parentFirstTables);

				ProcessInserts(parentFirstTables, pkChangeTableList);
				ProcessUpdates(parentFirstTables);

				ArrayList childFirstTables = new ArrayList(parentFirstTables);
				childFirstTables.Reverse();
				ProcessDeletes(childFirstTables);

				StringBuilder errorString = new StringBuilder();
				foreach(DataTable t in context.DataSet.Tables)
				{
					if(t.HasErrors)
					{
						errorString.Append(String.Format("\n{0}:\n", t.TableName));
						foreach(DataRow row in t.GetErrors())
							errorString.Append(String.Format("{0}\n", row.RowError));
					}
				}
				if(errorString.Length > 0)
					throw new DataStoreSaveException("Errors while saving: " + errorString.ToString());
				CommitTransaction();
			}
			catch(Exception e)
			{
				RollbackTransaction();
				if(e is DataStoreSaveException)
					throw e;
				throw new DataStoreSaveException(e);
			}
			finally
			{
				Close();
			}
			return pkChangeTableList;

		}

		public void ProcessInserts(ArrayList tables, ArrayList pkChangeTables)
		{
			for(int ix = 0; ix < tables.Count; ix++)
			{
				PkChangeTable	pkChangeTable;
				DataRow[]		modrows;
				DataTable table = ((OrderTable) tables[ix]).Table;

				pkChangeTable = new PkChangeTable(table.TableName);
				modrows = table.Select("", "", DataViewRowState.Added);
				foreach(DataRow row in modrows)
				{
					InsertRow(row, pkChangeTable);
				}
				if (pkChangeTable.Count > 0) 
				{
					pkChangeTables.Add(pkChangeTable);
				}
			}
		}

		public void ProcessDeletes(ArrayList tables)
		{
			for(int ix = 0; ix < tables.Count; ix++)
			{
				DataRow[]		modrows;
				DataTable table = ((OrderTable) tables[ix]).Table;

				modrows = table.Select("", "", DataViewRowState.Deleted);
				foreach(DataRow row in modrows)
				{
					DeleteRow(row);
				}
			}
		}

		public void ProcessUpdates(ArrayList tables)
		{
			for(int ix = 0; ix < tables.Count; ix++)
			{
				DataRow[]		modrows;
				DataTable table = ((OrderTable) tables[ix]).Table;

				modrows = table.Select("", "", DataViewRowState.ModifiedCurrent);
				foreach(DataRow row in modrows)
				{
					UpdateRow(row);
				}
			}
		}

		public void SortOrderOfInsertTables(ArrayList tables)
		{
			bool changesToOrder;
			// sort of order
			changesToOrder = true;
			
			while(changesToOrder) 
			{
				changesToOrder = false;
				for(int ix = 0; ix < tables.Count; ix++)
				{
					if (ix < tables.Count -1) 
					{
						OrderTable t1 = (OrderTable) tables[ix];
						OrderTable t2 = (OrderTable) tables[ix +1];
						if (t1.Order < t2.Order)
						{
							tables[ix] = t2;
							tables[ix+1] = t1;
							changesToOrder = true;
						}
					}
				}
			}
		}

		public void AssignOrderToInsertTables(ArrayList tables)
		{
			bool changesToOrder = true;
			while (changesToOrder) 
			{
				changesToOrder = false;
				foreach(OrderTable t in tables)
				{
					//how many parents
					DataRelationCollection parents = t.Table.ParentRelations;
					string tableN = t.Table.TableName;

					foreach(OrderTable t2 in tables)
					{
						if (t.IsChildOf(t2) && t2.Order <= t.Order && t.Table.TableName != t2.Table.TableName) 
						{
							t2.Order++;
							changesToOrder = true;
						}
					}
				}
			}
		}

		//--------------------------------------------------------------------------------------

		protected virtual void InsertRow(DataRow row, PkChangeTable pkChangeTable)
		{
			ArrayList		columnList;
			StringBuilder	builder;
			DataColumn		pkColumn;
			bool			usesIdentityColumn;
			int				rowsAffected;

			pkColumn = row.Table.PrimaryKey[0];
			usesIdentityColumn = ((row.Table.PrimaryKey.Length == 1) && (pkColumn.AutoIncrement));
			columnList = new ArrayList();
			foreach(DataColumn column in row.Table.Columns)
			{
				if((usesIdentityColumn == false) || (column != pkColumn))
					columnList.Add(column);
			}

			builder = new StringBuilder();
			builder.Append("INSERT INTO ");
			builder.Append(row.Table.TableName);
			builder.Append(" (");
			WriteColumns(builder, row.Table, columnList);
			builder.Append(") VALUES (");
			WriteParameters(builder, row.Table, "", columnList);
			builder.Append(")");

			ArrayList parameters = new ArrayList();
			
			foreach(DataColumn column in row.Table.Columns)
				parameters.Add(GetParameter(column, "", row[column]));

			rowsAffected = ExecuteNonQuery(builder.ToString(), parameters);

			if((usesIdentityColumn) && (rowsAffected == 1))
			{
				object	   finalPkValue;

				// SELECT _SCOPE_IDENTITY AS NEW_ID in SQL8 ...
				finalPkValue = ExecuteScalar("SELECT @@IDENTITY AS NEW_ID", null);
				pkChangeTable.AddPkChange(row[pkColumn], finalPkValue);
				row[pkColumn] = finalPkValue;
			}

			PostProcessRow(row, (rowsAffected == 1), "Failed to insert row into database.");
		}


		protected virtual void DeleteRow(DataRow row)
		{
			StringBuilder	builder;
			int				rowsAffected;

			builder = new StringBuilder();

			builder.Append("DELETE FROM ");
			builder.Append(row.Table.TableName);
			builder.Append(" WHERE");

			WriteOptimisticLockMatch(builder, row.Table, "");

			ArrayList parameters = new ArrayList();

			foreach(DataColumn column in row.Table.Columns)
				parameters.Add(GetParameter(column, "", row[column, DataRowVersion.Original]));
			
			rowsAffected = ExecuteNonQuery(builder.ToString(), parameters);
			PostProcessRow(row, (rowsAffected == 1), "Failed to delete row in database.");
		}


		protected virtual void UpdateRow(DataRow row)
		{
			StringBuilder	builder;
			int				rowsAffected;

			builder = new StringBuilder();

			builder.Append("UPDATE ");
			builder.Append(row.Table.TableName);
			builder.Append(" SET");

			WriteAllColumnsAndParameters(builder, row.Table, "", ", ");

			builder.Append(" WHERE");

			WriteOptimisticLockMatch(builder, row.Table, "_ORIG");

			ArrayList parameters = new ArrayList();

			foreach(DataColumn column in row.Table.Columns)
			{
				parameters.Add(GetParameter(column, "", row[column]));
				parameters.Add(GetParameter(column, "_ORIG", row[column, DataRowVersion.Original]));
			}

			rowsAffected = ExecuteNonQuery(builder.ToString(), parameters);
			PostProcessRow(row, (rowsAffected == 1), "Failed to update row in database.");
		}


		//--------------------------------------------------------------------------------------
		//	Other public methods
		//--------------------------------------------------------------------------------------

		public virtual void ClearTable(string tableName)
		{
			StringBuilder	builder;
			int				rowsAffected;

			builder = new StringBuilder();
			builder.Append("DELETE FROM ");
			builder.Append(tableName);
			
			rowsAffected = ExecuteNonQuery(builder.ToString(), null);
		}


		//--------------------------------------------------------------------------------------
		//	Internal helper
		//--------------------------------------------------------------------------------------

		protected virtual SqlCommand CreateCommand(string text)
		{
			SqlCommand command;

			if(transaction == null)
				command = CreateCommand(text, connection, null);
			else
				command = CreateCommand(text, connection, transaction);
			return command;	
		}


		protected virtual SqlCommand CreateCommand(string text, SqlConnection conn, SqlTransaction txn)
		{
			return new SqlCommand(text, conn, txn);
		}



		//--------------------------------------------------------------------------------------

		protected virtual int ExecuteNonQuery(string sqlCommand, IList parameters)
		{
			int rowsAffected;

			SqlCommand cmd = CreateCommand(sqlCommand);

			if(parameters != null)
			{
				foreach(SqlParameter parameter in parameters)
					cmd.Parameters.Add(parameter);
			}

			if(logger.IsDebugEnabled) 
			{
				logger.Debug(cmd.CommandText);
				foreach(SqlParameter p in cmd.Parameters)
					logger.Debug("    " + p.ParameterName + " = " + p.Value);
			}

			EnsureOpen();

			rowsAffected = cmd.ExecuteNonQuery();

			Close();
			
			if(logger.IsDebugEnabled) 
				logger.Debug(String.Format("{0} rows affected", rowsAffected));
			
			return rowsAffected;
		}


		protected virtual object ExecuteScalar(string sqlCommand, IDictionary parameters)
		{
			SqlCommand cmd = CreateCommand(sqlCommand);

			if(parameters != null)
			{
				foreach(DictionaryEntry entry in parameters)
					SetParameter(cmd, (DataColumn)entry.Key, "", entry.Value);
			}

			if(logger.IsDebugEnabled) 
				logger.Debug(cmd.CommandText);

			object result;

			if(logger.IsDebugEnabled) 
				logger.Debug(cmd.CommandText);

			EnsureOpen();

			result = cmd.ExecuteScalar();

			Close();

			if(logger.IsDebugEnabled) 
				logger.Debug(String.Format("result = {0}", result));

			return result;
		}

		
		//--------------------------------------------------------------------------------------

		protected virtual void PostProcessRow(DataRow row, bool succeeded, string errorMessage)
		{
			if(succeeded)
			{
				if(transaction != null)
					processedRows.Add(row);
				else
					row.AcceptChanges();
			}
			else
				row.RowError = errorMessage;
		}

		
		//--------------------------------------------------------------------------------------

		protected void WriteColumns(StringBuilder builder, DataTable table, ICollection columns)
		{
			bool first = true;

			foreach(DataColumn c in columns)
			{
				if(first == false)
					builder.Append(", ");
				builder.Append(c.ColumnName);
				first = false;
			}
		}


		protected void WriteParameters(StringBuilder builder, DataTable table, string suffix, ICollection columns)
		{
			bool first = true;

			foreach(DataColumn c in columns)
			{
				if(first == false)
					builder.Append(", ");
				builder.Append(ConvertToParameterName(c.ColumnName, suffix));
				first = false;
			}
		}

		
		protected void WriteAllColumnsAndParameters(StringBuilder builder, DataTable table, string suffix, string separator)
		{
			bool firstColumn = true;

			for(int i = 0; i < table.Columns.Count; i++)
			{
				// Workaround for update with autoincrement bug
				if(table.Columns[i].AutoIncrement)
					continue;

				if(firstColumn == false)
					builder.Append(separator);
				firstColumn = false;

				builder.Append(" ");
				builder.Append(table.Columns[i].ColumnName);
				builder.Append(" = ");
				builder.Append(ConvertToParameterName(table.Columns[i].ColumnName, suffix));
			}
		}

		
		protected void WriteOptimisticLockMatch(StringBuilder builder, DataTable table, string suffix)
		{
			// This is not really correct, we should exclude BLOBs. Then again, we don't do BLOBs yet.
			for(int i = 0; i < table.Columns.Count; i++)
			{
				if(i > 0)
					builder.Append(" AND");
				builder.Append(" ((");
				builder.Append(table.Columns[i].ColumnName);
				builder.Append(" = ");
				builder.Append(ConvertToParameterName(table.Columns[i].ColumnName, suffix));
				builder.Append(" ) OR ((");
				builder.Append(table.Columns[i].ColumnName);
				builder.Append(" IS NULL) AND (");
				builder.Append(ConvertToParameterName(table.Columns[i].ColumnName, suffix));
				builder.Append(" IS NULL)))");
			}
		}


		//--------------------------------------------------------------------------------------

		protected virtual SqlParameter SetParameter(SqlCommand command, DataColumn pcolumn, string suffix, object pvalue)
		{
			SqlDbType	 dbtype;
			SqlParameter param;

			if((dbtype = ConvertToDbType(pcolumn.DataType)) == SqlDbType.Variant)
				throw new ArgumentException(String.Format("Cannot handle data type {0} of column {1} in table {2}.", pcolumn.DataType.ToString(), pcolumn.ColumnName, pcolumn.Table.TableName));
			param = command.Parameters.Add(ConvertToParameterName(pcolumn.ColumnName, suffix), dbtype, Math.Max(0, pcolumn.MaxLength), pcolumn.ColumnName);
			param.IsNullable = pcolumn.AllowDBNull;
			param.Value = pvalue;
			return param;
		}

		protected virtual SqlParameter GetParameter(DataColumn pcolumn, string suffix, object pvalue)
		{
			SqlDbType	 dbtype;
			SqlParameter param;

			if((dbtype = ConvertToDbType(pcolumn.DataType)) == SqlDbType.Variant)
				throw new ArgumentException(String.Format("Cannot handle data type {0} of column {1} in table {2}.", pcolumn.DataType.ToString(), pcolumn.ColumnName, pcolumn.Table.TableName));
			param = new SqlParameter(ConvertToParameterName(pcolumn.ColumnName, suffix), dbtype, Math.Max(0, pcolumn.MaxLength), pcolumn.ColumnName);
			param.IsNullable = pcolumn.AllowDBNull;
			param.Value = pvalue;
			return param;
		}


		//--------------------------------------------------------------------------------------
	
		protected virtual string ConvertToParameterName(string column, string suffix)
		{
			string paramName = column+suffix;
			paramName = paramName.Replace(@"\", "_");
			paramName = paramName.Replace(@"/", "_");
			paramName = paramName.Replace(@"'", "_");
			paramName = paramName.Replace(@"=", "_");
			paramName = paramName.Replace("\"", "_");
			paramName = paramName.Replace(@"-", "_");
			paramName = paramName.Replace(@" ", "_");
			return "@" + paramName;
		}


		private static Hashtable dbTypeTable;

		protected virtual SqlDbType ConvertToDbType(Type t)
		{
			if(dbTypeTable == null)
			{
				dbTypeTable = new Hashtable();
				dbTypeTable.Add(typeof(System.Boolean),  SqlDbType.Bit);
				dbTypeTable.Add(typeof(System.Int16),	 SqlDbType.SmallInt);
				dbTypeTable.Add(typeof(System.Int32),	 SqlDbType.Int);
				dbTypeTable.Add(typeof(System.Int64),	 SqlDbType.BigInt);
				dbTypeTable.Add(typeof(System.Double),	 SqlDbType.Float);
				dbTypeTable.Add(typeof(System.Decimal),	 SqlDbType.Decimal);
				dbTypeTable.Add(typeof(System.String),	 SqlDbType.VarChar);
				dbTypeTable.Add(typeof(System.DateTime), SqlDbType.DateTime);
				dbTypeTable.Add(typeof(System.Byte[]),	 SqlDbType.VarBinary);
				dbTypeTable.Add(typeof(System.Guid),	 SqlDbType.UniqueIdentifier);
			}

			object entry = dbTypeTable[t];
			return (entry != null) ? (SqlDbType)entry : SqlDbType.Variant;	
		}

	}


}
