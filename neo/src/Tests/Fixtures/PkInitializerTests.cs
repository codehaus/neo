using System;
using System.Collections;
using System.Data;
using NUnit.Framework;
using Neo.Core;
using Neo.Framework;
using Pubs4.Model;


namespace Neo.Tests
{
	[TestFixture]
	public class PkInitializerTests : TestBase
	{
		protected DataTable		table;
		protected DataColumn	pkColumn;

		[SetUp]
		public void SetUp()
		{
			table = new DataTable();
			pkColumn = table.Columns.Add("ID");
			table.PrimaryKey = new DataColumn[] { pkColumn };
		}

	
		[Test]
		public void NewGuidInitializer()
		{
			IPkInitializer	initializer;
			DataRow			row, row2;
			
			pkColumn.DataType = typeof(Guid);
			
			initializer = new NewGuidPkInitializer();

			row = table.NewRow();
			initializer.InitializeRow(row, null);

			row2 = table.NewRow();
			initializer.InitializeRow(row2, null);

			Assertion.AssertNotNull("Should have set value.", row["ID"]);
			Assertion.AssertNotNull("Should create different values.", row2["ID"]);
		}

	}
}
