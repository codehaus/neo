using System;
using System.Collections;
using Neo.Core;
using NUnit.Framework;
using Pubs4.Model;


namespace Neo.Tests
{
	[NUnit.Framework.TestFixture]
	public class ObjectCollectionTests : TestBase
	{
		protected ObjectContext	context;


		[NUnit.Framework.SetUp]
		public void LoadDataSet()
		{
			SetupLog4Net();
			
			context = new ObjectContext(GetTestDataSet());
		}

	
		[NUnit.Framework.Test]
		public void FindingObjectsSimple()
		{
		    Publisher	publisher;
			Title		originalTitle;
			TitleList	foundTitles;

			originalTitle = new TitleFactory(context).FindObject("TC7777");
			publisher = new PublisherFactory(context).FindObject("0877");
			foundTitles = publisher.Titles.Find("TitleId", "TC7777");
		    Assertion.AssertEquals("Found wrong number of titles.", 1, foundTitles.Count);
			Assertion.AssertEquals("Found wrong title.", originalTitle, foundTitles[0]);
		}

		
		[Test]
		public void FindingObjectsRel()
		{
			Publisher	publisher;
			TitleList	allTitles, matchingTitles;

			publisher = new PublisherFactory(context).FindObject("0877");
			allTitles = new TitleFactory(context).FindAllObjects();
			matchingTitles = allTitles.Find("Publisher", publisher);
			Assertion.AssertEquals("Found wrong number of titles.", publisher.Titles.Count, matchingTitles.Count);
		}


		[Test]
		public void FindingObjectsUnique()
		{
			Publisher	publisher;
			Title		originalTitle;
			Title		foundTitle;

			originalTitle = new TitleFactory(context).FindObject("TC7777");
			publisher = new PublisherFactory(context).FindObject("0877");
			foundTitle = publisher.Titles.FindUnique("TitleId", "TC7777");
			Assertion.AssertEquals("Found wrong title.", originalTitle, foundTitle);
		}


		[Test]
		[ExpectedException(typeof(NotUniqueException))]
		public void FindingObjectsUniqueMultipleInstances()
		{
			Publisher	publisher;

			publisher = new PublisherFactory(context).FindObject("0877");
			publisher.Titles.FindUnique("Publisher", publisher);
		}

		[Test]
		[ExpectedException(typeof(NotUniqueException))]
		public void FindingObjectsUniqueNoInstances()
		{
			Publisher	publisher;

			publisher = new PublisherFactory(context).FindObject("1622");
			Assertion.AssertNotNull("Found wrong publisher.", publisher);
			publisher.Titles.FindUnique("Publisher = {0} AND Royalty = {1}", publisher, 10);
		}

		[Test]
		public void FindingFirstObjectOneInstance()
		{
			Publisher	publisher;
			Title		originalTitle;
			Title		foundTitle;

			originalTitle = new TitleFactory(context).FindObject("TC7777");
			publisher = new PublisherFactory(context).FindObject("0877");
			foundTitle = publisher.Titles.FindFirst("TitleId", "TC7777");
			Assertion.AssertEquals("Found wrong title.", originalTitle, foundTitle);
		}


		[Test]
		public void FindingFirstObjectNoInstance()
		{
			Publisher	publisher;
			Title		foundTitle;

			publisher = new PublisherFactory(context).FindObject("1622");
			Assertion.AssertNotNull("Found wrong publisher.", publisher);
			foundTitle = publisher.Titles.FindFirst("Publisher", publisher);
			Assertion.AssertNull("Found wrong title.", foundTitle);
		}


		[Test]
		public void FindObjectsByNull()
		{
			Publisher	publisher;
			TitleList	matchingTitles;

			publisher = new PublisherFactory(context).FindObject("0877");
			matchingTitles = publisher.Titles.Find("Notes", null);
			Assertion.AssertEquals("Wrong number of titles.", 1, matchingTitles.Count);
			Assertion.AssertEquals("Wrong title object.", "MC3026", matchingTitles[0].TitleId);
		}


		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void MakeReadOnly()
		{
			TitleList	titles;
			int			countBefore;

			titles = new TitleFactory(context).FindAllObjects();
			countBefore = titles.Count;
			Assertion.Assert("Too few titles.", countBefore > 1);
			titles.Remove(titles[0]);
			Assertion.Assert("Removal failed.", titles.Count == countBefore - 1);
			titles.MakeReadOnly();
			titles.Remove(titles[0]);
		}
		

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void MakeReadOnlyAndExplicitIListImpl()
		{
		    IList	titles;

			titles = new TitleFactory(context).FindAllObjects();
			Assertion.Assert("Too few titles.", titles.Count > 2);
			Assertion.Assert("First titles must not be the same.", titles[0].Equals(titles[1]) == false);
			titles[0] = titles[1];
			Assertion.Assert("Assignment failed.", titles[0].Equals(titles[1]));
			((TitleList)titles).MakeReadOnly();
			titles[0] = titles[1];
		}

	}
}
