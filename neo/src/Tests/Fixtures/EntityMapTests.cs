using System;
using System.Data;
using NUnit.Framework;
using Neo.Core;
using Neo.Core.Util;


namespace Neo.Tests
{
	[TestFixture]
	public class EntityMapTests : TestBase
	{
		protected DefaultEntityMapFactory EmapFactory;
		

		[SetUp]
		public void SetUp()
		{
			SetupLog4Net();
			EmapFactory = DefaultEntityMapFactory.SharedInstance;
		}


		[Test]
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
			EmapFactory.GetMap(typeof(Pubs4.Model.Title)).UpdateSchemaInDataSet(dataset, SchemaUpdate.Full);
			
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
