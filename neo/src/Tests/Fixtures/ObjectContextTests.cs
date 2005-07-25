using System;
using System.Collections;
using System.Data;
using System.Reflection;
using Neo.Core;
using Neo.Core.Util;
using NMock;
using NMock.Constraints;
using NUnit.Framework;
using Pubs4.Model;


namespace Neo.Tests.Fixtures
{
	[TestFixture]
	public class ObjectContextTests : TestBase
	{
		private ObjectContext	context;

		[SetUp]
		public void CreateContext()
		{
			SetupLog4Net();
			
			context = new ObjectContext();
			context.MergeData(GetTestDataSet());
			context.AcceptChanges();
		}


		[Test]
		public void ObjectRegistration()
		{
			Title	title;
			bool	foundSampleTitle;

			Assert.IsTrue(context.GetAllRegisteredObjects().Count > 0, "No objects registered.");

			foundSampleTitle = false;
			foreach(object o in context.GetAllRegisteredObjects())
			{
				if(((title = o as Title) != null) && (title.TitleId.Equals("TC7777")))
				{
					foundSampleTitle = true;
					break;
				}
			}
			Assert.IsTrue(foundSampleTitle, "Could not find title with id TC7777.");
		}


		[Test]
		public void ObjectIdMapping()
		{
			Title		titleObject;

			titleObject = (Title)context.GetObjectFromTable("titles", new string[] { "TC7777" });
			Assert.IsNotNull(titleObject, "Failed to find title with id TC7777");
		}


		[Test]
		public void SequencePrimaryKeys()
		{
			Job	newJob;

			newJob = new JobFactory(context).CreateObject();
			Assert.AreEqual(-1, newJob.JobId, "Wrong (sequence) primary key for new object.");
			newJob = new JobFactory(context).CreateObject();
			Assert.AreEqual(-2, newJob.JobId, "Wrong (sequence) primary key for new object.");
		}
		

		[Test]
		public void PkUpdates()
		{
			Title			titleObject;
			PkChangeTable	table;

			titleObject = (Title)context.GetObjectFromTable("titles", new string[] { "TC7777" });

			table = new PkChangeTable("titles");
			table.AddPkChange("TC7777", "TC9999");
			context.UpdatePrimaryKeys(table);

			Assert.AreEqual("TC9999", titleObject.TitleId, "Pk update failed.");
		}


		[Test]
		public void SetupAfterCreateIsCalled()
		{
			/* If this test fails, check that the Title class really implementes the required
			 * method. (The class file might have been overwritten accidentally.) The class
			 * should contain the following method:
			 * 
			 *  [LifecycleCreate]
			 *	protected void SetupAfterCreate()
			 *	{
			 *		Type = "UNDECIDED";
			 *	}
			 */

			Title	newTitle;

			// test a new object
			newTitle = new TitleFactory(context).CreateObject("XX9999");
			Assert.AreEqual("UNDECIDED", newTitle.Type, "Wrong default value for type.");

			// test with an object that is based on a recycled row
			newTitle.Type = "Whatever";
			Assert.AreEqual("Whatever", newTitle.Type, "Wrong value for type.");
			newTitle.Delete();
			newTitle = new TitleFactory(context).CreateObject("XX9999");
			Assert.AreEqual("UNDECIDED", newTitle.Type, "Wrong default value for type after recreate.");
		}


