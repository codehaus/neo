using System.Collections;
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
			
			Assert.AreEqual(1, recorder.ChangeHandlerCallCount, "Event should have been fired");
			Assert.AreEqual(ListChangedType.Reset, recorder.LastReceivedChangeType, "Event of correct type should have been fired.");
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
			
			// The list changed event is fired two times: first time for adding
			// , second time for changing the foreign key column
			publisher.Titles.Add(title);
			
			Assert.AreEqual(2, recorder.ChangeHandlerCallCount, "Event should have been fired two times");
			Assert.AreEqual(ListChangedType.ItemAdded, recorder.ReceivedChangeTypes[0], "Event of correct type should have been fired.");
			Assert.AreEqual(publisher.Titles.IndexOf(title), recorder.ReceivedNewIndexes[0], "Wrong NewIndex reported.");
		}


		[Test]
		public void SendsNotifcationOnRemove()
		{
			Publisher		publisher;
			EventRecorder	recorder;

			publisher = new PublisherFactory(context).FindObject("0877");
			publisher.Titles.Touch(); // this causes an reset event, we don't want to record
			
			
			recorder = new EventRecorder(publisher.Titles);
			
			int removedTitleIndex = publisher.Titles.IndexOf(publisher.Titles[2]);
			// The list change event fires just one time for removing, because first the object
			// is removed from the list and afterwards the objectContext notifies of the change
			// of the foreign key (the ObjectRelationBase doesn't care about this last notification 
			// anymore, because the object isn't contained in the list anymore)
			publisher.Titles.Remove(publisher.Titles[2]);
			
			Assert.AreEqual(1, recorder.ChangeHandlerCallCount, "Event should have been fired one time.");
			Assert.AreEqual(ListChangedType.ItemDeleted, recorder.LastReceivedChangeType, "Event of correct type should have been fired.");
			Assert.AreEqual(removedTitleIndex, recorder.ReceivedNewIndexes[0], "Wrong NewIndex reported.");
		}


		[Test]
		public void SendsNotifcationOnObjectChange()
		{
			Publisher		publisher;
			EventRecorder	recorder;

			publisher = new PublisherFactory(context).FindObject("0877");
			// Touch list first, otherwise we'll see a reset event.
			publisher.Titles.Touch();
			// Now the real operation...
			recorder = new EventRecorder(publisher.Titles);
			
			publisher.Titles[2].Advance += 1;
			
			Assert.AreEqual(1, recorder.ChangeHandlerCallCount, "Event should have been fired");
			Assert.AreEqual(ListChangedType.ItemChanged, recorder.LastReceivedChangeType, "Event of correct type should have been fired.");
			Assert.AreEqual(2, recorder.ReceivedNewIndexes[0], "Wrong NewIndex reported.");
		}


		#region Helper class: Event Recorder

		private class EventRecorder
		{
			public ListChangedType LastReceivedChangeType
			{
				get	{ return (ListChangedType)ReceivedChangeTypes[ReceivedChangeTypes.Count-1]; }
			}
			public int ChangeHandlerCallCount = 0;
			public ArrayList ReceivedChangeTypes = new ArrayList();
			public ArrayList ReceivedNewIndexes = new ArrayList();
			

			public EventRecorder(ObjectRelationBase collection)
			{
				IBindingList bindingList = collection;
				Assert.IsTrue(bindingList.SupportsChangeNotification, "Collection must support notification");
				collection.ListChanged += new ListChangedEventHandler(Titles_ListChanged);
			}
		
			private void Titles_ListChanged(object sender, ListChangedEventArgs e)
			{
				ChangeHandlerCallCount += 1;
				ReceivedChangeTypes.Add(e.ListChangedType);
				ReceivedNewIndexes.Add(e.NewIndex);
			}

		}

		#endregion

	}
}
