using System;
using System.Data;
using Neo.Core;
using Neo.Framework;
using NUnit.Framework;


namespace Neo.Tests.Fixtures
{
	[NUnit.Framework.TestFixture]
	public class PkInitializerTests : TestBase
	{
		protected DataTable		table;
		protected DataColumn	pkColumn;

		[NUnit.Framework.SetUp]
		public void SetUp()
		{
			table = new DataTable();
			pkColumn = table.Columns.Add("ID");
			table.PrimaryKey = new DataColumn[] { pkColumn };
		}

	
		[NUnit.Framework.Test]
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