		[Test]
		public void ContextTransfersForOneObject()
		{
			ObjectContext childContext;
			Title		  titleInParent, titleInChild;
			
			childContext = new ObjectContext(context);
			titleInParent = new TitleFactory(context).FindUnique("TitleId = 'TC7777'");
			titleInChild = (Title)childContext.GetLocalObject(titleInParent);
			Assert.IsNotNull(titleInChild, "titleInChild not there.");
			Assert.IsTrue(childContext == titleInChild.Context, "titleInChild not in child context.");
			Assert.AreEqual(titleInParent.TitleId, titleInChild.TitleId, "Referes to different title.");
		}

		
		[Test]
		[ExpectedException(typeof(ObjectNotFoundException))]
		public void ContextTransfersForOneObjectNotFound()
		{
			ObjectContext childContext;
			Title		  titleInParent;
			
			titleInParent = new TitleFactory(context).FindUnique("TitleId = 'TC7777'");
			// note we are using a new context as parent context
			childContext = new ObjectContext(new ObjectContext()); 
			childContext.GetLocalObject(titleInParent);
		}

	
		[Test]
		public void ContextTransfersForObjectLists()
		{
			ObjectContext childContext;
			TitleList	  titlesInParent, titlesInChild;
			IList		  returnedList;
			
			childContext = new ObjectContext(context);
			titlesInParent = new TitleFactory(context).FindAllObjects();
			returnedList = childContext.GetLocalObjects(titlesInParent);
			Assert.AreEqual(titlesInParent.GetType(), returnedList.GetType(), "Wrong type of returned list.");
			titlesInChild = returnedList as TitleList;
			Assert.AreEqual(titlesInParent.Count, titlesInChild.Count, "Not all titles there.");
			for(int i = 0; i < titlesInParent.Count; i++)
			{
				Assert.IsTrue(childContext == titlesInChild[i].Context, "titleInChild not in child context.");
				Assert.IsTrue(titlesInParent[i].IsSame(titlesInChild[i]), "Referes to different title.");
			}
		}


		[Test]
		public void NestedContextGetObjectFromTable()
		{
			ObjectContext	childContext;
			Title			titleInParent, titleInChild;

			titleInParent = (Title)context.GetObjectFromTable("titles", new string[] { "TC7777" });
			childContext = new ObjectContext(context);
			titleInChild = (Title)childContext.GetObjectFromTable("titles", new string[] { "TC7777" });

			Assert.IsTrue(titleInParent.IsSame(titleInChild), "Not the same.");
		}

			
		[Test]
		public void NestedContextChangesSaves()
		{
			ObjectContext childContext;
			TitleList	  titleListInChild, titleListInParent;
			
			childContext = new ObjectContext(context);

			titleListInChild = new TitleFactory(childContext).FindAllObjects();
			foreach(Title t in titleListInChild)
				t.TheTitle += " changed";
			
			childContext.SaveChanges();
			// what we are really testing is
			//   context.SaveChangesInObjectContext(childContext);
	
			titleListInParent = new TitleFactory(context).FindAllObjects();
			foreach(Title t in titleListInParent)
				Assert.IsTrue(t.TheTitle.IndexOf("changed") > 0, "Changes not reflected in parent context");
		}

		[Test]
		public void NestedContextDeleteSaves()
		{
			ObjectContext childContext;
			TitleList	  titleListInChild, titleListInParent;
			
			childContext = new ObjectContext(context);

			titleListInChild = new TitleFactory(childContext).FindAllObjects();
			foreach(Title t in titleListInChild)
				t.Delete();
			
			childContext.SaveChanges();
			// what we are really testing is
			//   context.SaveChangesInObjectContext(childContext);
	
			titleListInParent = new TitleFactory(context).FindAllObjects();
			Assert.AreEqual(0 , titleListInParent.Count, "Should have no titles");
		}


		[Test]
		public void NestedContextSavesWithoutChange()
		{
			ObjectContext childContext;
			
			childContext = new ObjectContext(context);
			childContext.SaveChanges();
		}

		[Test]
		public void NestedContextUpdateRelations()
		{
			Title titleInParent = new TitleFactory(context).CreateObject("A new Title!");
			Publisher publisher1 = new PublisherFactory(context).FindAllObjects()[0];
			titleInParent.Publisher = publisher1;

			Assert.AreEqual(6, publisher1.Titles.Count);
			
			ObjectContext childContext = new ObjectContext(context);

			Publisher publisher2InChild = new PublisherFactory(childContext).CreateObject("publisher2");
			
			Title titleInChild = (Title)childContext.GetLocalObject(titleInParent);
			titleInChild.Publisher = publisher2InChild;

			Publisher publisher1InChildContext = (Publisher)childContext.GetLocalObject(publisher1);
			Assert.AreEqual(5, publisher1InChildContext.Titles.Count);
			Assert.AreEqual(1, publisher2InChild.Titles.Count);
			
			childContext.SaveChanges();

			Publisher publisher2InParent = (Publisher)context.GetLocalObject(publisher2InChild);

			Assert.AreEqual(5, publisher1.Titles.Count);
			Assert.AreEqual(1, publisher2InParent.Titles.Count);
		}


