using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using Neo.Database;


namespace Neo.SqlClient
{
	public class SqlImplFactory : IDbImplementationFactory
	{

		//--------------------------------------------------------------------------------------
		//	IDbImplementationFactory impl
		//--------------------------------------------------------------------------------------

		public IDbConnection CreateConnection(string connectionString)
		{
			return new SqlConnection(connectionString);
		}

		public IDbCommand CreateCommand()
		{
			return new SqlCommand();
		}

		public IDbDataAdapter CreateDataAdapter()
		{
			return new SqlDataAdapter();
		}

		public IDataParameter CreateParameter(DataColumn pcolumn, string pname, object pvalue)
		{
			SqlDbType	 dbtype;
			SqlParameter param;

			if((dbtype = ConvertToDbType(pcolumn.DataType)) == SqlDbType.Variant)
				throw new ArgumentException(String.Format("Cannot handle data type {0} of column {1} in table {2}.", pcolumn.DataType.ToString(), pcolumn.ColumnName, pcolumn.Table.TableName));
			param = new SqlParameter(pname, dbtype, Math.Max(0, pcolumn.MaxLength), pcolumn.ColumnName);
			param.IsNullable = pcolumn.AllowDBNull;
			param.Value = pvalue;
			return param;
		}
	
		private static Hashtable dbTypeTable;

		private SqlDbType ConvertToDbType(Type t)
		{
			if(dbTypeTable == null)
			{
				dbTypeTable = new Hashtable();
				dbTypeTable.Add(typeof(Boolean), SqlDbType.Bit);
				dbTypeTable.Add(typeof(Int16),	 SqlDbType.SmallInt);
				dbTypeTable.Add(typeof(Int32),	 SqlDbType.Int);
				dbTypeTable.Add(typeof(Int64),	 SqlDbType.BigInt);
				dbTypeTable.Add(typeof(Double),	 SqlDbType.Float);
				dbTypeTable.Add(typeof(Decimal), SqlDbType.Decimal);
				dbTypeTable.Add(typeof(String),	 SqlDbType.VarChar);
				dbTypeTable.Add(typeof(DateTime),SqlDbType.DateTime);
				dbTypeTable.Add(typeof(Byte[]),	 SqlDbType.VarBinary);
				dbTypeTable.Add(typeof(Guid),	 SqlDbType.UniqueIdentifier);
			}

			object entry = dbTypeTable[t];
			return (entry != null) ? (SqlDbType)entry : SqlDbType.Variant;	
		}


		public IDbCommandBuilder CreateCommandBuilder(DataTable table)
		{
			return new SqlCommandBuilder(table, this);
		}


	}
}
