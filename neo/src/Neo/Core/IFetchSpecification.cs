using System;


namespace Neo.Core
{
	/// <summary>
	/// <c>IFetchSpecification</c> is used to select objects in object contexts and data stores.
	/// </summary>
	/// <remarks>
	/// Concrete implementations of this class may be used to form templates for finding similar objects
	/// </remarks>
	public interface IFetchSpecification
	{
		/// <summary>
		/// Entity map translating Attributes to Columns and vice versa
		/// </summary>
		IEntityMap EntityMap { get; }

		/// <summary>
		/// Specifies which rows to get
		/// </summary>
		Qualifier Qualifier { get; }

		/// <summary>
		/// Specifies how many object to get.
		/// </summary>
		Int32 FetchLimit { get; }

		/// <summary>
		/// Specifies the order in which the objects are returned. First comparer has highest precedence.
		/// </summary>
		PropertyComparer[] SortOrderings { get; }
	}

}
