using System;
using System.Collections;


namespace Neo.Core
{
	/// <summary>
	/// Entity maps Manage the mapping from object types to DataTables. Returns an 
	/// instance of a <c>IEntityMap</c> object depending upon the request type
	/// </summary>
	public interface IEntityMapFactory
	{
		/// <summary>
		/// Gets a collection of all Object Type Maps
		/// </summary>
		/// <returns>A collection of all Object Type Maps</returns>
		ICollection GetAllMaps();

		/// <summary>
		/// Gets a map to translate between a Type and a Table
		/// </summary>
		/// <param name="type">Type to map</param>
		/// <returns>IEntityMap object matching the supplied Type</returns>
		IEntityMap GetMap(Type type);

		/// <summary>
		/// Gets a map to translate between a Table and a Type
		/// </summary>
		/// <param name="tablename">Table Name to map</param>
		/// <returns>IEntityMap object matching the supplied table name</returns>
		IEntityMap GetMap(string tablename);

		/// <summary>
		/// Adds a custom mapping
		/// </summary>
		void AddCustomType(Type objType, IEntityMap map);

	}

}
