using System;


namespace Neo.Core
{
	/// <summary>
	/// Defines which objects should be fetched from a context or datastore.
	/// </summary>
	/// <remarks>
	/// The <c>FetchSpecification</c> class is a straigh-forward implementation of this interface 
	/// which allows to set all aspects directly. The Code Generator creates query templates for 
	/// each entity that also implement <c>IFetchSpecification</c> and provide a strongly typed API.
	/// </remarks>
	public interface IFetchSpecification
	{
		/// <summary>
		/// Entity map used to specify the entity from which to fetch the objects.
		/// </summary>
		IEntityMap EntityMap { get; }

		/// <summary>
		/// The <c>Qualifier</c> to apply to the fetched objects.
		/// </summary>
		Qualifier Qualifier { get; }

		/// <summary>
		/// The maximum number of objects to return, or -1 for no limit.
		/// </summary>
		Int32 FetchLimit { get; }

		/// <summary>
		/// If true, objects in memory will be updated with the values returned by
		/// the fetch. If false, in-memory values for objects that the context knew about 
		/// before the fetch are kept.
		/// </summary>
		RefreshStrategy RefreshStrategy { get; }

		/// <summary>
		/// An array of <c>PropertyComparer</c> that describes how the returned objects are to
		/// be sorted. First comparer has highest precedence.
		/// </summary>
		PropertyComparer[] SortOrderings { get; }

		/// <summary>
		/// An array of paths to entities from which related objects are fetched with this
		/// query.
		/// </summary>
		string[] Spans { get; }

	}

	/// <summary>
	/// Values for specifying which refresh strategy <c>ObjectContext</c> should use for known
	/// objects retrieved by a fetch.
	/// </summary>
	public enum RefreshStrategy
	{
		/// <summary>
		/// Keep what is in memory, ignore values from refetched objects.
		/// </summary>
		Keep,
		/// <summary>
		/// Update object in memory, overwrite local changes with database contents.
		/// </summary>
		Update,
		/// <summary>
		/// Update object in memory, but keep local changes
		/// </summary>
		Merge
	}

}
