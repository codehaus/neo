using System.Data;
using Neo.Core;
using Neo.Database;


namespace Neo.OracleClient
{

	public class OracleCommandBuilder : GenericSql92Builder
	{
		//--------------------------------------------------------------------------------------
		//	Fields and Constructor
		//--------------------------------------------------------------------------------------

		public OracleCommandBuilder(DataTable aTable, IDbImplementationFactory aParamFactory) : base(aTable, aParamFactory)
		{
		}

		
		//--------------------------------------------------------------------------------------
		//  SELECT
		//--------------------------------------------------------------------------------------
	
		public override void WriteSelect(IFetchSpecification fetchSpec)
		{
			emap = fetchSpec.EntityMap;

			builder.Append("SELECT ");
			WriteColumns(table.Columns);

			builder.Append(" FROM ");
			builder.Append(table.TableName);

			if((fetchSpec.Qualifier != null) || (fetchSpec.FetchLimit != -1)) 
			{
				builder.Append(" WHERE ");
				if(fetchSpec.FetchLimit != -1) 
				{
					builder.Append("ROWNUM <= ");
					builder.Append(fetchSpec.FetchLimit);
					if(fetchSpec.Qualifier == null) 
						builder.Append(" ");
					else 
						builder.Append(" AND ");
				}
				if(fetchSpec.Qualifier != null) 
				{
					WriteQualifier(fetchSpec.Qualifier);
				}
			}

			if(fetchSpec.SortOrderings != null)
			{
				builder.Append(" ORDER BY ");
				WriteSortOrderings(fetchSpec.SortOrderings);
			}
		}


		//--------------------------------------------------------------------------------------
		//  Parameter Names
		//--------------------------------------------------------------------------------------
	
		protected override string ConvertToParameterName(string column)
		{
			string paramName = column;
			paramName = paramName.Replace(@"\", "_");
			paramName = paramName.Replace(@"/", "_");
			paramName = paramName.Replace(@"'", "_");
			paramName = paramName.Replace(@"=", "_");
			paramName = paramName.Replace("\"", "_");
			paramName = paramName.Replace(@"-", "_");
			paramName = paramName.Replace(@" ", "_");
			paramName = ":" + paramName;
	        return (paramName.Length <= 30) ? paramName : paramName.Substring(0, 30);
		}


	}
}
