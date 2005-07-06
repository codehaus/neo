using System.Collections;
using System.Data;
using System.Text.RegularExpressions;

namespace Neo.Core.Util
{
	/// <summary>
	/// You can use this class to decorate any data store. It checks for queries
	/// which have neither limit nor qualifier and throws an exception.
	/// </summary>

	public class QueryChecker : IDataStore
	{
		IDataStore		chainedStore;
		Regex			typeFilter;

		public QueryChecker(IDataStore aDataStore)
		{
			chainedStore = aDataStore;
		}

		
		/// <summary>
		/// A regular expression string that specifies which types are allowed to have unqualified queries.
		/// </summary>
		public string TypeFilter
		{
			get { return typeFilter.ToString(); }
			set { typeFilter = new Regex(value); }
		}


		public virtual DataSet FetchRows(IFetchSpecification fetchSpec)
		{
			if((typeFilter == null) || (typeFilter.IsMatch(fetchSpec.EntityMap.ObjectType.FullName) == false))
			{
				if((fetchSpec.Qualifier == null) && (fetchSpec.FetchLimit == -1))
					throw new InvalidQueryException("QueryChecker: No qualifier and no limit on query; found " + fetchSpec);
			}
			return chainedStore.FetchRows(fetchSpec);
		}

		public virtual ICollection SaveChangesInObjectContext(ObjectContext context)
		{
			return chainedStore.SaveChangesInObjectContext(context);
		}

	}
}
