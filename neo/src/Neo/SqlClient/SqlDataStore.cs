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
using Neo.Database;


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
				logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
		}
	

		//--------------------------------------------------------------------------------------
		//	Fields and constructor
		//--------------------------------------------------------------------------------------

		private SqlConnection		connection;
		private SqlTransaction		transaction;
		private SqlParameterFactory	parameterFactory;
		private ArrayList			processedRows;

		public SqlDataStore() : this(null)
		{
		}

		public SqlDataStore(string connectionString)
		{
			logger.Debug("Created new SqlDataStore.");

			parameterFactory = new SqlParameterFactory();

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

		    GenericSqlSelectWriter writer = new GenericSqlSelectWriter(table, parameterFactory);
			writer.WriteSelect(fetchSpec);

			FillTable(table, writer.Command, writer.Parameters);
	
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
			ArrayList			columnList;
			GenericSqlModWriter	writer;
			DataColumn			pkColumn;
			bool				usesIdentityColumn;
			int					rowsAffected;
	
			pkColumn = row.Table.PrimaryKey[0];
			usesIdentityColumn = ((row.Table.PrimaryKey.Length == 1) && (pkColumn.AutoIncrement));
			columnList = new ArrayList();
			foreach(DataColumn column in row.Table.Columns)
			{
				if((usesIdentityColumn == false) || (column != pkColumn))
					columnList.Add(column);
			}

			writer = new GenericSqlModWriter(row.Table, parameterFactory);
			writer.WriteInsert(row, columnList);
	
			rowsAffected = ExecuteNonQuery(writer.Command, writer.Parameters);

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
			GenericSqlModWriter	writer;
			int					rowsAffected;

			writer = new GenericSqlModWriter(row.Table, parameterFactory);
			writer.WriteDelete(row);
			
			rowsAffected = ExecuteNonQuery(writer.Command, writer.Parameters);
			PostProcessRow(row, (rowsAffected == 1), "Failed to delete row in database.");
		}


		protected virtual void UpdateRow(DataRow row)
		{
			GenericSqlModWriter	writer;
			int					rowsAffected;

			writer = new GenericSqlModWriter(row.Table, parameterFactory);
			writer.WriteUpdate(row);

			rowsAffected = ExecuteNonQuery(writer.Command, writer.Parameters);
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
					cmd.Parameters.Add(parameterFactory.CreateParameter((DataColumn)entry.Key, "", entry.Value));
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


	}


}
