using System;
using System.Collections;
using System.Data;
using System.Data.OracleClient;
using Neo.Database;


namespace Neo.OracleClient
{
	public class OracleImplFactory : IDbImplementationFactory
	{

		//--------------------------------------------------------------------------------------
		//	IDbImplementationFactory impl
		//--------------------------------------------------------------------------------------
		
		public IDbConnection CreateConnection(string connectionString)
		{
			return new OracleConnection(connectionString);
		}

		public IDbCommand CreateCommand()
		{
			return new OracleCommand();
		}

		public IDbDataAdapter CreateDataAdapter()
		{
			return new OracleDataAdapter();
		}

		public IDataParameter CreateParameter(DataColumn pcolumn, string pname, object pvalue)
		{
			OracleType	 dbtype;
			OracleParameter param;

			if((dbtype = ConvertToDbType(pcolumn.DataType)) == OracleType.Blob)
				throw new ArgumentException(String.Format("Cannot handle data type {0} of column {1} in table {2}.", pcolumn.DataType.ToString(), pcolumn.ColumnName, pcolumn.Table.TableName));
			param = new OracleParameter(pname, dbtype, Math.Max(0, pcolumn.MaxLength), pcolumn.ColumnName);
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

		private OracleType ConvertToDbType(Type t)
		{
			if(dbTypeTable == null)
			{
				dbTypeTable = new Hashtable();
				dbTypeTable.Add(typeof(Boolean),  OracleType.Byte);
				dbTypeTable.Add(typeof(Int16),	 OracleType.Int16);
				dbTypeTable.Add(typeof(Int32),	 OracleType.Int32);
				dbTypeTable.Add(typeof(Int64),	 OracleType.Number);
				dbTypeTable.Add(typeof(Double),	 OracleType.Double);
				dbTypeTable.Add(typeof(Decimal),	 OracleType.Number);
				dbTypeTable.Add(typeof(String),	 OracleType.VarChar);
				dbTypeTable.Add(typeof(DateTime), OracleType.DateTime);
				dbTypeTable.Add(typeof(Byte[]),	 OracleType.Raw);
			}

			object entry = dbTypeTable[t];
			return (entry != null) ? (OracleType)entry : OracleType.Blob;	
		}


		public IDbCommandBuilder CreateCommandBuilder(DataTable table)
		{
			return new OracleCommandBuilder(table, this);
		}

	}
}
