using System;
using System.Data;


namespace Neo.Core
{
	/// <summary>
	/// Describes the mapping from ADO.NET concepts, such as DataTable, DataColumn, to
	/// the corresponding entity object concepts; and vice versa.
	/// </summary>

	public interface IEntityMap
	{	
		/// <summary>
		/// The <c>IEntityMapFactory</c> object used to generate an instance of this object
		/// </summary>
		IEntityMapFactory Factory { get; set; }

		/// <summary>
		/// This object&apos;s type
		/// </summary>
		Type ObjectType { get; }

		/// <summary>
		/// The type that is really created. Should be a subclass of ObjectType
		/// </summary>
		Type ConcreteObjectType { get; set; }

		/// <summary>
		/// Table Name (usually a SQL table name, but may be other sort)
		/// </summary>
		string TableName { get; }

		/// <summary>
		/// All primary key columns for this table
		/// </summary>
		string[] PrimaryKeyColumns { get; }

		/// <summary>
		/// All columns within this table
		/// </summary>
		string[] Columns { get; }

		/// <summary>
		/// All attributes within this type
		/// </summary>
		/// <remarks>
		/// The name Attribute refers to Entity-Relationship modelling terminology. An
		/// attribute is usually implemented using a property in the corresponding type.
		/// </remarks>
		string[] Attributes { get; }
		
		/// <summary>
		/// Gets an object to create primary keys
		/// </summary>
		/// <returns>An object to create primary keys for this entity</returns>
		IPkInitializer GetPkInitializer();
		
		/// <summary>
		/// Translates between Attribute name and column name
		/// </summary>
		/// <param name="attribute">attribute name</param>
		/// <returns>corresponding column name</returns>
		string GetColumnForAttribute(string attribute);

		/// <summary>
		/// Creates an instance for this entity, a blank entity object
		/// </summary>
		/// <param name="row">The row that is represented by this object</param>
		/// <param name="context">The context with which the object is associated with</param>
		/// <returns></returns>
		IEntityObject CreateInstance(DataRow row, ObjectContext context);

		/// <summary>
		/// Adds or updates ADO.NET schema information for the entity to the data set.
		/// </summary>
		/// <param name="aDataSet">The dataset to work on</param>
		/// <param name="update">Flags specifying what kind of schema information to add/update</param>
		void UpdateSchemaInDataSet(DataSet aDataSet, SchemaUpdate update);

		/// <summary>
		/// Adds or updates ADO.NET schema information for the entity in the table.
		/// </summary>
		/// <remarks>
		/// The table passed must match the entity map's.
		/// </remarks>
		/// <param name="table">The table to work on</param>
		/// <param name="update">Flags specifying what kind of schema information to add/update</param>		
		void UpdateSchema(DataTable table, SchemaUpdate update);
	}

	[Flags]
	public enum SchemaUpdate
	{
		Basic		= 0x01,
		Constraints = 0x02,
		Relations	= 0x04,
		Full		= 0x07
	}

}