		[Test]
		public void NestedContextsDemandLoadFromTheDataStoreBelongingToTheirParentContexts()
		{
			// the context intialised in setup is used as if it were the db data store.
			// create a resource with a title in the "database"
			Publisher publisher = new PublisherFactory(context).CreateObject("1010");
			Title title = new TitleFactory(context).CreateObject("XX9999");
			title.TheTitle = "Nested Contexts Explained";
			title.Publisher = publisher;			

			ObjectContext normalContext = new ObjectContext(context, false);
			// demand load publisher in this context, only the publisher not the title
			normalContext.GetLocalObject(publisher);

			ObjectContext childContext = new ObjectContext(normalContext, false);
			Publisher publisherInChildContext = (Publisher)childContext.GetLocalObject(publisher);
			Assert.AreEqual(1, publisherInChildContext.Titles.Count);
			Assert.AreEqual("Nested Contexts Explained", publisherInChildContext.Titles[0].TheTitle);
		}


		[Test]
		public void MergeUpdatesRelationsForNewObjects()
		{
			ObjectContext	childContext;
			Publisher		publisherInParent, publisherInChild;
			Publisher		otherPublisherInParent, otherPublisherInChild;
			Title			newTitleInChild, newTitleInParent;
			int				countBefore;
			
			publisherInParent = new PublisherFactory(context).FindObject("0877");
			otherPublisherInParent = new PublisherFactory(context).FindObject("0736");
			countBefore = publisherInParent.Titles.Count;

			childContext = new ObjectContext(context);
			newTitleInChild = new TitleFactory(childContext).CreateObject("XX9999");
			otherPublisherInChild = (Publisher)childContext.GetLocalObject(otherPublisherInParent);
			newTitleInChild.Publisher = otherPublisherInChild;
			Assert.IsTrue(otherPublisherInChild.Titles.Contains(newTitleInChild), "Other publisher in child doesn't know new title.");
			
			publisherInChild = (Publisher)childContext.GetLocalObject(publisherInParent);
			newTitleInChild.Publisher = publisherInChild;
			Assert.IsTrue(publisherInChild.Titles.Contains(newTitleInChild), "Publisher in child doesn't know new title.");

			childContext.SaveChanges();
			Assert.IsTrue(publisherInParent.Titles.Count == countBefore + 1, "Publisher in parent doesn't have a new title.");
			newTitleInParent = (Title)context.GetLocalObject(newTitleInChild);
			Assert.IsTrue(publisherInParent.Titles.Contains(newTitleInParent), "Publisher in child doesn't know new title.");
			Assert.IsTrue(otherPublisherInParent.Titles.Contains(newTitleInParent) == false, "Other publisher in child does know new title.");
		}

	
		[Test]
		public void MergeUpdatesRelationsForChangedObjects()
		{
			ObjectContext	childContext;
			Publisher		publisherInParent, publisherInChild;
			Title			titleInChild, titleInParent;
			int				countBefore;
			
			publisherInParent = new PublisherFactory(context).FindObject("0877");
			countBefore = publisherInParent.Titles.Count;

			childContext = new ObjectContext(context);
			publisherInChild = (Publisher)childContext.GetLocalObject(publisherInParent);
			titleInChild = new TitleFactory(childContext).FindObject("PS7777");
			titleInChild.Publisher = publisherInChild;
			Assert.IsTrue(publisherInChild.Titles.Contains(titleInChild), "Publisher in child doesn't know changed title.");

			childContext.SaveChanges();
			Assert.IsTrue(publisherInParent.Titles.Count == countBefore + 1, "Publisher in parent doesn't have a new title.");
			titleInParent = (Title)context.GetLocalObject(titleInChild);
			Assert.IsTrue(publisherInParent.Titles.Contains(titleInParent), "Publisher in child doesn't know changed title.");
		}

	
		[Test]
		public void MergeUpdatesRelationsForDeletedObjects()
		{
			ObjectContext	childContext;
			Publisher		publisherInParent, publisherInChild;
			Title			titleInChild;
			int				countBefore;
			
			publisherInParent = new PublisherFactory(context).FindObject("0877");
			countBefore = publisherInParent.Titles.Count;

			childContext = new ObjectContext(context);
			publisherInChild = (Publisher)childContext.GetLocalObject(publisherInParent);
			titleInChild = publisherInChild.Titles[0];
			Assert.AreEqual(childContext, titleInChild.Context, "");
			titleInChild.Delete();
			Assert.IsTrue(publisherInChild.Titles.Count == countBefore - 1, "Publisher in child still has title.");

			childContext.SaveChanges();
			Assert.IsTrue(publisherInParent.Titles.Count == countBefore - 1, "Publisher in parent still has title.");
		}


