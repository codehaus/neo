using System.Globalization;
using Neo.Core;
using NUnit.Framework;
using Pubs4.Model;


namespace Neo.Tests.Fixtures
{
	
	[TestFixture]
	public class PropertyComparerTests : TestBase
	{
		protected ObjectContext	context;


		[SetUp]
		public void LoadDataSetAndGetTitleObject()
		{
			SetupLog4Net();
			
			context = new ObjectContext();
			context.MergeData(GetTestDataSet());
		}


		[Test]
		public void SimpleSort()
		{
			TitleList	titles;

			titles = new TitleList(new PublisherFactory(context).FindObject("0736").Titles);
		    Assert.AreEqual(5, titles.Count, "Wrong number of titles.");
			
			titles.Sort(new PropertyComparer("Advance", SortDirection.Ascending));
			for(int i = 1; i < titles.Count; i++)
				Assert.IsTrue(titles[i - 1].Advance <= titles[i].Advance, "Wrong ordering; not ascending.");

			titles.Sort(new PropertyComparer("Advance", SortDirection.Descending));
			for(int i = 1; i < titles.Count; i++)
				Assert.IsTrue(titles[i - 1].Advance >= titles[i].Advance, "Wrong ordering; not descending.");

			// try again on the off change that they were pre-sorted and the first didn't really work
			titles.Sort(new PropertyComparer("Advance", SortDirection.Ascending));
			for(int i = 1; i < titles.Count; i++)
				Assert.IsTrue(titles[i - 1].Advance <= titles[i].Advance, "Wrong ordering; not ascending again.");
		}


		[Test]
		public void CaseInsensitiveSortOnStrings()
		{
			TitleFactory factory;
			TitleList	 titles;
			Title		 t;

			factory = new TitleFactory(context);
			titles = new TitleList();
			
			t = factory.CreateObject("XX9002");   t.TheTitle = "mmm";   titles.Add(t);
			t = factory.CreateObject("XX9000");   t.TheTitle = "XYZ";   titles.Add(t);
			t = factory.CreateObject("XX9001");   t.TheTitle = "ABC";   titles.Add(t);

			titles.Sort(new PropertyComparer("TheTitle", SortDirection.AscendingCaseInsensitive));
			Assert.AreEqual("ABC", titles[0].TheTitle, "Wrong ordering; not ascending.");
			Assert.AreEqual("mmm", titles[1].TheTitle, "Wrong ordering; not ascending.");
			Assert.AreEqual("XYZ", titles[2].TheTitle, "Wrong ordering; not ascending.");
	
			titles.Sort(new PropertyComparer("TheTitle", SortDirection.DescendingCaseInsensitive));
			Assert.AreEqual("XYZ", titles[0].TheTitle, "Wrong ordering; not ascending.");
			Assert.AreEqual("mmm", titles[1].TheTitle, "Wrong ordering; not ascending.");
			Assert.AreEqual("ABC", titles[2].TheTitle, "Wrong ordering; not ascending.");
		}
	

		[Test]
		public void CultureBasedSortOnStrings()
		{
			CultureInfo	 culture;
			TitleFactory factory;
			TitleList	 titles;
			Title		 t;

			// culture identifier 0x040A (Spanish - Spain, traditional sort)
			// in this culture 'll' is a letter that comes after 'l' in the alphabet
			culture = new CultureInfo(0x040A, false);

			factory = new TitleFactory(context);
			titles = new TitleList();

			t = factory.CreateObject("XX9002");   t.TheTitle = "llegar";	titles.Add(t);
			t = factory.CreateObject("XX9000");   t.TheTitle = "lugar";		titles.Add(t);

			titles.Sort(new PropertyComparer("TheTitle", SortDirection.AscendingCaseInsensitive, culture));
			Assert.AreEqual("lugar", titles[0].TheTitle, "Wrong ordering; not ascending.");
			Assert.AreEqual("llegar", titles[1].TheTitle, "Wrong ordering; not ascending.");
	
			titles.Sort(new PropertyComparer("TheTitle", SortDirection.DescendingCaseInsensitive, culture));
			Assert.AreEqual("llegar", titles[0].TheTitle, "Wrong ordering; not descending.");
			Assert.AreEqual("lugar", titles[1].TheTitle, "Wrong ordering; not descending.");
		}


		// This is on ObjectListBase

		[Test]
		public void SortList()
		{
			TitleList	titles, sortedTitles;

			titles = new TitleFactory(context).FindAllObjects();

			// we know this works
			sortedTitles = new TitleList(titles);
			sortedTitles.Sort("TitleId", SortDirection.AscendingCaseInsensitive);					

			titles.Sort("TitleId", SortDirection.AscendingCaseInsensitive);
			for(int i = 0; i < titles.Count; i++)
				Assert.AreEqual(sortedTitles[i], titles[i], "Sorting failed.");
		}
	

		// This is on ObjectRelationBase

		[Test]
		public void SortedListFromRelation()
		{
			TitleRelation	titles;
			TitleList		sortedTitles, sortedTitlesFromRel;

			titles = new PublisherFactory(context).FindObject("0736").Titles;

			// we know this works
			sortedTitles = new TitleList(titles);
			sortedTitles.Sort("TitleId", SortDirection.AscendingCaseInsensitive);					

			sortedTitlesFromRel = titles.GetSortedList("TitleId", SortDirection.AscendingCaseInsensitive);
			for(int i = 0; i < titles.Count; i++)
				Assert.AreEqual(sortedTitles[i], sortedTitlesFromRel[i], "Sorting failed.");
		}

	}

}
