using System.ComponentModel;
using Neo.Core;
using Neo.Framework;
using NUnit.Framework;
using Pubs4.Model;


namespace Neo.Tests.Fixtures
{
	[TestFixture]
	public class ObjectRelationTests : TestBase
	{
		protected ObjectContext	context;


		[SetUp]
		public void LoadDataSet()
		{
			SetupLog4Net();
			
			context = new ObjectContext();
			context.MergeData(GetTestDataSet());
		}

	
		[Test]
		public void GetList()
		{
			Publisher		publisher;
			TitleList		titleList;

			publisher = new PublisherFactory(context).FindObject("0877");
			titleList = publisher.Titles.GetReadOnlyList();
		    Assert.AreEqual(publisher.Titles.Count, titleList.Count, "Count differs.");
			for(int i = 0; i < titleList.Count; i++)
				Assert.AreEqual(publisher.Titles[i], titleList[i], "Objects differ.");
			Assert.IsTrue(titleList.IsReadOnly, "List not read-only.");
		}


		[Test]
		public void ListHandlesPkChanges()
		{
			// I know that these PKs are not meant to change but when you work 
			// with db generated keys these do change on save and we need to be
			// able to handle this case.

			Title		title;
			int			countBefore;

			title = new TitleFactory(context).FindObject("MC3021");
			countBefore = title.TitleAuthors.Count;

			title.TitleId = "XX9999";

			Assert.AreEqual(countBefore, title.TitleAuthors.Count, "Should have kept list same");
		}


		[Test]
		public void SendsNotifcationOnFirstAccess()
		{
			Publisher		publisher;
			EventRecorder	recorder;

			publisher = new PublisherFactory(context).CreateObject("4711");
			recorder = new EventRecorder(publisher.Titles);
	
			publisher.Titles.Touch();
			
			Assert.IsTrue(recorder.changeHandlerWasCalled, "Event should have been fired");
			Assert.AreEqual(ListChangedType.Reset, recorder.receivedChangeType, "Event of correct type should have been fired.");
		}


		[Test]
		public void SendsNotifcationOnAdd()
		{
			Publisher		publisher;
			Title			title;
			EventRecorder	recorder;

			publisher = new PublisherFactory(context).FindObject("0877");
			title = new TitleFactory(context).CreateObject("XX9999");
			// Touch list first, otherwise we'll see a reset event.
			publisher.Titles.Touch();
			// Now the real operation...
			recorder = new EventRecorder(publisher.Titles);
			
			publisher.Titles.Add(title);
			
			Assert.IsTrue(recorder.changeHandlerWasCalled, "Event should have been fired");
			Assert.AreEqual(ListChangedType.ItemAdded, recorder.receivedChangeType, "Event of correct type should have been fired.");
		}


		[Test]
		public void SendsNotifcationOnRemove()
		{
			Publisher		publisher;
			EventRecorder	recorder;

			publisher = new PublisherFactory(context).FindObject("0877");
			recorder = new EventRecorder(publisher.Titles);
			
			publisher.Titles.Remove(publisher.Titles[0]);
			
			Assert.IsTrue(recorder.changeHandlerWasCalled, "Event should have been fired");
			Assert.AreEqual(ListChangedType.ItemDeleted, recorder.receivedChangeType, "Event of correct type should have been fired.");
		}


		[Test]
		[Ignore("Change notifications do not work yet.")]
		public void SendsNotifcationOnObjectChange()
		{
			Publisher		publisher;
			EventRecorder	recorder;

			publisher = new PublisherFactory(context).FindObject("0877");
			// Touch list first, otherwise we'll see a reset event.
			publisher.Titles.Touch();
			// Now the real operation...
			recorder = new EventRecorder(publisher.Titles);
			
			publisher.Titles[0].Advance += 1;
			
			Assert.IsTrue(recorder.changeHandlerWasCalled, "Event should have been fired");
			Assert.AreEqual(ListChangedType.ItemChanged, recorder.receivedChangeType, "Event of correct type should have been fired.");
		}


		#region Helper class: Event Recorder

		private class EventRecorder
		{
			public ListChangedType receivedChangeType;
			public bool changeHandlerWasCalled = false;

			public EventRecorder(ObjectRelationBase collection)
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
