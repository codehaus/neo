using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Foo;
using log4net;
using Neo.Core;
using Neo.Core.Qualifiers;
using Neo.Core.Util;


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

		protected IDbConnectionFactory		connectionFactory;
		protected IDbImplementationFactory	implFactory;
		protected IDbConnection				connection;
		protected IDbTransaction			transaction;
		protected ArrayList					processedRows;
        protected bool                      usesDelimitedIdentifiers;
		protected int						commandTimeout;

		public DbDataStore()
		{
			if(logger == null)
				logger = LogManager.GetLogger(this.GetType().FullName);
			commandTimeout = 30; // The default, according to the docs for IDbCommand
		}

		protected DbDataStore(IDbImplementationFactory implFactory) : this()
		{
			this.implFactory = implFactory;
		}

		protected DbDataStore(IDbImplementationFactory implFactory, IDbConnectionFactory connectionFactory) : this(implFactory)
		{
			this.connectionFactory = connectionFactory;
		}

		protected void FinishInitialization(String connectionString)
		{
			if(connectionFactory == null)
				connectionFactory = new DbImplementationFactoryAdaptor(implFactory, connectionString);
			if(connection == null)
				connection = connectionFactory.CreateConnection();
		}

		//--------------------------------------------------------------------------------------
		//	Serialisation support
		//--------------------------------------------------------------------------------------

		protected DbDataStore(SerializationInfo info, StreamingContext context) : this()
		{
			Type fType = (Type)info.GetValue("implFactoryType", typeof(Type));
			implFactory = (IDbImplementationFactory)Activator.CreateInstance(fType);
			connectionFactory = (IDbConnectionFactory)info.GetValue("connectionFactory", typeof(IDbConnectionFactory));
			connection = connectionFactory.CreateConnection();
            usesDelimitedIdentifiers = info.GetBoolean("usesDelimitedIdentifiers");
		}

		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("connectionFactory", connectionFactory);
			info.AddValue("implFactoryType", implFactory.GetType());
            info.AddValue("usesDelimitedIdentifiers", usesDelimitedIdentifiers);
		}

	
		//--------------------------------------------------------------------------------------
		//	Public properties
		//--------------------------------------------------------------------------------------
		
		public bool UsesDelimitedIdentifiers
		{
			get { return usesDelimitedIdentifiers; }
			set { usesDelimitedIdentifiers = value; }
		}

		public int CommandTimeout
		{
			get { return commandTimeout; }
			set { commandTimeout = value; }
		}

		public IDbTransaction Transaction
		{
			get { return transaction; }
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
		
		public virtual DataSet FetchRows(IFetchSpecification fetchSpec)
		{
			DataSet ds = new DataSet();
			ds.EnforceConstraints = false;
			if(fetchSpec.Spans == null)
				FillTable(ds, fetchSpec);
			else
				FillDataSet(ds, fetchSpec, fetchSpec.Spans);
			return ds;
		}

		protected void FillDataSet(DataSet ds, IFetchSpecification fetchSpec, string[] spans)
		{
			IEntityMap emap = fetchSpec.EntityMap;
			FillTable(ds, fetchSpec);
	
			foreach(String path in spans)
			{
				string[] pathElements = path.Split(new char[]{ '.' }, 2);

				RelationInfo info = emap.GetRelationInfo(pathElements[0]);
				IEntityMap other = (info.ParentEntity == emap) ? info.ChildEntity : info.ParentEntity;
				string inverseName = other.GetRelationName(info);

				Qualifier q = new PathQualifier(inverseName, fetchSpec.Qualifier);
				IFetchSpecification spanFetchSpec = new FetchSpecification(other, q);

				if(pathElements.Length == 1)
					FillTable(ds, spanFetchSpec);
				else
					FillDataSet(ds, spanFetchSpec, new string[]{ pathElements[1] });
			}
		}

		protected void FillTable(DataSet ds, IFetchSpecification fetchSpec)
		{
			fetchSpec.EntityMap.UpdateSchemaInDataSet(ds, SchemaUpdate.Basic | SchemaUpdate.Relations);
			DataTable table = ds.Tables[fetchSpec.EntityMap.TableName];
			IDbCommandBuilder builder = GetCommandBuilder(table);
			builder.WriteSelect(fetchSpec);
			FillTable(table, builder.Command, builder.Parameters);
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
				OrderedTableCollection tables = new OrderedTableCollection(context.DataSet);

				ProcessInserts(tables, pkChangeTableList);
				ProcessUpdates(tables);
				ProcessDeletes(tables);

				ArrayList errors = new ArrayList();
				foreach(DataTable t in context.DataSet.Tables)
				{
					if(t.HasErrors)
					{
						foreach(DataRow row in t.GetErrors())
							errors.Add(new DataStoreSaveException.ErrorInfo(ObjectId.GetObjectIdForRow(row), row.RowError));
					}
				}
				
				if(errors.Count > 0)
				{
					string message = "Multiple errors.";
					if(errors.Count == 1)
						message = ((DataStoreSaveException.ErrorInfo)errors[0]).Message;
					throw new DataStoreSaveException(message, (DataStoreSaveException.ErrorInfo[])errors.ToArray(typeof(DataStoreSaveException.ErrorInfo)));
				}
				
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

		public virtual void ProcessInserts(OrderedTableCollection tables, ArrayList pkChangeTables)
		{
			for(int ix = 0; ix < tables.Count; ix++)
			{
				ProcessInserts(tables[ix], pkChangeTables);
			}
		}

		public virtual void ProcessInserts(DataTable table, ArrayList pkChangeTables)
		{
			PkChangeTable	pkChangeTable;
			DataRow[]		modrows;
	
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

		public virtual void ProcessDeletes(OrderedTableCollection tables)
		{
			for(int ix = tables.Count - 1; ix >= 0; ix--)
			{
				ProcessDeletes(tables[ix]);
			}
		}

		public virtual void ProcessDeletes(DataTable table)
		{
			DataRow[]		modrows;
	
			modrows = table.Select("", "", DataViewRowState.Deleted);
			foreach(DataRow row in modrows)
			{
				DeleteRow(row);
			}
		}

		public virtual void ProcessUpdates(OrderedTableCollection tables)
		{
			for(int ix = 0; ix < tables.Count; ix++)
			{
				ProcessUpdates(tables[ix]);
			}
		}

		public virtual void ProcessUpdates(DataTable table)
		{
			DataRow[] modrows;
	
			modrows = table.Select("", "", DataViewRowState.ModifiedCurrent);
			foreach(DataRow row in modrows)
			{
				UpdateRow(row);
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

			try
			{
				rowsAffected = ExecuteNonQuery(builder.Command, builder.Parameters);

				if((usesIdentityColumn) && (rowsAffected == 1))
				{
					object finalPkValue = GetFinalPk(row, builder);
					pkChangeTable.AddPkChange(row[pkColumn], finalPkValue);
					row[pkColumn] = finalPkValue;
				}
			}
			catch(Exception e)
			{
				throw new DataStoreSaveException(e, ObjectId.GetObjectIdForRow(row));
			}

			PostProcessRow(row, (rowsAffected == 1), "Failed to insert row into database.");
		}


		protected virtual void DeleteRow(DataRow row)
		{
		    IDbCommandBuilder	builder;
			int					rowsAffected;

		    builder = GetCommandBuilder(row.Table);
		    builder.WriteDelete(row);
			
			try
			{
				rowsAffected = ExecuteNonQuery(builder.Command, builder.Parameters);
			}
			catch(Exception e)
			{
				throw new DataStoreSaveException(e, ObjectId.GetObjectIdForRow(row));
			}
			
			PostProcessRow(row, (rowsAffected == 1), "Failed to delete row in database: No rows affected. Most likely another process has updated the database since the object was fetched.");
		}


		protected virtual void UpdateRow(DataRow row)
		{
		    IDbCommandBuilder	builder;
			int					rowsAffected;

			// ADO.NET can flag rows that have no changes at all as modified.
			if(RowHasValueChanges(row) == false)
				return;
		  
			builder = GetCommandBuilder(row.Table);
		    builder.WriteUpdate(row);

			try
			{
				rowsAffected = ExecuteNonQuery(builder.Command, builder.Parameters);
			}
			catch(Exception e)
			{
				throw new DataStoreSaveException(e, ObjectId.GetObjectIdForRow(row));
			}

			PostProcessRow(row, (rowsAffected == 1), "Failed to delete row in database: No rows affected. Most likely another process has updated the database since the object was fetched.");
		}


		protected virtual bool RowHasValueChanges(DataRow row)
		{
			for(int i = 0; i < row.Table.Columns.Count; i++)
			{
				object currentValue = row[i, DataRowVersion.Current];
				object originalValue = row[i, DataRowVersion.Original];
				if(currentValue.Equals(originalValue) == false)
					return true;
			}
			return false;
		}


		//--------------------------------------------------------------------------------------
		//	Internal helper
		//--------------------------------------------------------------------------------------
		
		protected virtual IDbCommandBuilder GetCommandBuilder(DataTable table)
		{
		    IDbCommandBuilder commandBuilder = implFactory.CreateCommandBuilder(table);
            GenericSql92Builder sql92Builder = commandBuilder as GenericSql92Builder;
            if (sql92Builder != null)
            {
                sql92Builder.UsesDelimitedIdentifiers = usesDelimitedIdentifiers;   
            }
		    return commandBuilder;
		}


		protected virtual IDbCommand CreateCommand(string text) 
		{
			IDbCommand command = implFactory.CreateCommand();
			command.CommandText = text;
			command.Connection = connection;
			command.CommandTimeout = commandTimeout;

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
			{
				row.RowError = errorMessage;
			}
		}



		//--------------------------------------------------------------------------------------
		//	Abstract methods
		//--------------------------------------------------------------------------------------

		protected abstract object GetFinalPk(DataRow row, IDbCommandBuilder builder);

	}
}
