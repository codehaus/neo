using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using Neo.Core.Util;


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


		public FetchSpecification(IEntityMap anEntityMap)
		{
			entityMap = anEntityMap;
			qualifier = null;
			fetchLimit = -1;
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
		//	IFetchSpecification impl
		//--------------------------------------------------------------------------------------

		public virtual IEntityMap EntityMap
		{
			get { return entityMap; }
		}


		public virtual Qualifier Qualifier
		{
			get { return qualifier; }
		}


		public virtual int FetchLimit
		{
			get { return fetchLimit; }
		}


	}
}
