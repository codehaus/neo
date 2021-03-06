using System;
using System.Data;

using Neo.Core;
using Neo.Database;

namespace Neo.MySqlClient
{
    public class MySqlCommandBuilder : GenericSql92Builder
    {
        //--------------------------------------------------------------------------------------
        //	Fields and Constructor
        //--------------------------------------------------------------------------------------

        public MySqlCommandBuilder(DataTable aTable, IDbImplementationFactory aParamFactory) : base(aTable, aParamFactory)
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
            WriteIdentifier(table.TableName);

            if (fetchSpec.Qualifier != null)
            {
                builder.Append(" WHERE ");
                WriteQualifier(fetchSpec.Qualifier);
            }

            if (fetchSpec.SortOrderings != null)
            {
                builder.Append(" ORDER BY ");
                WriteSortOrderings(fetchSpec.SortOrderings);
            }

            if (fetchSpec.FetchLimit != -1)
            {
                builder.Append(" LIMIT ");
                builder.Append(fetchSpec.FetchLimit);
            }
        }

        //--------------------------------------------------------------------------------------
        //  Conversions
        //--------------------------------------------------------------------------------------
	
        protected override string ConvertToParameterName(string column) {
            string paramName = column;
            paramName = paramName.Replace(@"\", "_");
            paramName = paramName.Replace(@"/", "_");
            paramName = paramName.Replace(@"'", "_");
            paramName = paramName.Replace(@"=", "_");
            paramName = paramName.Replace("\"", "_");
            paramName = paramName.Replace(@"-", "_");
            paramName = paramName.Replace(@" ", "_");
            return "?" + paramName;
        }

        protected override string ConvertToDelimitedIdentifier(string identifier) {
            return String.Format("`{0}`", identifier);
        }

    }
}