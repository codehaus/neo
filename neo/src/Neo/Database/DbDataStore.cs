using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using log4net;
using Neo.Core;


namespace Neo.Database
{
	[Serializable]
	public abstract class DbDataStore : IDataStore, ISerializable
	{
		//--------------------------------------------------------------------------------------
		//	Static fields and constructor
		//--------------------------------------------------------------------------------------

		protected static ILog logger = null;
	

		//--------------------------------------------------------------------------------------
		//	Fields and constructor
		//--------------------------------------------------------------------------------------

		protected IDbImplementationFactory	implFactory;
		protected IDbConnection				connection;
		protected IDbTransaction			transaction;
		protected ArrayList					processedRows;

		public DbDataStore()
		{
			if(logger == null)
				logger = LogManager.GetLogger(this.GetType().FullName);
		}


		//--------------------------------------------------------------------------------------
		//	Serialisation support
		//--------------------------------------------------------------------------------------

		protected DbDataStore(SerializationInfo info, StreamingContext context) : this()
		{
			Type fType = (Type)info.GetValue("implFactoryType", typeof(Type));
			implFactory = (IDbImplementationFactory)Activator.CreateInstance(fType);
			connection = implFactory.CreateConnection(info.GetString("connectionString"));
		}

		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("connectionString", connection.ConnectionString);
			info.AddValue("implFactoryType", implFactory.GetType());
		}

	
		//--------------------------------------------------------------------------------------
		//	Opening and closing the connection
		//--------------------------------------------------------------------------------------

		protected virtual void EnsureOpen()
		{
			if(connection.State != ConnectionState.Open)
			{
				if(logger.IsDebugEnabled) 
					logger.Debug("Opening connection to \"" + GetConnectionStringForLogging() + "\"");
				connection.Open();
			}
		}

		protected virtual void Close()
		{
			if(connection.State != ConnectionState.Closed && transaction == null)
			{
				connection.Close();
				if(logger.IsDebugEnabled) 
					logger.Debug("Closed connection to \"" + GetConnectionStringForLogging() + "\"");
			}
		}

		protected virtual string GetConnectionStringForLogging()
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
		
		public virtual DataTable FetchRows(IFetchSpecification fetchSpec)
		{
			DataSet ds = new DataSet();
			ds.EnforceConstraints = false;
			fetchSpec.EntityMap.UpdateSchemaInDataSet(ds, SchemaUpdate.Basic | SchemaUpdate.Relations);
			DataTable table = ds.Tables[fetchSpec.EntityMap.TableName];

		    IDbCommandBuilder builder = GetCommandBuilder(table);
		    builder.WriteSelect(fetchSpec);

			FillTable(table, builder.Command, builder.Parameters);
	
			return table;
		}


		public virtual void FillTable(DataTable table, string sqlCommand, IList parameters)
		{
			IDbCommand cmd = CreateCommand(sqlCommand);

			if(parameters != null)
			{
				foreach(IDataParameter param in parameters)
					cmd.Parameters.Add(param);
			}

			EnsureOpen();

			if(logger.IsDebugEnabled) 
			{				
				logger.Debug(cmd.CommandText);
				foreach(IDataParameter p in cmd.Parameters)
					logger.Debug("    " + p.ParameterName + " = " + p.Value);
			}

			IDbDataAdapter adapter = implFactory.CreateDataAdapter();
			adapter.SelectCommand = cmd;
			((DbDataAdapter)adapter).Fill(table);

			if(logger.IsDebugEnabled) 
				logger.Debug(String.Format("returned {0} rows", table.Rows.Count));
			
			Close();
		}

		
		//--------------------------------------------------------------------------------------
		//	IDataStore impl (saving changes)
		//--------------------------------------------------------------------------------------

