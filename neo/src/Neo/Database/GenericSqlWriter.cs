using System.Collections;
using System.Data;
using System.Text;


namespace Neo.Database
{
	public abstract class GenericSqlWriter
	{
		//--------------------------------------------------------------------------------------
		//	Fields and Constructor
		//--------------------------------------------------------------------------------------

		protected DataTable			table;
		protected IParameterFactory	parameterFactory;

		protected StringBuilder		builder;
		protected IList				parameters;

		public GenericSqlWriter(DataTable aTable, IParameterFactory aParamFactory)
		{
			table = aTable;
			parameterFactory = aParamFactory;

			builder = new StringBuilder();
			parameters = new ArrayList();
		}


		//--------------------------------------------------------------------------------------
		//	accessors
		//--------------------------------------------------------------------------------------
		
		public string Command
		{
			get { return builder.ToString(); }
		}
		

		public IList Parameters	
		{
			get { return parameters; }
		}


	
		//--------------------------------------------------------------------------------------
		//	low-level writers
		//--------------------------------------------------------------------------------------

		protected void WriteColumns(ICollection columns)
		{
			bool first = true;

			foreach(DataColumn c in columns)
			{
				if(first == false)
					builder.Append(", ");
				builder.Append(c.ColumnName);
				first = false;
			}
		}

		protected void WriteParameters(ICollection columns)
		{
			bool first = true;

			foreach(DataColumn c in columns)
			{
				if(first == false)
					builder.Append(", ");
				builder.Append(parameterFactory.ConvertToParameterName(c.ColumnName, ""));
				first = false;
			}
		}

		
		protected void WriteAllColumnsAndParameters(string suffix, string separator)
		{
			bool firstColumn = true;

			for(int i = 0; i < table.Columns.Count; i++)
			{
				// Workaround for update with autoincrement bug
				if(table.Columns[i].AutoIncrement)
					continue;

				if(firstColumn == false)
					builder.Append(separator);
				firstColumn = false;

				builder.Append(" ");
				builder.Append(table.Columns[i].ColumnName);
				builder.Append(" = ");
				builder.Append(parameterFactory.ConvertToParameterName(table.Columns[i].ColumnName, suffix));
			}
		}


	}
}
