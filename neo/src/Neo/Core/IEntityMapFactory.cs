using System;
using System.Collections;


namespace Neo.Core
{
	/// <summary>
	/// Provides <c>IEntityMap</c> implementations for given types
	/// and/or table names.
	/// </summary>
	/// <remarks>
	/// EntityMaps should never be instantiated directly. Use the following code to
	/// retrieve an <c>IEntityMap</c> for a given type:
	/// <code>
	/// IEntityMap map = context.EntityMapFactory.GetMap(typeof(TheType));
	/// </code>
	/// </remarks>
	public interface IEntityMapFactory
	{
		/// <summary>
		/// Gets a collection of all maps
		/// </summary>
		/// <returns>A collection of all Object Type Maps</returns>
		ICollection GetAllMaps();

		/// <summary>
		/// Gets a map to translate between a type and a table
		/// </summary>
		/// <param name="type">Type to map</param>
		/// <returns>IEntityMap object matching the supplied type</returns>
		IEntityMap GetMap(Type type);

		/// <summary>
		/// Gets a map to translate between a table and a type
		/// </summary>
		/// <param name="tablename">Table Name to map</param>
		/// <returns>IEntityMap object matching the supplied table name</returns>
		IEntityMap GetMap(string tablename);

		/// <summary>
		/// Adds a custom mapping. Note that multiple types can be mapped to one
		/// <c>IEntityMap</c>.
		/// </summary>
		void AddCustomType(Type objType, IEntityMap map);

	}

}
