using Neo.Core.Util;
using NUnit.Framework;


namespace Neo.Tests.Fixtures
{
	[TestFixture]
	public class OrderedTableCollectionTests : TestBase
	{
		OrderedTableCollection tables;
		
		[Test]
		public void testSortsPubsTablesCorrectly()
		{
			tables = new OrderedTableCollection(GetTestDataSet());

			Assert.IsTrue(IndexOf("publishers") < IndexOf("titles"), "publishers should come before titles");
			Assert.IsTrue(IndexOf("titles") < IndexOf("titleauthor"), "titles should come before titleauthor");
			Assert.IsTrue(IndexOf("author") < IndexOf("titleauthor"), "author should come before titleauthor");
		}


		private int IndexOf(string name)
		{
			for(int i = 0; i < tables.Count; i++)
			{
				if(tables[i].TableName == name)
					return i;
			}
			return -1;
		}

	}
}
