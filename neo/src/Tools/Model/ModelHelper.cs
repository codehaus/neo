using System;
using System.Collections;
using System.Text;

using Neo.Model;


namespace Neo.Model
{
	public enum NamingMethod
	{
		NoChange,
		Underscore,
		CamelCase
	}


	public class ModelHelper
	{
		private IEntity	entity;

		public ModelHelper(IEntity anEntity)
		{
			entity = anEntity;
		}


		[Obsolete("Use implementation in IEntity instead.")]
		public bool PrimaryKeyIsForeignKey()
		{
			foreach(IAttribute attr in entity.PkColumns)
			{
				IRelationship rel;

				if(((rel = RelationshipForAttribute(attr)) == null) || (rel.Direction == RelDirection.Parent))
					return false;
			}
			return true;
		}


		[Obsolete("Use implementation in IEntity instead.")]
		public IRelationship RelationshipForAttribute(IAttribute attr)
		{
			ArrayList relList;

			relList = new ArrayList();
			foreach(IRelationship rel in entity.Relationships)
			{
				if(attr.ColumnName == rel.LocalKey)
					return rel;
			}
			return null;
		}


		[Obsolete("Use implementation in IEntity instead.")]
		public IRelationship[] RelationshipsForAttribute(IAttribute attr)
		{
			ArrayList relList;

			relList = new ArrayList();
			foreach(IRelationship rel in entity.Relationships)
			{
				if(attr.ColumnName == rel.LocalKey)
					relList.Add(rel);
			}
			return (IRelationship[])relList.ToArray(typeof(IRelationship));
		}


		[Obsolete("Use implementation in IEntity instead.")]
		public IRelationship[][] RelationshipSetsForColumns(IAttribute[] attrList)
		{
			ArrayList[]		lists;
			IRelationship[]	relList;

			lists = new ArrayList[attrList.Length];
			for(int i = 0; i < attrList.Length; i++)
			{
				lists[i] = new ArrayList();
				relList = RelationshipsForAttribute(attrList[i]);
				if(i == 0)
				{
					AddCombinations(new IRelationship[0], relList, lists[i]);
				}
				else
				{
					foreach(IRelationship[] fixedPart in lists[i - 1])
						AddCombinations(fixedPart, relList, lists[i]);
				}
			}
			return (IRelationship[][])lists[attrList.Length - 1].ToArray(typeof(IRelationship[]));
		}


		protected void AddCombinations(IRelationship[] fixedPart, IRelationship[] combinations, ArrayList list)
		{
			IRelationship[] entry;

			foreach(IRelationship r in combinations)
			{
				entry = new IRelationship[fixedPart.Length + 1];
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
