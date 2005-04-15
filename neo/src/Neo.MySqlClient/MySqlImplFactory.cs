using System;
using System.Collections;
using System.Data;

using MySql.Data.MySqlClient;

using Neo.Database;

namespace Neo.MySqlClient
{
    public class MySqlImplFactory : IDbImplementationFactory
    {
        //--------------------------------------------------------------------------------------
        //	IDbImplementationFactory impl
        //--------------------------------------------------------------------------------------

        public IDbConnection CreateConnection(string connectionString)
        {
            return new MySqlConnection(connectionString);
        }

        public IDbCommand CreateCommand()
        {
            return new MySqlCommand();
        }

        public IDbDataAdapter CreateDataAdapter()
        {
            return new MySqlDataAdapter();
        }

        public IDataParameter CreateParameter(DataColumn pcolumn, string pname, object pvalue)
        {
            MySqlDbType     dbtype;
            MySqlParameter  param;
            Console.Out.WriteLine("pname = {0}", pname);

            if((dbtype = ConvertToDbType(pcolumn.DataType)) == MySqlDbType.Blob)
                throw new ArgumentException(String.Format("Cannot handle data type {0} of column {1} in table {2}.", pcolumn.DataType.ToString(), pcolumn.ColumnName, pcolumn.Table.TableName));
            param = new MySqlParameter(pname, dbtype, Math.Max(0, pcolumn.MaxLength), pcolumn.ColumnName);
            Console.Out.WriteLine("param.ParameterName = {0}", param.ParameterName);
            param.IsNullable = pcolumn.AllowDBNull;
            param.Value = pvalue;
            param.ParameterName = pname;
            Console.Out.WriteLine("param.ParameterName = {0}", param.ParameterName);
            return param;
        }

        private static Hashtable dbTypeTable;

        private MySqlDbType ConvertToDbType(Type t) {
            if(dbTypeTable == null) {
                dbTypeTable = new Hashtable();
                dbTypeTable.Add(typeof(Boolean),    MySqlDbType.Byte);
                dbTypeTable.Add(typeof(Int16),	    MySqlDbType.Int16);
                dbTypeTable.Add(typeof(Int32),	    MySqlDbType.Int32);
                dbTypeTable.Add(typeof(Int64),	    MySqlDbType.Int64);
                dbTypeTable.Add(typeof(Double),	    MySqlDbType.Double);
                dbTypeTable.Add(typeof(Decimal),    MySqlDbType.Decimal);
                dbTypeTable.Add(typeof(String),	    MySqlDbType.VarChar);
                dbTypeTable.Add(typeof(DateTime),   MySqlDbType.Timestamp);
                dbTypeTable.Add(typeof(Byte[]),	    MySqlDbType.Blob);
            }

            object entry = dbTypeTable[t];
            return (entry != null) ? (MySqlDbType)entry : MySqlDbType.Blob;	
        }

        public IDbCommandBuilder CreateCommandBuilder(DataTable table)
        {
            return new MySqlCommandBuilder(table, this);
        }
    }
}