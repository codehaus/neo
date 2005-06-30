using System;
using System.Collections;
using System.Data;
using System.Reflection;
using Neo.Core;
using Neo.Core.Util;


namespace Neo.Framework
{

	public abstract class EntityMap : IEntityMap
	{
		//--------------------------------------------------------------------------------------
		//	Fields
		//--------------------------------------------------------------------------------------

		private IEntityMapFactory	factory;
		private IDictionary			relationInfos;


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

		public abstract string[] Relations
		{
			get;
		}
		
		protected abstract IDictionary GetRelationInfos();
		
		public abstract IPkInitializer GetPkInitializer();


		//--------------------------------------------------------------------------------------
		//	ToString() override
		//--------------------------------------------------------------------------------------

		public override string ToString()
		{
			return String.Format("{0} [{1}, {2}]", GetType().ToString(), TableName, ObjectType.ToString());
		}

		
		//--------------------------------------------------------------------------------------
		//	Derived properties
		//--------------------------------------------------------------------------------------

		public string GetColumnForAttribute(string attribute)
		{
			string[] attrs = Attributes;

			for(int i = 0; i < attrs.Length; i++)
			{
				if(attrs[i] == attribute)
					return Columns[i];
			}
			throw new ArgumentException(String.Format("Attribute {0} not found in class {1} (Maybe you are looking for a Relation?)",
				attribute, ConcreteObjectType.ToString()));
		}

	
		private void CacheRelationInfos()
		{
			if(relationInfos == null)
				relationInfos = GetRelationInfos();
		}


		public RelationInfo GetRelationInfo(string relation)
		{
			CacheRelationInfos();

			RelationInfo info = (RelationInfo)relationInfos[relation];
			if(info != null)
				return info;
			
			throw new ArgumentException(String.Format("Relation {0} not found in class {1}",
				relation, ConcreteObjectType.ToString()));
		}


		public string GetRelationName(RelationInfo info)
		{
			CacheRelationInfos();

			foreach(DictionaryEntry entry in relationInfos)
			{
				if(entry.Value.Equals(info))
					return (string)entry.Key;
			}
			throw new InvalidOperationException("Relation info not found in entity " + ObjectType.ToString());
		}


		//--------------------------------------------------------------------------------------
		//	Object Instantiation
		//--------------------------------------------------------------------------------------

		public virtual IEntityObject CreateInstance(DataRow row, ObjectContext context)
		{
			BindingFlags bflags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			object[] args = { row, context };
			return (IEntityObject)Activator.CreateInstance(ConcreteObjectType, bflags, null, args, null);
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
				CacheRelationInfos();
				foreach(DictionaryEntry e in relationInfos)
				{
					RelationInfo info = (RelationInfo)e.Value;
					if(info.ParentEntity != this)
						info.ParentEntity.UpdateSchemaInDataSet(aDataSet, SchemaUpdate.Basic | SchemaUpdate.Constraints);
					if(info.ChildEntity != this)
						info.ChildEntity.UpdateSchemaInDataSet(aDataSet, SchemaUpdate.Basic | SchemaUpdate.Constraints);
				}
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
