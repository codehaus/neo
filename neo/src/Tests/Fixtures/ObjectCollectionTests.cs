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
		    Assert.AreEqual(1, foundTitles.Count, "Found wrong number of titles.");
			Assert.AreEqual(originalTitle, foundTitles[0], "Found wrong title.");
		}

		
		[Test]
		public void FindingObjectsRel()
		{
			Publisher	publisher;
			TitleList	allTitles, matchingTitles;

			publisher = new PublisherFactory(context).FindObject("0877");
			allTitles = new TitleFactory(context).FindAllObjects();
			matchingTitles = allTitles.Find("Publisher", publisher);
			Assert.AreEqual(publisher.Titles.Count, matchingTitles.Count, "Found wrong number of titles.");
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
			Assert.AreEqual(originalTitle, foundTitle, "Found wrong title.");
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
			Assert.IsNotNull(publisher, "Found wrong publisher.");
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
			Assert.AreEqual(originalTitle, foundTitle, "Found wrong title.");
		}


		[Test]
		public void FindingFirstObjectNoInstance()
		{
			Publisher	publisher;
			Title		foundTitle;

			publisher = new PublisherFactory(context).FindObject("1622");
			Assert.IsNotNull(publisher, "Found wrong publisher.");
			foundTitle = publisher.Titles.FindFirst("Publisher", publisher);
			Assert.IsNull(foundTitle, "Found wrong title.");
		}


		[Test]
		public void FindObjectsByNull()
		{
			Publisher	publisher;
			TitleList	matchingTitles;

			publisher = new PublisherFactory(context).FindObject("0877");
			matchingTitles = publisher.Titles.Find("Notes", null);
			Assert.AreEqual(1, matchingTitles.Count, "Wrong number of titles.");
			Assert.AreEqual("MC3026", matchingTitles[0].TitleId, "Wrong title object.");
		}


		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void MakeReadOnly()
		{
			TitleList	titles;
			int			countBefore;

			titles = new TitleFactory(context).FindAllObjects();
			countBefore = titles.Count;
			Assert.IsTrue(countBefore > 1, "Too few titles.");
			titles.Remove(titles[0]);
			Assert.IsTrue(titles.Count == countBefore - 1, "Removal failed.");
			titles.MakeReadOnly();
			titles.Remove(titles[0]);
		}
		

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void MakeReadOnlyAndExplicitIListImpl()
		{
		    IList	titles;

			titles = new TitleFactory(context).FindAllObjects();
			Assert.IsTrue(titles.Count > 2, "Too few titles.");
			Assert.IsTrue(titles[0].Equals(titles[1]) == false, "First titles must not be the same.");
			titles[0] = titles[1];
			Assert.IsTrue(titles[0].Equals(titles[1]), "Assignment failed.");
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
			
			Assert.IsTrue(recorder.changeHandlerWasCalled, "Event should have been fired");
			Assert.AreEqual(ListChangedType.Reset, recorder.receivedChangeType, "Event of correct type should have been fired.");
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
			
			Assert.IsTrue(recorder.changeHandlerWasCalled, "Event should have been fired");
			Assert.AreEqual(ListChangedType.ItemAdded, recorder.receivedChangeType, "Event of correct type should have been fired.");
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

			Assert.IsTrue(recorder.changeHandlerWasCalled, "Event should have been fired");
			Assert.AreEqual(ListChangedType.ItemDeleted, recorder.receivedChangeType, "Event of correct type should have been fired.");
		}


		[Test]
		public void ChecksWetherOtherCollectionContainsSameObjects()
		{
			TitleList		list1, list2;
			TitleFactory	factory;

			factory = new TitleFactory(context);
			list1 = new TitleList();
			list1.Add(factory.FindObject("TC7777"));
			list1.Add(factory.FindObject("MC3021"));
			list1.Add(factory.FindObject("MC3026"));
			list2 = new TitleList();
			list2.Add(factory.FindObject("TC7777"));
			list2.Add(factory.FindObject("MC3026"));
			list2.Add(factory.FindObject("MC3021"));

			Assert.IsTrue(list1.ContainsSameObjects(list2), "List contain same objects, should return true");
		}


		[Test]
		public void ChecksWetherOtherCollectionContainsSameObjectsWithNoRepeats()
		{
			TitleList		list1, list2;
			TitleFactory	factory;

			factory = new TitleFactory(context);
			list1 = new TitleList();
			list1.Add(factory.FindObject("TC7777"));
			list1.Add(factory.FindObject("MC3021"));
			list2 = new TitleList();
			list2.Add(factory.FindObject("MC3021"));
			list2.Add(factory.FindObject("MC3021"));

			Assert.IsFalse(list1.ContainsSameObjects(list2), "Second list does not contain all objects, should return false");
		}


		[Test]
		public void GetsPropertyForObjectsInCollection()
		{
			TitleRelation	titles;
			IList			publishers;

			titles = new PublisherFactory(context).FindObject("0877").Titles;
			publishers = titles.GetProperty("Publisher");
			
			Assert.AreEqual(titles.Count,  publishers.Count, "Should have same number of elements");
            for(int i = 0; i < titles.Count; i++)
				Assert.AreEqual(titles[i].Publisher, publishers[i], "Should have retrieved property");
		}


		#region Helper class: Event Recorder

		private class EventRecorder
		{
			public ListChangedType receivedChangeType;
			public bool changeHandlerWasCalled = false;

			public EventRecorder(ObjectCollectionBase collection)
			{
				IBindingList bindingList = collection;
				Assert.IsTrue(bindingList.SupportsChangeNotification, "Collection must support notification");
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
