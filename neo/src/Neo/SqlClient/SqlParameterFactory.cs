using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using Neo.Database;


namespace Neo.SqlClient
{
	/// <summary>
	/// Summary description for ParameterFactory.
	/// </summary>
	public class SqlParameterFactory : IParameterFactory
	{
		public SqlParameterFactory()
		{
		}

		//--------------------------------------------------------------------------------------

		public IDataParameter CreateParameter(DataColumn pcolumn, string suffix, object pvalue)
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
	
		public string ConvertToParameterName(string column, string suffix)
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

		protected SqlDbType ConvertToDbType(Type t)
		{
			if(dbTypeTable == null)
			{
				dbTypeTable = new Hashtable();
				dbTypeTable.Add(typeof(Boolean),  SqlDbType.Bit);
				dbTypeTable.Add(typeof(Int16),	 SqlDbType.SmallInt);
				dbTypeTable.Add(typeof(Int32),	 SqlDbType.Int);
				dbTypeTable.Add(typeof(Int64),	 SqlDbType.BigInt);
				dbTypeTable.Add(typeof(Double),	 SqlDbType.Float);
				dbTypeTable.Add(typeof(Decimal),	 SqlDbType.Decimal);
				dbTypeTable.Add(typeof(String),	 SqlDbType.VarChar);
				dbTypeTable.Add(typeof(DateTime), SqlDbType.DateTime);
				dbTypeTable.Add(typeof(Byte[]),	 SqlDbType.VarBinary);
				dbTypeTable.Add(typeof(Guid),	 SqlDbType.UniqueIdentifier);
			}

			object entry = dbTypeTable[t];
			return (entry != null) ? (SqlDbType)entry : SqlDbType.Variant;	
		}
	}
}
