using System.Data;
using Neo.Core;
using Neo.Core.Util;
using NUnit.Framework;
using Pubs4.Model;


namespace Neo.Tests.Fixtures
{
	[NUnit.Framework.TestFixture]
	public class EntityMapTests : TestBase
	{
		protected DefaultEntityMapFactory EmapFactory;
		

		[NUnit.Framework.SetUp]
		public void SetUp()
		{
			SetupLog4Net();
			EmapFactory = DefaultEntityMapFactory.SharedInstance;
		}


		[NUnit.Framework.Test]
		public void EntityMapRegistry()
		{
		    Assertion.AssertEquals("Wrong number of maps types.", 7, EmapFactory.GetRegisteredTypes().Count);
			Assertion.AssertEquals("Wrong number of maps.", 7, EmapFactory.GetAllMaps().Count);
		}


		[Test]
		public void SchemaGeneration()
		{
		    DataSet			dataset;
			DataTable		titleTable;
			DataColumn		salesColumn;

			dataset = new DataSet();
			EmapFactory.GetMap(typeof(Title)).UpdateSchemaInDataSet(dataset, SchemaUpdate.Full);
			
			titleTable = dataset.Tables["titles"];
			Assertion.AssertNotNull("Could not find titles table.", titleTable);
			Assertion.AssertNotNull("Could not find publishers.titles relation.", dataset.Relations["publishers.titles"]);
			Assertion.AssertEquals("Wrong number of columns in titles.", 10, titleTable.Columns.Count);
			salesColumn = titleTable.Columns["ytd_sales"];
			Assertion.AssertNotNull("Could not find sales column.", salesColumn);
			Assertion.AssertEquals("Wrong type of sales column.", typeof(int), salesColumn.DataType);
		}
	}
}
