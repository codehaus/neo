using System;
using System.Data;
using Neo.Core.Util;


namespace Neo.Core
{
	/// <summary>
	/// Describes the mapping from ADO.NET concepts, such as DataTable and DataColumn, to
	/// the corresponding entity object concepts; and vice versa.
	/// </summary>
	/// <remarks>
	/// EntityMaps should never be instantiated directly. Use the following code to
	/// retrieve an <c>IEntityMap</c> for a given type:
	/// <code>
	/// IEntityMap map = context.EntityMapFactory.GetMap(typeof(TheType));
	/// </code>
	/// </remarks>

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
		/// All relations withing this type
		/// </summary>
		string[] Relations { get; }

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
		/// Translates between Relation name and relation info
		/// </summary>
		/// <param name="relation">relation name</param>
		/// <returns>corresponding relation info</returns>
		RelationInfo GetRelationInfo(string relation);

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

	/// <summary>
	/// Flags for specifying which aspects of the schema an <c>EntityMap</c> should write into
	/// a <c>DataTable</c>.
	/// </summary>
	[Flags]
	public enum SchemaUpdate
	{
		/// <summary>
		/// Write the table definition
		/// </summary>
		Basic		= 0x01,
		/// <summary>
		/// Write the constraints
		/// </summary>
		Constraints = 0x02,
		/// <summary>
		/// Write all related table and the relations between this table and the others
		/// </summary>
		Relations	= 0x04,
		/// <summary>
		/// Write everything
		/// </summary>
		Full		= 0x07
	}

}
