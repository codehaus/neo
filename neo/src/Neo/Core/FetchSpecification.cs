using System;
using System.Collections;


namespace Neo.Core
{
	public class FetchSpecification : IFetchSpecification
	{
		//--------------------------------------------------------------------------------------
		//	Fields and constructor
		//--------------------------------------------------------------------------------------
		
		private IEntityMap entityMap;
		private Qualifier qualifier;
		private Int32 fetchLimit;
		private PropertyComparer[] sortOrderings;

		public FetchSpecification()
		{
			fetchLimit = -1;
		}

		public FetchSpecification(IEntityMap anEntityMap) : this()
		{
			entityMap = anEntityMap;
		}

		public FetchSpecification(IEntityMap anEntityMap, Qualifier aQualifier) : this(anEntityMap)
		{
			qualifier = aQualifier;
		}

		public FetchSpecification(IEntityMap anEntityMap, Qualifier aQualifer, int aLimit) : this(anEntityMap, aQualifer)
		{
			fetchLimit = aLimit;
		}


		//--------------------------------------------------------------------------------------
		//	ToString() override
		//--------------------------------------------------------------------------------------

		public override string ToString()
		{
			return String.Format("{0}({1})", entityMap.ObjectType.Name, qualifier );
		}

		
		//--------------------------------------------------------------------------------------
		//	IFetchSpecification impl plus setters
		//--------------------------------------------------------------------------------------

		public virtual IEntityMap EntityMap
		{
			set { entityMap = value; }
			get { return entityMap; }
		}


		public virtual Qualifier Qualifier
		{
			set { qualifier = value; }
			get { return qualifier; }
		}


		public virtual int FetchLimit
		{
			set { fetchLimit = value; }
			get { return fetchLimit; }
		}


		public virtual PropertyComparer[] SortOrderings 
		{
			set { sortOrderings = value; }
			get { return sortOrderings; }
		}


	}
}
