using System.Data;


namespace Neo.Database
{
	public interface IParameterFactory
	{
	    IDataParameter CreateParameter(DataColumn pcolumn, string suffix, object pvalue);
		string ConvertToParameterName(string column, string suffix);

	}

}