		[Test]
		[ExpectedException(typeof(ObjectNotFoundException))]
		public void GetObjectFromTableThrowsWhenNoRowsReturnedByDataStore()
		{
			DummyDataStore dataStore = new DummyDataStore();
			ObjectContext context = new ObjectContext(dataStore);
			IEntityMap map = context.EntityMapFactory.GetMap(typeof(Publisher));
			dataStore.DataTable = new DataTable(map.TableName);

			context.GetObjectFromTable(map.TableName, new object[] { "dummy" });
		}


		#region Helper class: dummy implementation of IDataStore

		class DummyDataStore: IDataStore
		{
			public DataTable DataTable;

			#region IDataStore Members

			public ICollection SaveChangesInObjectContext(ObjectContext context)
			{
				return null;
			}

			public DataSet FetchRows(IFetchSpecification fetchSpec)
			{
				DataSet ds = new DataSet();
				ds.Tables.Add(DataTable);
				return ds;
			}

			#endregion
		}

		#endregion


		[Test]
		public void DeleteOfExistingObjectsUpdatesObjectTable()
		{
			TitleList	titleList;
			int			countBefore;
			
			titleList = new TitleFactory(context).FindAllObjects();
			countBefore = titleList.Count;

			titleList.FindUnique("TitleId = 'PS7777'").Delete();

			titleList = new TitleFactory(context).FindAllObjects();
			Assert.AreEqual(countBefore - 1, titleList.Count, "Wrong number after delete.");
			Assert.IsTrue(titleList.Find("TitleId = 'PS7777'").Count == 0, "Object should be gone.");
		}


		[Test]
		public void RejectChangesOfExistingObjectsIsNoopForObjectTable()
		{
			TitleList	 titleList;
			Title		 title;
			int			 countBefore;
			
			titleList = new TitleFactory(context).FindAllObjects();
			countBefore = titleList.Count;

			title = titleList.FindUnique("TitleId = 'PS7777'");
			title.TheTitle = "something else";
			title.RejectChanges();

			titleList = new TitleFactory(context).FindAllObjects();
			Assert.AreEqual(countBefore, titleList.Count, "Wrong number after delete.");
			Assert.IsTrue(titleList.Find("TitleId = 'PS7777'").Count == 1, "Object should be there still.");
		}


		[Test]
		public void DeleteOfNewObjectsUpdatesObjectTable()
		{
			TitleFactory factory;
			TitleList	 titleList;
			int			 countBefore;
			
			factory = new TitleFactory(context);
			factory.CreateObject("XX9999");
			titleList = factory.FindAllObjects();
			countBefore = titleList.Count;

			titleList.FindUnique("TitleId = 'XX9999'").Delete();

			titleList = new TitleFactory(context).FindAllObjects();
			Assert.AreEqual(countBefore - 1, titleList.Count, "Wrong number after delete.");
			Assert.IsTrue(titleList.Find("TitleId = 'XX9999'").Count == 0, "Still found it.");
		}


