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

			Assert.AreEqual( 7, emapFactory.GetRegisteredTypes().Count, "Wrong number of maps types.");
			Assert.AreEqual(7, emapFactory.GetAllMaps().Count, "Wrong number of maps.");
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
			Assert.IsNotNull(titleTable, "Could not find titles table.");
			Assert.IsNotNull(dataset.Relations["publishers*titles.pub_id"], "Could not find publishers.titles relation.");
			Assert.AreEqual(10, titleTable.Columns.Count, "Wrong number of columns in titles.");
			salesColumn = titleTable.Columns["ytd_sales"];
			Assert.IsNotNull(salesColumn, "Could not find sales column.");
			Assert.AreEqual(typeof(int), salesColumn.DataType, "Wrong type of sales column.");
		}
	}
}
