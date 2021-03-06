using System;
using System.Collections;
using Neo.Core;


namespace Neo.Framework
{

	public class ObjectFactory
	{
		//--------------------------------------------------------------------------------------
		//	Fields and Constructor
		//--------------------------------------------------------------------------------------

		public readonly ObjectContext Context;
		public readonly IEntityMap EntityMap;
		

		public ObjectFactory(ObjectContext aContext, Type objectType)
		{
			Context = aContext;
			EntityMap = Context.EntityMapFactory.GetMap(objectType);
		}


		//--------------------------------------------------------------------------------------
		//	Generic create and find methods
		//--------------------------------------------------------------------------------------

		protected IEntityObject CreateObject(object[] pkvalues)
		{
			return Context.CreateObject(EntityMap.ObjectType, pkvalues);
		}

		protected IEntityObject FindObject(object[] pkvalues)
		{
			return Context.GetObjectFromTable(EntityMap.TableName, pkvalues);
		}


		public IList FindAllObjects()
		{
			return Context.GetObjects(new FetchSpecification(EntityMap));
		}


		public IList Find(IFetchSpecification fetchSpec)
		{
			if((fetchSpec.EntityMap == null) && (fetchSpec is FetchSpecification))
				((FetchSpecification)fetchSpec).EntityMap = EntityMap;
			return Context.GetObjects(fetchSpec);
		}
		
		public IList Find(Qualifier qualifier)
		{
			FetchSpecification f = new FetchSpecification(EntityMap, qualifier);
			return Find(f);
		}

		public IList Find(string qualifierFormat, params object[] parameters)
		{
			Qualifier q = Qualifier.Format(qualifierFormat, parameters);
			return this.Find(q);
		}

		public IList FindWithLimit(int limit, string qualifierFormat, params object[] parameters)
		{
			Qualifier q = Qualifier.Format(qualifierFormat, parameters);
			FetchSpecification f = new FetchSpecification(EntityMap, q, limit);
			return Find(f);
		}

		public IEntityObject FindFirst(string qualifierFormat, params object[] parameters)
		{
			IList results = FindWithLimit(1, qualifierFormat, parameters);

			return (results.Count > 0) ? (IEntityObject)results[0] : null;
		}

		public IEntityObject FindFirst(Qualifier q)
		{
			FetchSpecification f = new FetchSpecification(EntityMap, q, 1);
			IList result = Find(f);
			return result.Count > 0 ? (IEntityObject)result[0]: null;
		}

		public IEntityObject FindUnique(string qualifierFormat, params object[] parameters)
		{
			IList results = FindWithLimit(2, qualifierFormat, parameters);

			if(results.Count != 1)
				throw new NotUniqueException(results.Count > 1, qualifierFormat, parameters);

			return (IEntityObject)results[0];
		}


	}

}

