using System;
using System.Data;
using Neo.Core;


namespace Neo.Framework
{

	public abstract class EntityMap : IEntityMap
	{
		//--------------------------------------------------------------------------------------
		//	Fields
		//--------------------------------------------------------------------------------------

		private IEntityMapFactory	factory;


		//--------------------------------------------------------------------------------------
		//	Public properties
		//--------------------------------------------------------------------------------------

		public IEntityMapFactory Factory
		{
			set { factory = value; }
			get { return factory; }
		}

		
		//--------------------------------------------------------------------------------------
		//	Public properties to be overwritten by subclasses
		//--------------------------------------------------------------------------------------

		public abstract Type ObjectType
		{
			get;
		}

		public virtual Type ConcreteObjectType
		{
			get { return ObjectType; }
			set { throw new InvalidOperationException("Cannot set concrete object type. (You have to override this property using a custom template.)"); }
		}

		public abstract string TableName
		{
			get;
		}

		public abstract string[] PrimaryKeyColumns
		{
			get;
		}

		public abstract string[] Columns
		{
			get;
		}


		public abstract string[] Attributes
		{
			get;
		}

		
		public abstract IPkInitializer GetPkInitializer();

		
		public abstract string[] RelatedTables
		{
			get;
		}


		//--------------------------------------------------------------------------------------
		//	ToString() override
		//--------------------------------------------------------------------------------------

		public override string ToString()
		{
			return String.Format("{0} [{1}, {2}]", GetType().ToString(), TableName, ObjectType.ToString());
		}

		
		//--------------------------------------------------------------------------------------
		//	Helper
		//--------------------------------------------------------------------------------------

		public string GetColumnForAttribute(string attribute)
		{
			string[] attrs = Attributes;
			for(int i = 0; i < attrs.Length; i++)
			{
				if(attrs[i] == attribute)
					return Columns[i];
			}
			throw new ArgumentException(String.Format("Attribute {0} not found in class {1} (Maybe you are looking for a relation?)",
				attribute, ObjectType.ToString()));
		}
		
		
		//--------------------------------------------------------------------------------------
		//	Schema generation
		//--------------------------------------------------------------------------------------

		public virtual void UpdateSchemaInDataSet(DataSet aDataSet)
		{
			UpdateSchemaInDataSet(aDataSet, SchemaUpdate.Full);
		}


		public virtual void UpdateSchemaInDataSet(DataSet aDataSet, SchemaUpdate update)
		{
			DataTable	table;

			if(((update & SchemaUpdate.Basic) != 0) && (table = aDataSet.Tables[TableName]) == null)
			{
				table = aDataSet.Tables.Add(TableName);
				WriteBasicSchema(table);
			}
			if((update & SchemaUpdate.Constraints) != 0)
			{
				WriteConstraints(aDataSet.Tables[TableName]);
			}
			if((update & SchemaUpdate.Relations) != 0)
			{
				foreach(string t in RelatedTables)
					factory.GetMap(t).UpdateSchemaInDataSet(aDataSet, SchemaUpdate.Basic | SchemaUpdate.Constraints);
				WriteRelations(aDataSet.Tables[TableName]);
			}

		}


		public virtual void UpdateSchema(DataTable table, SchemaUpdate update)
		{
			if((update & SchemaUpdate.Basic) != 0)
			{
				WriteBasicSchema(table);
			}
			if((update & SchemaUpdate.Constraints) != 0)
			{
				WriteConstraints(table);
			}
		}

		
		protected abstract void WriteBasicSchema(DataTable table);

		
		protected virtual void WriteRelations(DataTable table)
		{
		}


		protected virtual void WriteConstraints(DataTable table)
		{
			if((table.PrimaryKey != null) && (table.PrimaryKey.Length > 0))
				return;

			DataColumn[] pkcolumns;
			DataColumn	 column;
			int			 columnIdx = 0;

			pkcolumns = new DataColumn[PrimaryKeyColumns.Length];
			foreach(string columnName in PrimaryKeyColumns)
			{
				if((column = table.Columns[columnName]) == null)
					throw new InvalidOperationException(String.Format("Failed to find PK column {0} in table {1}", columnName, TableName));
				pkcolumns[columnIdx++] = column;
			}
			table.PrimaryKey = pkcolumns;
		}


	
	
	}
}
