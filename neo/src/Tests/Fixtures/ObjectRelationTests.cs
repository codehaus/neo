using Neo.Core;
using NUnit.Framework;
using Pubs4.Model;


namespace Neo.Tests.Fixtures
{
	[NUnit.Framework.TestFixture]
	public class ObjectRelationTests : TestBase
	{
		protected ObjectContext	context;


		[NUnit.Framework.SetUp]
		public void LoadDataSet()
		{
			SetupLog4Net();
			
			context = new ObjectContext();
			context.MergeData(GetTestDataSet());
		}

	
		[NUnit.Framework.Test]
		public void GetList()
		{
		    Publisher		publisher;
			TitleList		titleList;

			publisher = new PublisherFactory(context).FindObject("0877");
			titleList = publisher.Titles.GetReadOnlyList();
		    Assertion.AssertEquals("Count differs.", publisher.Titles.Count, titleList.Count);
			for(int i = 0; i < titleList.Count; i++)
				Assertion.AssertEquals("Objects differ.", publisher.Titles[i], titleList[i]);
			Assertion.Assert("List not read-only.", titleList.IsReadOnly);
		}

	}
}
