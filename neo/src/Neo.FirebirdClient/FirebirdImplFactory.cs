using System;
using System.Collections;
using System.Data;
using Neo.Database;
using FirebirdSql.Data.Firebird;


namespace Neo.FirebirdClient
{
	public class FirebirdImplFactory : IDbImplementationFactory
	{

		//--------------------------------------------------------------------------------------
		//	IDbImplementationFactory impl
		//--------------------------------------------------------------------------------------

		public IDbConnection CreateConnection(string connectionString)
		{
			return new FbConnection(connectionString);
		}

		public IDbCommand CreateCommand()
		{
			return new FbCommand();
		}

		public IDbDataAdapter CreateDataAdapter()
		{
			return new FbDataAdapter();
		}

		public IDataParameter CreateParameter(DataColumn pcolumn, string pname, object pvalue)
		{
			FbDbType	 dbtype;
			FbParameter param;

			if((dbtype = ConvertToDbType(pcolumn.DataType)) == FbDbType.Binary)
				throw new ArgumentException(String.Format("Cannot handle data type {0} of column {1} in table {2}.", pcolumn.DataType.ToString(), pcolumn.ColumnName, pcolumn.Table.TableName));
			param = new FbParameter(pname, dbtype, Math.Max(0, pcolumn.MaxLength), pcolumn.ColumnName);
			param.IsNullable = pcolumn.AllowDBNull;
			param.Value = pvalue;
			return param;
		}
	
		private static Hashtable dbTypeTable;

		private FbDbType ConvertToDbType(Type t)
		{
			if(dbTypeTable == null)
			{
				dbTypeTable = new Hashtable();
				dbTypeTable.Add(typeof(Boolean), FbDbType.SmallInt);
				dbTypeTable.Add(typeof(Int16),	 FbDbType.SmallInt);
				dbTypeTable.Add(typeof(Int32),	 FbDbType.Integer);
				dbTypeTable.Add(typeof(Int64),	 FbDbType.BigInt);
				dbTypeTable.Add(typeof(Double),	 FbDbType.Double);
				dbTypeTable.Add(typeof(Decimal), FbDbType.Decimal);
				dbTypeTable.Add(typeof(String),	 FbDbType.VarChar);
				dbTypeTable.Add(typeof(DateTime),FbDbType.TimeStamp);
				dbTypeTable.Add(typeof(Byte[]),	 FbDbType.Binary);
			}

			object entry = dbTypeTable[t];
			return (entry != null) ? (FbDbType)entry : FbDbType.Binary;	
		}


		public IDbCommandBuilder CreateCommandBuilder(DataTable table)
		{
			return new FirebirdCommandBuilder(table, this);
		}


	}
}