		[Test]
		public void RejectChangesOfNewObjectsUpdatesObjectTable()
		{
			TitleFactory factory;
			TitleList	 titleList;
			int			 countBefore;
			
			factory = new TitleFactory(context);
			factory.CreateObject("XX9999");
			titleList = factory.FindAllObjects();
			countBefore = titleList.Count;

			titleList.FindUnique("TitleId = 'XX9999'").RejectChanges();

			titleList = new TitleFactory(context).FindAllObjects();
			Assert.AreEqual(countBefore - 1, titleList.Count, "Wrong number after delete.");
			Assert.IsTrue(titleList.Find("TitleId = 'XX9999'").Count == 0, "Still found it.");
		}


		[Test]
		public void RejectingDeleteOfFetchedObjectReinsertsItIntoObjectTable()
		{
			JobFactory  factory;
			Job			job;
			int			countBefore;

			factory = new JobFactory(context);
			job = factory.FindAllObjects()[0];
			countBefore = factory.FindAllObjects().Count;
			job.Delete();
			job.RejectChanges();
			Assert.AreEqual(countBefore, factory.FindAllObjects().Count);
		} 


		[Test]
		public void AddedObjectDeletionsPropagateToParentContext()
		{
			Title			newTitleInParent, newTitleInChild;
			ObjectContext	childContext;

			newTitleInParent = new TitleFactory(context).CreateObject("XX9999");
			childContext = new ObjectContext(context);
			newTitleInChild = (Title)childContext.GetLocalObject(newTitleInParent);
			
			newTitleInChild.Delete();
			childContext.SaveChanges();

			Assert.AreEqual(DataRowState.Detached, newTitleInParent.Row.RowState, "Wrong row state.");
		}


		[Test]
		public void ShouldKeepObjectsAndStateWhenSerializing()
		{
			TitleFactory	factory;
			ObjectContext	newContext;

			factory = new TitleFactory(context);
			factory.FindObject("TC7777").Advance -= 1;
			factory.FindObject("MC3021").Delete();
			factory.CreateObject("XX9999");

			newContext = (ObjectContext)RunThroughSerialization(context);

			Assert.AreEqual(context.GetAllRegisteredObjects().Count, newContext.GetAllRegisteredObjects().Count, "Number of objects should be the same.");
			factory = new TitleFactory(newContext);
			Assert.IsTrue(factory.FindObject("TC7777").HasChanges(), "Title TC7777 should have changes.");
			Assert.IsTrue(factory.FindObject("MC3021") == null, "Title MC3021 should be deleted.");
			Assert.IsTrue(factory.FindObject("XX9999").Row.RowState == DataRowState.Added, "Title XX9999 should be new.");
		}


		[Test]
		public void ShouldKeepCustomerEmapFactoryWhenSerializing()
		{
			context = new ObjectContext();
			context.EntityMapFactory = new CustomEntityMapFactory();

			context = (ObjectContext)RunThroughSerialization(context);

			Assert.AreEqual(typeof(CustomEntityMapFactory), context.EntityMapFactory.GetType(), "Should recreate custom emap factory");
		}

		[Test]
		public void SendsNotificationOnCreateEntityObject()
		{
			JobFactory	factory;
			
			EventRecorder recorder = new EventRecorder(this.context);
			factory = new JobFactory(context);
			Job job = factory.CreateObject();

			Assert.AreEqual(1, recorder.ChangeHandlerCallCount);
			Assert.AreSame(job, recorder.ReceivedEntityObject);
			Assert.AreEqual(EntityObjectAction.Add, recorder.ReceivedAction);
		}

		[Test]
		public void SendsNotificationOnChangeAttributeOfEntityObject()
		{
			JobFactory	factory;
			factory = new JobFactory(context);
			Job job = factory.CreateObject();
			
			EventRecorder recorder = new EventRecorder(this.context);
			job.Description = "A good job.";

			Assert.AreEqual(1, recorder.ChangeHandlerCallCount);
			Assert.AreSame(job, recorder.ReceivedEntityObject);
			Assert.AreEqual(EntityObjectAction.Change, recorder.ReceivedAction);
		}

