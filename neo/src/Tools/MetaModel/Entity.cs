using System;
using System.Collections;


namespace Neo.MetaModel
{
	public enum IdMethod
	{
		IdBroker,
		Native,
		Guid,
		None
	}


	public class Entity 
	{
		//--------------------------------------------------------------------------------------
		//	constructor
		//--------------------------------------------------------------------------------------

		private Model		model;
		private Hashtable	attributes;
		private ArrayList	relationships;

		private String		subPackageName;
		private String		className;
		private String		baseClassName;
		
		private String		tableName;
		private IdMethod	idMethod;		


		public Entity(Model aModel)
		{
			model = aModel;
			attributes = new Hashtable();
			relationships = new ArrayList();
		}

	
		//--------------------------------------------------------------------------------------
		//	accessors
		//--------------------------------------------------------------------------------------

		public Model Model
		{
			get { return model ; }
		}


		public void AddAttribute(EntityAttribute anAttribute)
		{
			attributes.Add(anAttribute.ColumnName, anAttribute);
		}

		public void RemoveAttribute(EntityAttribute anAttribute)
		{
			attributes.Remove(anAttribute.ColumnName);
		}

		public ICollection Attributes
		{
			get { return attributes.Values; }
		}


		public void AddRelationship(EntityRelationship aRelationship)
		{
			relationships.Add(aRelationship);
		}

		public void RemoveRelationship(EntityRelationship aRelationship)
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
				foreach(EntityRelationship r in Relationships)
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
				foreach(EntityAttribute a in attributes.Values)
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
				foreach(EntityAttribute attr in PkColumns)
				{
				    EntityRelationship rel;

					if(((rel = RelationshipForAttribute(attr)) == null) || (rel.Direction == RelDirection.Parent))
						return false;
				}
				return true;
			}
		}


		public EntityRelationship RelationshipForAttribute(EntityAttribute attr)
		{
			foreach(EntityRelationship rel in Relationships)
			{
				if(attr.ColumnName == rel.LocalKey)
					return rel;
			}
			return null;
		}


		public IList RelationshipsForAttribute(EntityAttribute attr)
		{
			ArrayList relList;

			relList = new ArrayList();
			foreach(EntityRelationship rel in Relationships)
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
				relList = RelationshipsForAttribute((EntityAttribute)attrList[i]);
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

			foreach(EntityRelationship r in combinations)
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
			return ((pkcolumns.Count == 1) && (((EntityAttribute)pkcolumns[0]).ColumnName == columnname));
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
				foreach(EntityRelationship rel in relationships)
				{
					if((rel.Type & types) != 0)
						list.Add(rel);
				}
			}
			return list;
		}

	}

}
