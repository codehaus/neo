using System.Collections;
using System.Data;


namespace Neo.Database
{
	/// <summary>
	/// Summary description for GenericSqlModWriter.
	/// </summary>
	public class GenericSqlModWriter : GenericSqlWriter
	{
		//--------------------------------------------------------------------------------------
		//	Fields and Constructor
		//--------------------------------------------------------------------------------------
		
		public GenericSqlModWriter(DataTable aTable, IParameterFactory aParamFactory) : base(aTable, aParamFactory)
		{
		}


		//--------------------------------------------------------------------------------------
		//	INSERT
		//--------------------------------------------------------------------------------------

		public void WriteInsert(DataRow row, IList columnList)
		{
			builder.Append("INSERT INTO ");
			builder.Append(row.Table.TableName);
			builder.Append(" (");
			WriteColumns(columnList);
			builder.Append(") VALUES (");
			WriteParameters(columnList);
			builder.Append(")");
			
			foreach(DataColumn column in columnList)
				parameters.Add(parameterFactory.CreateParameter(column, "", row[column]));
		}

		//--------------------------------------------------------------------------------------
		//	UPDATE
		//--------------------------------------------------------------------------------------

		public void WriteUpdate(DataRow row)
		{
			builder.Append("UPDATE ");
			builder.Append(row.Table.TableName);
			builder.Append(" SET");

			WriteAllColumnsAndParameters("", ", ");

			builder.Append(" WHERE");

			WriteOptimisticLockMatch();

			foreach(DataColumn column in row.Table.Columns)
			{
				parameters.Add(parameterFactory.CreateParameter(column, "", row[column]));
				parameters.Add(parameterFactory.CreateParameter(column, "_ORIG", row[column, DataRowVersion.Original]));
			}
		}

		
		//--------------------------------------------------------------------------------------
		//	DELETE
		//--------------------------------------------------------------------------------------

		public void WriteDelete(DataRow row)
		{
			builder.Append("DELETE FROM ");
			builder.Append(row.Table.TableName);
			builder.Append(" WHERE");

			WriteOptimisticLockMatch();

			foreach(DataColumn column in row.Table.Columns)
				parameters.Add(parameterFactory.CreateParameter(column, "_ORIG", row[column, DataRowVersion.Original]));

		}


		//--------------------------------------------------------------------------------------
		//	low-level writers
		//--------------------------------------------------------------------------------------
		
		protected void WriteOptimisticLockMatch()
		{
			// This is not really correct, we should exclude BLOBs. Then again, we don't do BLOBs yet.
			for(int i = 0; i < table.Columns.Count; i++)
			{
				if(i > 0)
					builder.Append(" AND");
				builder.Append(" ((");
				builder.Append(table.Columns[i].ColumnName);
				builder.Append(" = ");
				builder.Append(parameterFactory.ConvertToParameterName(table.Columns[i].ColumnName, "_ORIG"));
				builder.Append(" ) OR ((");
				builder.Append(table.Columns[i].ColumnName);
				builder.Append(" IS NULL) AND (");
				builder.Append(parameterFactory.ConvertToParameterName(table.Columns[i].ColumnName, "_ORIG"));
				builder.Append(" IS NULL)))");
			}
		}

	}
}