		[Test]
		public void SendsNotificationOnDeleteEntityObject()
		{
			JobFactory	factory;
			factory = new JobFactory(context);
			Job job = factory.CreateObject();
			
			context.AcceptChanges();
			EventRecorder recorder = new EventRecorder(this.context);
			
			job.Delete();
			

			Assert.AreEqual(1, recorder.ChangeHandlerCallCount);
			Assert.AreSame(job, recorder.ReceivedEntityObject);
			Assert.AreEqual(EntityObjectAction.Delete, recorder.ReceivedAction);
		}

		#region Helper class: Event Recorder

		private class EventRecorder
		{
			public EntityObjectAction ReceivedAction;
			public int ChangeHandlerCallCount = 0;
			public IEntityObject ReceivedEntityObject;

			public EventRecorder(ObjectContext context)
			{
				context.EntityObjectChanged += new EntityObjectChangedHandler(context_EntityObjectChanged);
			}
		
			private void context_EntityObjectChanged(object sender, EntityObjectChangeEventArgs e)
			{
				ChangeHandlerCallCount += 1;
				ReceivedEntityObject = e.EntityObject;
				ReceivedAction = e.Action;
			}
		}

		#endregion

		#region Helper class

		public class CustomEntityMapFactory : DefaultEntityMapFactory
		{
		}


		#endregion

		[Test]
		public void DeletedRowsAreAddedAsDeletedObjects()
		{
			Title titleToDelete = new TitleFactory(context).FindAllObjects()[0];
			titleToDelete.Delete();
			DataSet dataSetWithDeletedRows = context.DataSet.GetChanges(DataRowState.Deleted);

			int expectedNumberOfDeletedObjects = CountRows(dataSetWithDeletedRows);

			ObjectContext newContext = new ObjectContext(dataSetWithDeletedRows);

			Assert.AreEqual(expectedNumberOfDeletedObjects, newContext.GetDeletedObjects().Count);
		}

		#region Helper method

		private int CountRows(DataSet dataSet)
		{
			int count = 0;

			foreach(DataTable table in dataSet.Tables)
			{
			 	count += table.Rows.Count;
			}

			return count;
		}

		#endregion

		[Test]
		public void DoesNotRefetchFromStoreAfterFullTableFetch()
		{
			IEntityMap titleMap = DefaultEntityMapFactory.SharedInstance.GetMap(typeof(Title));
			
			IMock storeMock = new DynamicMock(typeof(IDataStore));
			DataTable resultTable = new DataTable(titleMap.TableName);
			context = new ObjectContext((IDataStore)storeMock.MockInstance);

			FetchSpecification fetchSpec = new FetchSpecification(titleMap);

			storeMock.ExpectAndReturn("FetchRows", resultTable, new IsAnything());
			context.GetObjects(fetchSpec);

			// We are fetching again, the mock doesn't expect to be called.
			context.GetObjects(fetchSpec);

			storeMock.Verify();
		}

		[Test]
		public void AddsEntityObjectsToDataStoreSaveException()
		{		
			DynamicMock storeMock = new DynamicMock(typeof(IDataStore));
			
			context = new ObjectContext((IDataStore)storeMock.MockInstance);
			context.MergeData(GetTestDataSet());

			ObjectId oid = new ObjectId("publishers", new object[] { "0877" } );
			DataStoreSaveException.ErrorInfo[] info = { new DataStoreSaveException.ErrorInfo(oid, "foo") };
			object[] args = new object[]{ "Message here", info };
			BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
			Exception thrownException = (Exception)Activator.CreateInstance(typeof(DataStoreSaveException), flags, null, args, null);
			storeMock.ExpectAndThrow("SaveChangesInObjectContext", thrownException, context);

			DataStoreSaveException caughtException = null;
			try
			{
				context.SaveChanges();
				Assert.Fail("Should have thrown exception.");
			}
			catch(DataStoreSaveException e)
			{
				caughtException = e;
			}

			Publisher obj = (Publisher)caughtException.Errors[0].EntityObject;
		 	Assert.AreEqual("0877", obj.PubId);

		}

	}
}
