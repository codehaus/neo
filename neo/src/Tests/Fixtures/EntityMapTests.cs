using System.Data;
using Neo.Core;
using Neo.Core.Util;
using NUnit.Framework;
using Pubs4.Model;


namespace Neo.Tests.Fixtures
{
	[TestFixture]
	public class EntityMapTests : TestBase
	{
		protected DefaultEntityMapFactory emapFactory;
		

		[SetUp]
		public void SetUp()
		{
			SetupLog4Net();
			emapFactory = DefaultEntityMapFactory.SharedInstance;
		}


		[Test]
		public void EntityMapRegistry()
		{
			// We create a fresh instance because some other tests register custom mappings which
			// would spoil the second count.
			emapFactory = new DefaultEntityMapFactory();

			Assertion.AssertEquals("Wrong number of maps types.", 7, emapFactory.GetRegisteredTypes().Count);
			Assertion.AssertEquals("Wrong number of maps.", 7, emapFactory.GetAllMaps().Count);
		}


		[Test]
		public void SchemaGeneration()
		{
			DataSet			dataset;
			DataTable		titleTable;
			DataColumn		salesColumn;

			dataset = new DataSet();
			emapFactory.GetMap(typeof(Title)).UpdateSchemaInDataSet(dataset, SchemaUpdate.Full);
			
			titleTable = dataset.Tables["titles"];
			Assertion.AssertNotNull("Could not find titles table.", titleTable);
			Assertion.AssertNotNull("Could not find publishers.titles relation.", dataset.Relations["publishers*titles.pub_id"]);
			Assertion.AssertEquals("Wrong number of columns in titles.", 10, titleTable.Columns.Count);
			salesColumn = titleTable.Columns["ytd_sales"];
			Assertion.AssertNotNull("Could not find sales column.", salesColumn);
			Assertion.AssertEquals("Wrong type of sales column.", typeof(int), salesColumn.DataType);
		}
	}
}
