using System.Data;


namespace Neo.Database
{
	public interface IDbImplementationFactory
	{
		IDbConnection CreateConnection(string connectionString);
		IDbCommand CreateCommand();
		IDbDataAdapter CreateDataAdapter();
		IDataParameter CreateParameter(DataColumn pcolumn, string pname, object pvalue);
		IDbCommandBuilder CreateCommandBuilder(DataTable table);

	}

}
