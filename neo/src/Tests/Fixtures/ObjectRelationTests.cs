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
		    Assertion.AssertEquals("Count differs.", publisher.Titles.Count, titleList.Count);
			for(int i = 0; i < titleList.Count; i++)
				Assertion.AssertEquals("Objects differ.", publisher.Titles[i], titleList[i]);
			Assertion.Assert("List not read-only.", titleList.IsReadOnly);
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

			Assertion.AssertEquals("Should have kept list same", countBefore, title.TitleAuthors.Count);
		}


		[Test]
		public void SendsNotifcationOnFirstAccess()
		{
			Publisher		publisher;
			EventRecorder	recorder;

			publisher = new PublisherFactory(context).CreateObject("4711");
			recorder = new EventRecorder(publisher.Titles);
	
			publisher.Titles.Touch();
			
			Assertion.Assert("Event should have been fired", recorder.changeHandlerWasCalled);
			Assertion.AssertEquals("Event of correct type should have been fired.", ListChangedType.Reset, recorder.receivedChangeType);
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
			
			Assertion.Assert("Event should have been fired", recorder.changeHandlerWasCalled);
			Assertion.AssertEquals("Event of correct type should have been fired.", ListChangedType.ItemAdded, recorder.receivedChangeType);
		}


		[Test]
		public void SendsNotifcationOnRemove()
		{
			Publisher		publisher;
			EventRecorder	recorder;

			publisher = new PublisherFactory(context).FindObject("0877");
			recorder = new EventRecorder(publisher.Titles);
			
			publisher.Titles.Remove(publisher.Titles[0]);
			
			Assertion.Assert("Event should have been fired", recorder.changeHandlerWasCalled);
			Assertion.AssertEquals("Event of correct type should have been fired.", ListChangedType.ItemDeleted, recorder.receivedChangeType);
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
			
			Assertion.Assert("Event should have been fired", recorder.changeHandlerWasCalled);
			Assertion.AssertEquals("Event of correct type should have been fired.", ListChangedType.ItemChanged, recorder.receivedChangeType);
		}


		#region Helper class: Event Recorder

		private class EventRecorder
		{
			public ListChangedType receivedChangeType;
			public bool changeHandlerWasCalled = false;

			public EventRecorder(ObjectRelationBase collection)
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
