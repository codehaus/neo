using System;
using System.Collections;
using System.Xml;
using Neo.Model;


namespace Neo.Model.Impl
{

	public class EntityImpl : IEntity
	{
		//--------------------------------------------------------------------------------------
		//	constructor
		//--------------------------------------------------------------------------------------

		private ModelImpl	model;
		private Hashtable	attributes;
		private ArrayList	relationships;

		private String		subPackageName;
		private String		className;
		private String		baseClassName;
		
		private String		tableName;
		private IdMethod	idMethod;		


		public EntityImpl(ModelImpl aModel)
		{
			model = aModel;
			attributes = new Hashtable();
			relationships = new ArrayList();
		}

	
		//--------------------------------------------------------------------------------------
		//	accessors
		//--------------------------------------------------------------------------------------

		public IModel Model
		{
			get { return model ; }
		}

		public void AddAttribute(IAttribute anAttribute)
		{
			attributes.Add(anAttribute.ColumnName, anAttribute);
		}

		public void RemoveAttribute(IAttribute anAttribute)
		{
			attributes.Remove(anAttribute.ColumnName);
		}

		public ICollection Attributes
		{
			get { return attributes.Values; }
		}


		public void AddRelationship(IRelationship aRelationship)
		{
			relationships.Add(aRelationship);
		}

		public void RemoveRelationship(IRelationship aRelationship)
		{
			relationships.Remove(aRelationship);
		}

		public ICollection Relationships
		{
			get { return relationships; }
		}
		
		public string SubPackageName
		{
			set { subPackageName = value; }
			get { return subPackageName; }
		}

		public string ClassName
		{
			set { className = value; }
			get { return className; }
		}

		public string BaseClassName
		{
			set { baseClassName = value; }
			get { return baseClassName; }
		}

		public string TableName
		{
			set { tableName = value; }
			get { return tableName; }
		}

		public IdMethod IdMethod
		{
			set { idMethod = value; }
			get { return idMethod; }
		}

		
		//--------------------------------------------------------------------------------------
		//	derived properties
		//--------------------------------------------------------------------------------------

		public string Namespace 
		{
			get
			{
				string ns = model.Namespace;
				if(SubPackageName != null)
					ns += "." + SubPackageName;
				return ns;
			}
		}


		public ICollection UsedNamespaces
		{
			get
			{
				ArrayList namespaces;

				namespaces = new ArrayList();
				foreach(RelationshipImpl r in Relationships)
				{
					if(namespaces.Contains(r.ForeignEntity.Namespace) == false)
						namespaces.Add(r.ForeignEntity.Namespace);
				}
				return namespaces;
			}
		}	


		public ICollection PkColumns
		{
			get
			{
				ArrayList columns = new ArrayList();
				foreach(AttributeImpl a in attributes.Values)
				{
					if(a.IsPkColumn)
						columns.Add(a);
				}
				return columns;
			}
		}

		
		public bool PrimaryKeyIsForeignKey
		{
			get
			{
				foreach(IAttribute attr in PkColumns)
				{
					IRelationship rel;

					if(((rel = RelationshipForAttribute(attr)) == null) || (rel.Direction == RelDirection.Parent))
						return false;
				}
				return true;
			}
		}


		public IRelationship RelationshipForAttribute(IAttribute attr)
		{
			ArrayList relList;

			relList = new ArrayList();
			foreach(IRelationship rel in Relationships)
			{
				if(attr.ColumnName == rel.LocalKey)
					return rel;
			}
			return null;
		}


		public IList RelationshipsForAttribute(IAttribute attr)
		{
			ArrayList relList;

			relList = new ArrayList();
			foreach(IRelationship rel in Relationships)
			{
				if(attr.ColumnName == rel.LocalKey)
					relList.Add(rel);
			}
			return relList;
		}


		public IList[] RelationshipSetsForColumns(IList attrList)
		{
			ArrayList[]	lists;
			IList		relList;

			lists = new ArrayList[attrList.Count];
			for(int i = 0; i < attrList.Count; i++)
			{
				lists[i] = new ArrayList();
				relList = RelationshipsForAttribute((IAttribute)attrList[i]);
				if(i == 0)
				{
					AddCombinations(new ArrayList(), relList, lists[i]);
				}
				else
				{
					foreach(ArrayList fixedPart in lists[i - 1])
						AddCombinations(fixedPart, relList, lists[i]);
				}
			}
			return (IList[])lists[attrList.Count - 1].ToArray(typeof(IList));
		}


		protected void AddCombinations(IList fixedPart, IList combinations, ArrayList list)
		{
			ArrayList entry;

			foreach(IRelationship r in combinations)
			{
				entry = new ArrayList(fixedPart.Count + 1);
				entry.AddRange(fixedPart);
				entry.Add(r);
				list.Add(entry);
			}
		}


		public ICollection ToOneRelationships
		{
			get
			{
				return GetRelationships(RelType.ToOne);
			}
		}

		
		public ICollection ToManyRelationships
		{
			get
			{
				return GetRelationships(RelType.ToMany);
			}
		}


	
		//--------------------------------------------------------------------------------------
		//	helper
		//--------------------------------------------------------------------------------------

		public bool ColumnIsPrimaryKey(string columnname)
		{
			if(HasAttribute(columnname) == false)
				throw new ArgumentException(String.Format("Table {0} has no column named {1}.", TableName, columnname));
			ArrayList pkcolumns = (ArrayList)PkColumns;
			return ((pkcolumns.Count == 1) && (((AttributeImpl)pkcolumns[0]).ColumnName == columnname));
		}

		
		public bool HasAttribute(string columnName)
		{
			return (attributes[columnName] != null);
		}


		protected ICollection GetRelationships(RelType types)
		{
			ArrayList list;

			if(types == RelType.All)
			{
				list = relationships;
			}
			else
			{
				list = new ArrayList();
				foreach(RelationshipImpl rel in relationships)
				{
					if((rel.Type & types) != 0)
						list.Add(rel);
				}
			}
			return list;
		}

	}

}
