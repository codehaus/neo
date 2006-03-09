using System;
using System.Collections;
using System.Collections.Specialized;
using Neo.Core;

namespace Neo.Framework
{
	/// <summary>
	/// A good base class for your query templates. CodeGen/Norque uses these.
	/// </summary>
	
	public class QueryTemplate : IFetchSpecification
	{
		//--------------------------------------------------------------------------------------
		//	Fields and constructors
		//--------------------------------------------------------------------------------------

		private IEntityMap entityMap;
		private ListDictionary queryValues;
		private int fetchLimit;
		private bool refreshesObjects;
		private PropertyComparer[] sortOrderings;
		private string[] spans;

		protected QueryTemplate(IEntityMap anEntityMap)
		{
			entityMap = anEntityMap;
			queryValues = new ListDictionary();
			fetchLimit = -1;
		}

		
		//--------------------------------------------------------------------------------------
		//	Protected properties
		//--------------------------------------------------------------------------------------

		protected virtual IDictionary QueryValues
		{
			get { return queryValues; }
		}

	
		//--------------------------------------------------------------------------------------
		//	Public properties
		//--------------------------------------------------------------------------------------

		public virtual IEntityMap EntityMap
		{
			get { return entityMap; }
		}

		public virtual Qualifier Qualifier
		{
			get { return Qualifier.FromPropertyDictionary(queryValues); }
		}

		public virtual Int32 FetchLimit
		{
			get { return fetchLimit; }
			set { fetchLimit = value; }
		}

		public bool RefreshesObjects
		{
			get { return refreshesObjects; }
			set { refreshesObjects = value; }
		}

		public virtual PropertyComparer[] SortOrderings
		{
			get { return sortOrderings; }
			set { sortOrderings = value; }
		}

		public virtual string[] Spans
		{
			get { return spans; }
			set { spans = value; }
		}

	}
}
