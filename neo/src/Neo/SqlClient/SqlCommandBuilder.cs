using System;
using System.Data;
using System.Text;

using Neo.Core;
using Neo.Database;


namespace Neo.SqlClient
{

	public class SqlCommandBuilder : GenericSql92Builder
	{
		//--------------------------------------------------------------------------------------
		//	Fields and Constructor
		//--------------------------------------------------------------------------------------

		public SqlCommandBuilder(DataTable aTable, IDbImplementationFactory aParamFactory) : base(aTable, aParamFactory)
		{
		}

		
		//--------------------------------------------------------------------------------------
		//  SELECT
		//--------------------------------------------------------------------------------------
	
		public override void WriteSelect(IFetchSpecification fetchSpec)
		{
			emap = fetchSpec.EntityMap;

			builder.Append("SELECT ");

			if(fetchSpec.FetchLimit != -1)
			{
				builder.Append("TOP ");
				builder.Append(fetchSpec.FetchLimit);
				builder.Append(" ");
			}

			WriteColumns(table.Columns);

			builder.Append(" FROM ");
			WriteIdentifier(table.TableName);

			if(fetchSpec.Qualifier != null)
			{
				builder.Append(" WHERE ");
				WriteQualifier(fetchSpec.Qualifier);
			}

			if(fetchSpec.SortOrderings != null)
			{
				builder.Append(" ORDER BY ");
				WriteSortOrderings(fetchSpec.SortOrderings);
			}

		}

	    //--------------------------------------------------------------------------------------
		//  Conversions
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
			return "@" + paramName;
		}

        protected override string ConvertToDelimitedIdentifier(string identifier) {
            return String.Format("[{0}]", identifier);
        }



	}
}