		public virtual ICollection SaveChangesInObjectContext(ObjectContext context)
		{
			ArrayList pkChangeTableList;
			bool	  requiredTransaction;
			
			pkChangeTableList = new ArrayList();
			
			EnsureOpen();
			requiredTransaction = (transaction == null);
			if(requiredTransaction)
				BeginTransaction();

			try
			{
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
				if(requiredTransaction)
					CommitTransaction();
			}
			catch(Exception e)
			{
				if(requiredTransaction)
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

		public virtual void ProcessInserts(ArrayList tables, ArrayList pkChangeTables)
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

		public virtual void ProcessDeletes(ArrayList tables)
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

		public virtual void ProcessUpdates(ArrayList tables)
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

		public virtual void SortOrderOfInsertTables(ArrayList tables)
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

		public virtual void AssignOrderToInsertTables(ArrayList tables)
		{
			bool changesToOrder = true;
			while (changesToOrder) 
			{
				changesToOrder = false;
				foreach(OrderTable t in tables)
				{
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
		    IDbCommandBuilder	builder;
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

		    builder = GetCommandBuilder(row.Table);
		    builder.WriteInsert(row, columnList);
	
			rowsAffected = ExecuteNonQuery(builder.Command, builder.Parameters);

			if((usesIdentityColumn) && (rowsAffected == 1))
			{
				object finalPkValue = GetFinalPk(row, builder);
				pkChangeTable.AddPkChange(row[pkColumn], finalPkValue);
				row[pkColumn] = finalPkValue;
			}

			PostProcessRow(row, (rowsAffected == 1), "Failed to insert row into database.");
		}


		protected virtual void DeleteRow(DataRow row)
		{
		    IDbCommandBuilder	builder;
			int					rowsAffected;

		    builder = GetCommandBuilder(row.Table);
		    builder.WriteDelete(row);
			
			rowsAffected = ExecuteNonQuery(builder.Command, builder.Parameters);
			PostProcessRow(row, (rowsAffected == 1), "Failed to delete row in database.");
		}


		protected virtual void UpdateRow(DataRow row)
		{
		    IDbCommandBuilder	builder;
			int					rowsAffected;

		    builder = GetCommandBuilder(row.Table);
		    builder.WriteUpdate(row);

			rowsAffected = ExecuteNonQuery(builder.Command, builder.Parameters);
			PostProcessRow(row, (rowsAffected == 1), "Failed to update row in database.");
		}


		//--------------------------------------------------------------------------------------
		//	Internal helper
		//--------------------------------------------------------------------------------------

		protected virtual IDbCommandBuilder GetCommandBuilder(DataTable table)
		{
			return implFactory.CreateCommandBuilder(table);
		}


		protected virtual IDbCommand CreateCommand(string text) 
		{
			IDbCommand command = implFactory.CreateCommand();
			command.CommandText = text;
			command.Connection = connection;

			if(transaction != null)
				command.Transaction = transaction;

			return command;	
		}


		//--------------------------------------------------------------------------------------

		protected virtual int ExecuteNonQuery(string sqlCommand, IList parameters)
		{
			int rowsAffected;

			IDbCommand cmd = CreateCommand(sqlCommand);

			if(parameters != null)
			{
				foreach(IDataParameter parameter in parameters)
					cmd.Parameters.Add(parameter);
			}

			if(logger.IsDebugEnabled) 
			{
				logger.Debug(cmd.CommandText);
				foreach(IDataParameter p in cmd.Parameters)
					logger.Debug("    " + p.ParameterName + " = " + p.Value);
			}

			EnsureOpen();

			rowsAffected = cmd.ExecuteNonQuery();

			Close();
			
			if(logger.IsDebugEnabled) 
				logger.Debug(String.Format("{0} rows affected", rowsAffected));
			
			return rowsAffected;
		}


		protected virtual object ExecuteScalar(string sqlCommand, IList parameters)
		{
			IDbCommand  cmd;
			object		result;
			
			cmd = CreateCommand(sqlCommand);
			if(parameters != null)
			{
				foreach(DictionaryEntry entry in parameters)
					cmd.Parameters.Add(entry.Value);
			}

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
		//	Abstract methods
		//--------------------------------------------------------------------------------------

		protected abstract object GetFinalPk(DataRow row, IDbCommandBuilder builder);

	}
}
