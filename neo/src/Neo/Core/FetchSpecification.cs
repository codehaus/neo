using System;


namespace Neo.Core
{
	/// <summary>
	/// Defines which objects should be fetched from a datastore. This class is a straight-forward 
        /// implementation of this interface which allows to set all aspects directly.
	/// </summary>
	/// <remarks>
	/// The Code Generator creates query templates for each entity that also implement
	/// <c>IFetchSpecification</c> and provide a strongly typed API.
	/// </remarks>
	public class FetchSpecification : IFetchSpecification
	{
		//--------------------------------------------------------------------------------------
		//	Fields and constructor
		//--------------------------------------------------------------------------------------
		
		private IEntityMap entityMap;
		private Qualifier qualifier;
		private Int32 fetchLimit;
		private PropertyComparer[] sortOrderings;
		private string[] spans;

		/// <summary>
		/// Creates an empty fetch specification.
		/// </summary>
		public FetchSpecification()
		{
			fetchLimit = -1;
		}

		/// <summary>
		/// Creates a fetch specification for the given entity without limit and qualifier.
		/// </summary>
		public FetchSpecification(IEntityMap anEntityMap) : this()
		{
			entityMap = anEntityMap;
		}

		/// <summary>
		/// Creates a fetch specification for the given entity with the given qualifier.
		/// </summary>
		public FetchSpecification(IEntityMap anEntityMap, Qualifier aQualifier) : this(anEntityMap)
		{
			qualifier = aQualifier;
		}

		/// <summary>
		/// Creates a fetch specification for the given entity with the given qualifier and fetch limit.
		/// </summary>
		public FetchSpecification(IEntityMap anEntityMap, Qualifier aQualifer, int aLimit) : this(anEntityMap, aQualifer)
		{
			fetchLimit = aLimit;
		}


		//--------------------------------------------------------------------------------------
		//	ToString() override
		//--------------------------------------------------------------------------------------

		/// <summary>
		/// Returns a textual description of the fetch specification.
		/// </summary>
		/// <remarks>
		/// Note that this is not parsable by a qualifier.
		/// </remarks>
		public override string ToString()
		{
			return String.Format("{0}({1})", entityMap.ObjectType.Name, qualifier );
		}

		
		//--------------------------------------------------------------------------------------
		//	IFetchSpecification impl plus setters
		//--------------------------------------------------------------------------------------

		/// <summary>
		/// The <c>EntityMap</c> for the entity to fetch the objects from.
		/// </summary>
		public virtual IEntityMap EntityMap
		{
			set { entityMap = value; }
			get { return entityMap; }
		}

		/// <summary>
		/// The <c>Qualifier</c> to apply to the fetched objects.
		/// </summary>
		public virtual Qualifier Qualifier
		{
			set { qualifier = value; }
			get { return qualifier; }
		}

		/// <summary>
		/// The maximum number of objects to return, or -1 for no limit.
		/// </summary>
		public virtual int FetchLimit
		{
			set { fetchLimit = value; }
			get { return fetchLimit; }
		}

		/// <summary>
		/// An array of <c>PropertyComparer</c> that describes how the returned objects are to
		/// be sorted. First comparer has highest precedence.
		/// </summary>
		public virtual PropertyComparer[] SortOrderings 
		{
			set { sortOrderings = value; }
			get { return sortOrderings; }
		}

		/// <summary>
		/// An array of property paths to entities that should be fetch alongside the main
		/// entity.
		/// </summary>
		/// <remarks>
		/// Specifying a path will fetch all entities along that path. So, specifying
		/// "TitleAuthor.Author" for a query on the Title entity, will result in all title 
		///author link objects and all corresponding authors to be fetched.
		/// </remarks>

		public string[] Spans
		{
			set { spans = value; }
			get { return spans; }
		}

		//--------------------------------------------------------------------------------------
		//	Convenience methods
		//--------------------------------------------------------------------------------------

		/// <summary>
		/// Creates a new <c>PropertyComparer</c> from the arguments and adds it to the end of
		/// the list of SortOrderings.
		/// </summary>
		/// <param name="aPropName">property to sort on</param>
		/// <param name="aDirection">direction</param>
		public void AddSortOrdering(string aPropName, SortDirection aDirection)
		{
			PropertyComparer comparer = new PropertyComparer(aPropName, aDirection);
			if(sortOrderings == null)
			{
				sortOrderings = new PropertyComparer[]{ comparer };
			}
			else
			{
				PropertyComparer[] newOrderings = new PropertyComparer[sortOrderings.Length + 1];
				sortOrderings.CopyTo(newOrderings, 0);
				newOrderings[sortOrderings.Length] = comparer;
				sortOrderings = newOrderings;
			}
		}

		/// <summary>
		/// Adds the property path to the list of spans.
		/// </summary>
		/// <param name="aSpan">path to be added</param>
		/// <remarks>
		/// See Spans property for details.
		/// </remarks>
		public void AddSpan(string aSpan)
		{
			AddSpans(aSpan);
		}

		/// <summary>
		/// Adds the property paths to the list of spans.
		/// </summary>
		/// <param name="someSpans">paths to be added</param>
		/// <remarks>
		/// See Spans property for details.
		/// </remarks>
		public void AddSpans(params string[] someSpans)
		{
			if(spans == null)
			{
				spans = someSpans;
			}
			else
			{
				string[] newSpans = new string[spans.Length + someSpans.Length];
				spans.CopyTo(newSpans, 0);
				someSpans.CopyTo(newSpans, spans.Length);
				spans = newSpans;
			}
		}
	}
}
