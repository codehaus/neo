using System;
using System.Data;


namespace Neo.Core
{
	public interface IEntityMap
	{	
		IEntityMapFactory Factory { get; set; }

		Type ObjectType { get; }
		string TableName { get; }
		string[] PrimaryKeyColumns { get; }
		string[] Columns { get; }
		string[] Attributes { get; }
		
		IPkInitializer GetPkInitializer();
		string GetColumnForAttribute(string attribute);

		void UpdateSchemaInDataSet(DataSet aDataSet, SchemaUpdate update);
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
