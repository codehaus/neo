using System;
using System.Collections;
using System.Data;
using NUnit.Framework;
using Neo.Core;
using Pubs4.Model;


namespace Neo.Tests
{
	[TestFixture]
	public class ObjectCollectionTests : TestBase
	{
		protected ObjectContext	context;


		[SetUp]
		public void LoadDataSet()
		{
			SetupLog4Net();
			
			context = new ObjectContext(GetTestDataSet());
		}

	
		[Test]
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
			Title		foundTitle;

			publisher = new PublisherFactory(context).FindObject("0877");
			try
			{
				foundTitle = publisher.Titles.FindUnique("Publisher", publisher);
				Console.WriteLine(foundTitle.TheTitle);
			}
			catch (NotUniqueException e)
			{
				Assertion.AssertEquals("Incorrect exception message",
					"[NEO] The query 'Publisher' for parameters '" + publisher.ToString() + "' was not unique.",
					e.Message);
				throw e;
			}
			Assertion.Fail("Should have thrown a NotUniqueException.");
		}

		[Test]
		[ExpectedException(typeof(NotUniqueException))]
		public void FindingObjectsUniqueNoInstances()
		{
			Publisher	publisher;
			Title		foundTitle;

			publisher = new PublisherFactory(context).FindObject("1622");
			Assertion.AssertNotNull("Found wrong publisher.", publisher);
			try
			{
				foundTitle = publisher.Titles.FindUnique("Publisher = {0} AND Royalty = {1}", publisher, 10);
			}
			catch (NotUniqueException e)
			{
				Console.WriteLine(e.Message);
				Assertion.AssertEquals("Incorrect exception message",
					"[NEO] The query 'Publisher = {0} AND Royalty = {1}' for parameters '" + publisher.ToString() + "', '10' returned zero matches.",
					e.Message);
				throw e;
			}
			Assertion.Fail("Should have thrown a NotUniqueException.");
		}

		[Test]
		public void FindingObjectsSingleOneInstance()
		{
			Publisher	publisher;
			Title		originalTitle;
			Title		foundTitle;

			originalTitle = new TitleFactory(context).FindObject("TC7777");
			publisher = new PublisherFactory(context).FindObject("0877");
			foundTitle = publisher.Titles.FindSingle("TitleId", "TC7777");
			Assertion.AssertEquals("Found wrong title.", originalTitle, foundTitle);
		}


		[Test]
		public void FindingObjectsSingleNoInstance()
		{
			Publisher	publisher;
			Title		foundTitle;

			publisher = new PublisherFactory(context).FindObject("1622");
			Assertion.AssertNotNull("Found wrong publisher.", publisher);
			foundTitle = publisher.Titles.FindSingle("Publisher", publisher);
			Assertion.AssertNull("Found wrong title.", foundTitle);
		}

		[Test]
		[ExpectedException(typeof(NotUniqueException))]
		public void FindingObjectsSingleMultipleInstances()
		{
			Publisher	publisher;
			Title		originalTitle;
			Title		foundTitle;

			originalTitle = new TitleFactory(context).FindObject("TC7777");
			publisher = new PublisherFactory(context).FindObject("0877");
			
			try
			{
				foundTitle = publisher.Titles.FindSingle("Type = {0} AND Royalty = {1}", "trad_cook   ", "10");
				Console.WriteLine(foundTitle.TheTitle);
			}
			catch (NotUniqueException e)
			{
				Assertion.AssertEquals("Incorrect exception message.", 
					"[NEO] The query 'Type = {0} AND Royalty = {1}' for parameters 'trad_cook   ', '10' was not unique.",
					e.Message);
				throw e;
			}

			Assertion.Fail("Should have thrown a NotUniqueException.");
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
			Assertion.Fail("Should not have allowed mutation.");
		}

	}
}
