using System;
using System.Collections;
using System.Data;
using Neo.Database;

using Npgsql;
using NpgsqlTypes;

namespace Neo.PostgresqlClient
{
	public class PostgresqlImplFactory : IDbImplementationFactory
	{

		//--------------------------------------------------------------------------------------
		//	IDbImplementationFactory impl
		//--------------------------------------------------------------------------------------
		
		public IDbConnection CreateConnection(string connectionString)
		{
			return new NpgsqlConnection(connectionString);
		}

		public IDbCommand CreateCommand()
		{
			return new NpgsqlCommand();
		}

		public IDbDataAdapter CreateDataAdapter()
		{
			return new NpgsqlDataAdapter();
		}

		public IDataParameter CreateParameter(DataColumn pcolumn, string pname, object pvalue)
		{
			NpgsqlDbType	 dbtype;
			NpgsqlParameter param;

			if((dbtype = ConvertToDbType(pcolumn.DataType)) == NpgsqlDbType.Text)
				throw new ArgumentException(String.Format("Cannot handle data type {0} of column {1} in table {2}.", pcolumn.DataType.ToString(), pcolumn.ColumnName, pcolumn.Table.TableName));
			param = new NpgsqlParameter(pname, dbtype, Math.Max(0, pcolumn.MaxLength), pcolumn.ColumnName);
			param.IsNullable = pcolumn.AllowDBNull;
			if((pvalue == null) || ((pcolumn.AllowDBNull) && (pvalue is String) && (((string)pvalue).Length == 0))) 
			{
				param.Value = DBNull.Value;
			} 
			else 
			{
				param.Value = pvalue;			
			}
			return param;
		}
	
		private static Hashtable dbTypeTable;

		private NpgsqlDbType ConvertToDbType(Type t)
		{
			if(dbTypeTable == null)
			{
				dbTypeTable = new Hashtable();
				dbTypeTable.Add(typeof(Boolean),  NpgsqlDbType.Bytea);
				dbTypeTable.Add(typeof(Int16),	 NpgsqlDbType.Smallint);
				dbTypeTable.Add(typeof(Int32),	 NpgsqlDbType.Integer);
				dbTypeTable.Add(typeof(Int64),	 NpgsqlDbType.Bigint);
				dbTypeTable.Add(typeof(Double),	 NpgsqlDbType.Double);
				dbTypeTable.Add(typeof(Decimal),	 NpgsqlDbType.Numeric);
				dbTypeTable.Add(typeof(String),	 NpgsqlDbType.Varchar);
				dbTypeTable.Add(typeof(DateTime), NpgsqlDbType.Date);
				dbTypeTable.Add(typeof(Byte[]),	 NpgsqlDbType.Text);
			}

			object entry = dbTypeTable[t];
			return (entry != null) ? (NpgsqlDbType)entry : NpgsqlDbType.Text;	
		}


		public IDbCommandBuilder CreateCommandBuilder(DataTable table)
		{
			return new PostgresqlCommandBuilder(table, this);
		}

	}
}
