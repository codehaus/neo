using System;
using System.Collections;
using System.Text;
using Neo.MetaModel;


namespace Neo.MetaModel
{
	public enum NamingMethod
	{
		NoChange,
		Underscore,
		CamelCase
	}


	public class ModelHelper
	{
		private Entity	entity;

		public ModelHelper(Entity anEntity)
		{
			entity = anEntity;
		}


		[Obsolete("Use implementation in IEntity instead.")]
		public bool PrimaryKeyIsForeignKey()
		{
			foreach(EntityAttribute attr in entity.PkColumns)
			{
				EntityRelationship rel;

				if(((rel = RelationshipForAttribute(attr)) == null) || (rel.Direction == RelDirection.Parent))
					return false;
			}
			return true;
		}


		[Obsolete("Use implementation in IEntity instead.")]
		public EntityRelationship RelationshipForAttribute(EntityAttribute attr)
		{
			foreach(EntityRelationship rel in entity.Relationships)
			{
				if(attr.ColumnName == rel.LocalKey)
					return rel;
			}
			return null;
		}


		[Obsolete("Use implementation in IEntity instead.")]
		public EntityRelationship[] RelationshipsForAttribute(EntityAttribute attr)
		{
			ArrayList relList;

			relList = new ArrayList();
			foreach(EntityRelationship rel in entity.Relationships)
			{
				if(attr.ColumnName == rel.LocalKey)
					relList.Add(rel);
			}
			return (EntityRelationship[])relList.ToArray(typeof(EntityRelationship));
		}


		[Obsolete("Use implementation in IEntity instead.")]
		public EntityRelationship[][] RelationshipSetsForColumns(EntityAttribute[] attrList)
		{
			ArrayList[]				lists;
			EntityRelationship[]	relList;

			lists = new ArrayList[attrList.Length];
			for(int i = 0; i < attrList.Length; i++)
			{
				lists[i] = new ArrayList();
				relList = RelationshipsForAttribute(attrList[i]);
				if(i == 0)
				{
					AddCombinations(new EntityRelationship[0], relList, lists[i]);
				}
				else
				{
					foreach(EntityRelationship[] fixedPart in lists[i - 1])
						AddCombinations(fixedPart, relList, lists[i]);
				}
			}
			return (EntityRelationship[][])lists[attrList.Length - 1].ToArray(typeof(EntityRelationship[]));
		}


		protected void AddCombinations(EntityRelationship[] fixedPart, EntityRelationship[] combinations, ArrayList list)
		{
			EntityRelationship[] entry;

			foreach(EntityRelationship r in combinations)
			{
				entry = new EntityRelationship[fixedPart.Length + 1];
				fixedPart.CopyTo(entry, 0);
				entry[fixedPart.Length] = r;
				list.Add(entry);
			}
		}


		public static string PrettifyName(string name, NamingMethod method)
		{
			StringBuilder	prettyNameBuilder;
			bool			capitalise;

			if(method == NamingMethod.NoChange)
				return name;
			
			prettyNameBuilder = new StringBuilder();
			capitalise = true;
			foreach(char c in name)
			{
				if(c == '_')
				{
					capitalise = true;
				}
				else
				{
					if(capitalise)
						prettyNameBuilder.Append(Char.ToUpper(c));
					else if(method == NamingMethod.Underscore)
						prettyNameBuilder.Append(Char.ToLower(c));
					else
						prettyNameBuilder.Append(c);
					capitalise = false;
				}
			}
			return prettyNameBuilder.ToString();
		}



	}
}
