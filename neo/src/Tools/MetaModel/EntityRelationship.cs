using System;
using System.Data;


namespace Neo.MetaModel
{
	[Flags]
	public enum RelType
	{
		ToOne  = 0x01,
		ToMany = 0x02,
		All    = 0xFF
	}

	[Flags]
	public enum RelDirection
	{
		Parent = 0x01,
		Child  = 0x02,
		Any    = 0xFF
	}
	
	
	
	public class EntityRelationship
	{
		//--------------------------------------------------------------------------------------
		//	fields and constructor
		//--------------------------------------------------------------------------------------

		private Entity	localEntity;

		private	String		varName;

		private String		relationshipAttributes; /* custom attributes in the .NET sense */
		
		private String		foreignTableName;
		private String		localKey;
		private String		foreignKey;

		private Rule		updateRule;
		private Rule		deleteRule;


		public EntityRelationship(Entity anEntity)
		{
			localEntity = anEntity;
		}


		//--------------------------------------------------------------------------------------
		//	accessors
		//--------------------------------------------------------------------------------------

		public Entity LocalEntity
		{
			get { return localEntity; }
		}

		public string VarName
		{
			set { varName = value; }
			get { return (varName != null) ? varName : ForeignEntity.ClassName; }
		}

		public String RelationshipAttributes
		{
			set { relationshipAttributes = value; }
			get { return relationshipAttributes; }
		}

		public String ForeignTableName
		{
			set { foreignTableName = value; }
			get { return foreignTableName; }
		}

		public string LocalKey
		{
			set { localKey = value; }
			get { return localKey; }
		}

		public string ForeignKey
		{
			set { foreignKey = value; }
			get { return foreignKey; }
		}

		public Rule UpdateRule
		{
			set { updateRule = value; }
			get { return updateRule; }
		}

		public Rule DeleteRule
		{
			set { deleteRule = value; }
			get { return deleteRule; }
		}


		//--------------------------------------------------------------------------------------
		//	derived properties
		//--------------------------------------------------------------------------------------

		public Entity ForeignEntity
		{
			get { return LocalEntity.Model.GetEntityForTable(foreignTableName); }
		}


		public RelType Type
		{
			get
			{
				if(ForeignEntity.ColumnIsPrimaryKey(ForeignKey))
					return RelType.ToOne;
				return RelType.ToMany;
			}
		}

		public RelDirection Direction
		{
			get
			{
//				string direction = ValueForAttribute("direction");
//				if(direction != null)
//					return (direction == "parent") ? RelDirection.Parent : RelDirection.Child;
				if(ForeignEntity.ColumnIsPrimaryKey(ForeignKey))
					return RelDirection.Child;
				if(LocalEntity.ColumnIsPrimaryKey(LocalKey))
					return RelDirection.Parent;
				throw new InvalidOperationException(String.Format("Failed to infer relationship direction for relationship {0} in entity {1}. (This might be caused by a misconfigured primary key in {2}.)", DotNetName, LocalEntity.ClassName, ForeignEntity.ClassName));
			}
		}


		public EntityRelationship InverseRelationship
		{
			get
			{
				foreach(EntityRelationship r in ForeignEntity.Relationships)
				{
					if((r.ForeignEntity.TableName == LocalEntity.TableName) && (r.ForeignKey == LocalKey) && (r.LocalKey == ForeignKey))
						return r;
				}
				return null;
			}
		}


		//--------------------------------------------------------------------------------------
		//	compatibility
		//--------------------------------------------------------------------------------------

		public string DotNetName
		{
			get	{ return VarName; }
		}


		public string DotNetUpdateRule
		{
			get { return "Rule." + updateRule.ToString(); }
		}


		public string DotNetDeleteRule
		{
			get { return "Rule." + deleteRule.ToString(); }
		}


	}
}
