using System;
using System.Collections;
using System.Data;
using NUnit.Framework;
using Neo.Core;
using Pubs4.Model;


namespace Neo.Tests
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

			Assertion.Assert("No objects registered.", context.GetAllRegisteredObjects().Count > 0);

			foundSampleTitle = false;
			foreach(object o in context.GetAllRegisteredObjects())
			{
				if(((title = o as Title) != null) && (title.TitleId.Equals("TC7777")))
				{
					foundSampleTitle = true;
					break;
				}
			}
			Assertion.Assert("Could not find title with id TC7777.", foundSampleTitle);
		}


		[Test]
		public void ObjectIdMapping()
		{
			Title		titleObject;

			titleObject = (Title)context.GetObjectFromTable("titles", new string[] { "TC7777" });
			Assertion.AssertNotNull("Failed to find title with id TC7777", titleObject);
		}


		[Test]
		public void SequencePrimaryKeys()
		{
			Job	newJob;

			newJob = new JobFactory(context).CreateObject();
			Assertion.AssertEquals("Wrong (sequence) primary key for new object.", -1, newJob.JobId);
			newJob = new JobFactory(context).CreateObject();
			Assertion.AssertEquals("Wrong (sequence) primary key for new object.", -2, newJob.JobId);
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

			Assertion.AssertEquals("Pk update failed.", "TC9999", titleObject.TitleId);
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
			Assertion.AssertEquals("Wrong default value for type.", "UNDECIDED", newTitle.Type);

			// test with an object that is based on a recycled row
			newTitle.Type = "Whatever";
			Assertion.AssertEquals("Wrong value for type.", "Whatever", newTitle.Type);
			newTitle.Delete();
			newTitle = new TitleFactory(context).CreateObject("XX9999");
			Assertion.AssertEquals("Wrong default value for type after recreate.", "UNDECIDED", newTitle.Type);
		}


		[Test]
		public void ContextTransfersForOneObject()
		{
			ObjectContext childContext;
			Title		  titleInParent, titleInChild;
			
			childContext = new ObjectContext(context);
			titleInParent = new TitleFactory(context).FindUnique("TitleId = 'TC7777'");
			titleInChild = (Title)childContext.GetLocalObject(titleInParent);
			Assertion.AssertNotNull("titleInChild not there.", titleInChild);
			Assertion.Assert("titleInChild not in child context.", childContext == titleInChild.Context);
			Assertion.AssertEquals("Referes to different title.", titleInParent.TitleId, titleInChild.TitleId);
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
			Assertion.AssertEquals("Wrong type of returned list.", titlesInParent.GetType(), returnedList.GetType());
			titlesInChild = returnedList as TitleList;
			Assertion.AssertEquals("Not all titles there.", titlesInParent.Count, titlesInChild.Count);
			for(int i = 0; i < titlesInParent.Count; i++)
			{
				Assertion.Assert("titleInChild not in child context.", childContext == titlesInChild[i].Context);
				Assertion.Assert("Referes to different title.", titlesInParent[i].IsSame(titlesInChild[i]));
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

			Assertion.Assert("Not the same.", titleInParent.IsSame(titleInChild));
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
				Assertion.Assert("Changes not reflected in parent context", t.TheTitle.IndexOf("changed") > 0);
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
			Assertion.AssertEquals("Should have no titles", 0 , titleListInParent.Count);
		}


		[Test]
		public void NestedContextSavesWithoutChange()
		{
			ObjectContext childContext;
			
			childContext = new ObjectContext(context);
			childContext.SaveChanges();
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
			Assertion.Assert("Other publisher in child doesn't know new title.", otherPublisherInChild.Titles.Contains(newTitleInChild));
			
			publisherInChild = (Publisher)childContext.GetLocalObject(publisherInParent);
			newTitleInChild.Publisher = publisherInChild;
			Assertion.Assert("Publisher in child doesn't know new title.", publisherInChild.Titles.Contains(newTitleInChild));

			childContext.SaveChanges();
			Assertion.Assert("Publisher in parent doesn't have a new title.", publisherInParent.Titles.Count == countBefore + 1);
			newTitleInParent = (Title)context.GetLocalObject(newTitleInChild);
			Assertion.Assert("Publisher in child doesn't know new title.", publisherInParent.Titles.Contains(newTitleInParent));
			Assertion.Assert("Other publisher in child does know new title.", otherPublisherInParent.Titles.Contains(newTitleInParent) == false);
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
			Assertion.Assert("Publisher in child doesn't know changed title.", publisherInChild.Titles.Contains(titleInChild));

			childContext.SaveChanges();
			Assertion.Assert("Publisher in parent doesn't have a new title.", publisherInParent.Titles.Count == countBefore + 1);
			titleInParent = (Title)context.GetLocalObject(titleInChild);
			Assertion.Assert("Publisher in child doesn't know changed title.", publisherInParent.Titles.Contains(titleInParent));
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
			Assertion.AssertEquals("", childContext, titleInChild.Context);
			titleInChild.Delete();
			Assertion.Assert("Publisher in child still has title.", publisherInChild.Titles.Count == countBefore - 1);

			childContext.SaveChanges();
			Assertion.Assert("Publisher in parent still has title.", publisherInParent.Titles.Count == countBefore - 1);
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

			public DataTable FetchRows(IFetchSpecification fetchSpec)
			{
				return DataTable;
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
			Assertion.AssertEquals("Wrong number after delete.", countBefore - 1, titleList.Count);
			Assertion.Assert("Object should be gone.", titleList.Find("TitleId = 'PS7777'").Count == 0);
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
			Assertion.AssertEquals("Wrong number after delete.", countBefore, titleList.Count);
			Assertion.Assert("Object should be there still.", titleList.Find("TitleId = 'PS7777'").Count == 1);
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
			Assertion.AssertEquals("Wrong number after delete.", countBefore - 1, titleList.Count);
			Assertion.Assert("Still found it.", titleList.Find("TitleId = 'XX9999'").Count == 0);
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
			Assertion.AssertEquals("Wrong number after delete.", countBefore - 1, titleList.Count);
			Assertion.Assert("Still found it.", titleList.Find("TitleId = 'XX9999'").Count == 0);
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

			Assertion.AssertEquals("Wrong row state.", DataRowState.Detached, newTitleInParent.Row.RowState);
		}

	}
}
