using System.Data;
using Neo.Core;
using Neo.Database;


namespace Neo.FirebirdClient
{

	public class FirebirdCommandBuilder : GenericSql92Builder
	{
		//--------------------------------------------------------------------------------------
		//	Fields and Constructor
		//--------------------------------------------------------------------------------------

		public FirebirdCommandBuilder(DataTable aTable, IDbImplementationFactory aParamFactory) : base(aTable, aParamFactory)
		{
			usesDelimitedIdentifiers = true;
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
				builder.Append("FIRST ");
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
			return "@" + paramName;
		}


	}
}
