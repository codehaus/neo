using System;
using System.Collections;
using System.ComponentModel;
using Neo.Core;
using Neo.Framework;
using NUnit.Framework;
using Pubs4.Model;


namespace Neo.Tests.Fixtures
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



		// We are testing the collection functionality through the list subclass, which
		// does not add to the IBindingList implementation.

		[Test]
		public void SendsNotifcationOnClear()
		{
			TitleList		titleList;
			EventRecorder	recorder;

			titleList = new TitleList();
			recorder = new EventRecorder(titleList);

			titleList.Clear();
			
			Assertion.Assert("Event should have been fired", recorder.changeHandlerWasCalled);
			Assertion.AssertEquals("Event of correct type should have been fired.", ListChangedType.Reset, recorder.receivedChangeType);
		}


		[Test]
		public void SendsNotifcationOnAdd()
		{
			TitleList		titleList;
			Title			title;
			EventRecorder	recorder;

			titleList = new TitleList();
			title = new TitleFactory(context).CreateObject("XX9999");
			recorder = new EventRecorder(titleList);
			
			titleList.Add(title);
			
			Assertion.Assert("Event should have been fired", recorder.changeHandlerWasCalled);
			Assertion.AssertEquals("Event of correct type should have been fired.", ListChangedType.ItemAdded, recorder.receivedChangeType);
		}


		[Test]
		public void SendsNotifcationOnRemove()
		{
			TitleList		titleList;
			Title			title;
			EventRecorder	recorder;

			titleList = new TitleList();
			title = new TitleFactory(context).CreateObject("XX9999");
			titleList.Add(title);
			recorder = new EventRecorder(titleList);

			titleList.Remove(title);

			Assertion.Assert("Event should have been fired", recorder.changeHandlerWasCalled);
			Assertion.AssertEquals("Event of correct type should have been fired.", ListChangedType.ItemDeleted, recorder.receivedChangeType);
		}


		#region Helper class: Event Recorder

		private class EventRecorder
		{
			public ListChangedType receivedChangeType;
			public bool changeHandlerWasCalled = false;

			public EventRecorder(ObjectCollectionBase collection)
			{
				IBindingList bindingList = collection;
				Assertion.Assert("Collection must support notification", bindingList.SupportsChangeNotification);
				collection.ListChanged += new ListChangedEventHandler(Titles_ListChanged);
			}
		
			private void Titles_ListChanged(object sender, ListChangedEventArgs e)
			{
				changeHandlerWasCalled = true;
				receivedChangeType = e.ListChangedType;
			}

		}

		#endregion

	}
}
