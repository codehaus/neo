using System;
using System.Collections;
using System.Data;
using Neo.Core;
using Neo.Core.Util;
using Neo.Database;
using Neo.OracleClient;
using NMock;
using NUnit.Framework;
using Pubs4.Model;


namespace Neo.Tests.Fixtures
{
	// Of course, DbDataStore is abstract. This fixture will test whichever 
	// datastore is specified in test.config (and uses an in-memory Firebird 
	// instance by default).

	[TestFixture]
	public class DbDataStoreTests : TestBase
	{
		IEntityMapFactory emapFactory;
		DbDataStore		  store;
		DataSet			  dataset, jobOnlyDataSet;
		DataTable		  titleTable, jobTable;


		[SetUp]
		public void SetupStoreAndDataset()
		{
			SetupLog4Net();

			emapFactory = DefaultEntityMapFactory.SharedInstance;
			store = (DbDataStore)GetDataStore();

			dataset = new DataSet();
			dataset.EnforceConstraints = false;
			
			jobOnlyDataSet = new DataSet();
			jobOnlyDataSet.EnforceConstraints = false;

			emapFactory.GetMap(typeof(Title)).UpdateSchemaInDataSet(dataset, SchemaUpdate.Basic | SchemaUpdate.Constraints);
			titleTable = dataset.Tables["titles"];
			emapFactory.GetMap(typeof(Job)).UpdateSchemaInDataSet(dataset, SchemaUpdate.Basic | SchemaUpdate.Constraints);
			emapFactory.GetMap(typeof(Job)).UpdateSchemaInDataSet(jobOnlyDataSet, SchemaUpdate.Basic | SchemaUpdate.Constraints);
			jobTable = dataset.Tables["jobs"];
		}


		[Test]
		public void CheckSetup()
		{
			loadTitle("TC7777");
			Assert.AreEqual("TC7777", titleTable.Rows[0]["title_id"], "Wrong title.");
		}


		[Test]
		public void FetchAll()
		{
			DataTable fetchedTable = store.FetchRows(new FetchSpecification(emapFactory.GetMap(typeof(Title)))).Tables["titles"];
			Assert.IsTrue( fetchedTable.Rows.Count > 15, "There should be more than 15 titles.");
		}


		[Test]
		public void SimpleUpdateWithCommit()
		{
			DataTable	fetchedTable;
			Decimal		newPrice;

			loadTitle("TC7777");
			newPrice = getDifferentPrice((Decimal)titleTable.Rows[0]["price"]);
			titleTable.Rows[0]["price"] = newPrice;
			store.BeginTransaction();
			store.ProcessUpdates(titleTable);
			store.CommitTransaction();
			Assert.AreEqual( DataRowState.Unchanged, titleTable.Rows[0].RowState, "Wrong row state.");

			fetchedTable = store.FetchRows(new FetchSpecification(emapFactory.GetMap(typeof(Title)), Qualifier.Format("TitleId", "TC7777"))).Tables["titles"];
			// Don't use AssertEquals with Decimals. (Would compare their string value which is precision dependent.)
			Assert.IsTrue(newPrice.Equals(fetchedTable.Rows[0]["price"]), "Price not updated in database.");
		}

		[Test]
		public void SimpleUpdateWithNoChangedValueDoesntCreateCommand()
		{
			Decimal	oldPrice;

			loadTitle("TC7777");
			oldPrice = (Decimal)titleTable.Rows[0]["price"];
			titleTable.Rows[0]["price"] = oldPrice;
			
			IMock implFactoryMock = new DynamicMock(typeof(IDbImplementationFactory));
			implFactoryMock.ExpectNoCall("CreateCommandBuilder", typeof(DataTable));
			DataStoreForTest store = new DataStoreForTest((IDbImplementationFactory)implFactoryMock.MockInstance);
			
			store.ProcessUpdates(titleTable);

			implFactoryMock.Verify();
		}

		[Test]
		public void SimpleUpdateWithRollback()
		{
			DataTable	fetchedTable;
			Decimal		oldPrice, newPrice;

			loadTitle("TC7777");
			oldPrice = (Decimal)titleTable.Rows[0]["price"]; 
			newPrice = getDifferentPrice(oldPrice);
			titleTable.Rows[0]["price"] = newPrice;
			store.BeginTransaction();
			store.ProcessUpdates(dataset.Tables["titles"]);
			store.RollbackTransaction();
			Assert.AreEqual(DataRowState.Modified, titleTable.Rows[0].RowState, "Wrong row state.");

			fetchedTable = store.FetchRows(new FetchSpecification(emapFactory.GetMap(typeof(Title)), Qualifier.Format("TitleId" , "TC7777"))).Tables["titles"];
			Assert.AreEqual(oldPrice, fetchedTable.Rows[0]["price"], "Price was updated in database.");
		}


		[Test]
		public void ConcurrentUpdateFailure()
		{
			DataSet		dataset2;
			DataRow		titleRow;
			Decimal		newPrice1, newPrice2;
			bool		hadErrors;

			loadTitle("TC7777");
			dataset2 = new DataSet("Other DataSet");
			emapFactory.GetMap(typeof(Title)).UpdateSchemaInDataSet(dataset2, SchemaUpdate.Basic | SchemaUpdate.Constraints);
			dataset2.Merge(titleTable, false, MissingSchemaAction.Ignore);
			
			titleRow = titleTable.Rows[0];
			newPrice1 = getDifferentPrice((Decimal)titleRow["price"]);
			titleRow["price"] = newPrice1;
			store.BeginTransaction();
			store.ProcessUpdates(dataset.Tables["titles"]);
			store.CommitTransaction();

			titleRow = dataset2.Tables["titles"].Rows[0];
			newPrice2 = getDifferentPrice((Decimal)titleRow["price"], newPrice1);
			titleRow["price"] = newPrice2;
			store.BeginTransaction();
			store.ProcessUpdates(dataset2.Tables["titles"]);
			hadErrors = dataset2.Tables["titles"].HasErrors;
			store.RollbackTransaction();
			Assert.IsTrue( hadErrors, "No error for conflicting concurrent updates.");
		}

		[Test]
		public void Delete()
		{
			DataTable	fetchedTable;

			loadTitle("MC3026"); // this doesn't have any authors and can be deleted
			dataset.Tables["titles"].Rows.Find("MC3026").Delete();
			store.BeginTransaction();
			store.ProcessDeletes(new OrderedTableCollection(dataset));
			fetchedTable = store.FetchRows(new FetchSpecification(emapFactory.GetMap(typeof(Title)), Qualifier.Format("TitleId", "MC3026"))).Tables["titles"];
			store.RollbackTransaction();
			Assert.IsTrue( fetchedTable.Rows.Count == 0, "Row not deleted from database.");
		}

		[Test]
		public void InsertIntoTableWithIdentityColumn()
		{
			DataRow		  newRow;
			int			  tempId, finalId;
			PkChangeTable changeTable;

			if(store is OracleDataStore)
				return;

			jobTable = jobOnlyDataSet.Tables["jobs"];
			newRow = jobTable.NewRow();
			newRow["job_desc"] = "Test row for automated tests";
			newRow["min_lvl"] = 100;
			newRow["max_lvl"] = 200;
			jobTable.Rows.Add(newRow);
			tempId = (Int16)newRow["job_id"];

			store.BeginTransaction();
			ArrayList changeTables = new ArrayList();
			store.ProcessInserts(new OrderedTableCollection(jobOnlyDataSet), changeTables);
			changeTable = (PkChangeTable) changeTables[0];
			finalId = (Int16)newRow["job_id"];
			store.RollbackTransaction();

			Assert.AreEqual(tempId, changeTable[0].OldValue, "Inconsistent old value in change table.");
			Assert.AreEqual(finalId, changeTable[0].NewValue, "Inconsistent new value in change table.");
		}


		[Test]
		public void InsertIntoTableWithIdentityColumnPropagatesNewKey()
		{
			DataTable	  jobRefTable;
			DataColumn	  column;
			DataRelation  relation;
			DataRow		  newJobRow, newJobRefRow;
			int			  tempId, finalId, finalForeignId;

			if(store is OracleDataStore)
				return;

			jobTable = jobOnlyDataSet.Tables["jobs"];
			// for this test we also need a completely new entity with a relation to job_id
			jobRefTable = jobOnlyDataSet.Tables.Add("jobrefs");
			column = jobRefTable.Columns.Add("job_id", typeof(Int16));
			column = jobRefTable.Columns.Add("ref_id", typeof(String));
			column.Unique = true;
			jobRefTable.PrimaryKey = new DataColumn[] { column };
			relation = jobOnlyDataSet.Relations.Add("jobs.jobrefs", jobTable.Columns["job_id"], jobRefTable.Columns["job_id"]);
			relation.ChildKeyConstraint.UpdateRule = Rule.Cascade;
			relation.ChildKeyConstraint.DeleteRule = Rule.Cascade;

			newJobRow = jobTable.NewRow();
			newJobRow["job_desc"] = "Test row for automated tests";
			newJobRow["min_lvl"] = 100;
			newJobRow["max_lvl"] = 200;
			jobTable.Rows.Add(newJobRow);
			tempId = (Int16)newJobRow["job_id"];
			newJobRefRow = jobRefTable.NewRow();
			newJobRefRow["ref_id"] = "FOO1";
			newJobRefRow["job_id"] = tempId;
			jobRefTable.Rows.Add(newJobRefRow);

			store.BeginTransaction();
			store.ProcessInserts(jobTable, new ArrayList());
			finalId = (Int16)newJobRow["job_id"];
			finalForeignId = (Int16)newJobRefRow["job_id"];
			store.RollbackTransaction();

			Assert.IsTrue(finalId.Equals(tempId) == false, "Final id should be different from temp id.");
			Assert.AreEqual(finalId, finalForeignId, "Value should be propagated to child table.");
		}


		[Test]
		public void PkChangeTableListNotPopulatedIfNoPrimaryKeyGeneration()
		{
			emapFactory.GetMap(typeof(Publisher)).UpdateSchemaInDataSet(dataset, SchemaUpdate.Basic | SchemaUpdate.Constraints);
			DataTable titleTable = dataset.Tables["titles"];
			DataTable publisherTable = dataset.Tables["publishers"];
			titleTable.ParentRelations.Add(publisherTable.Columns["pub_id"],titleTable.Columns["pub_id"]);

			DataRow titleNewRow = titleTable.NewRow();
			titleNewRow["title"] = "Title";
			titleNewRow["type"] = "Type";
			titleNewRow["pub_id"] = "PUBL";

			DataRow publisherNewRow = publisherTable.NewRow();
			publisherNewRow["pub_id"] = "PUBL";
			publisherNewRow["pub_name"] = "Publisher";
	
			ArrayList changeTables = new ArrayList();		
			store.ProcessInserts(new OrderedTableCollection(dataset), changeTables);

			Assert.AreEqual(0, changeTables.Count, "Should not have created change tables");
		}

		
		[Test]
		public void ShouldReturnRowsOrderedAccordingToFetchSpecification()
		{
			FetchSpecification	spec;
			DataTable			fetchedTable;
			DataRow				prevRow;

			spec = new FetchSpecification(emapFactory.GetMap(typeof(Title)));
			spec.SortOrderings = new PropertyComparer[] { new PropertyComparer("PublicationDate", SortDirection.Ascending),
														  new PropertyComparer("Type", SortDirection.Descending)};

			fetchedTable = store.FetchRows(spec).Tables["titles"];
			Assert.IsTrue(fetchedTable.Rows.Count > 2, "Should have more than 2 rows");

			prevRow = fetchedTable.Rows[0];
			foreach(DataRow currRow in fetchedTable.Rows)
			{
				Assert.IsTrue((DateTime)prevRow["pubdate"] <= (DateTime)currRow["pubdate"], "Should return titles in correct order.");
				if((DateTime)prevRow["pubdate"] == (DateTime)currRow["pubdate"])
					Assert.IsTrue(Comparer.Default.Compare((String)prevRow["type"], (String)currRow["type"]) >= 0, "Should return titles in correct order.");
				prevRow = currRow;
			}
		}


		[Test]
		public void UpdateTableWithIdentityColumn()
		{
			DataTable	  fetchedTable;

			if(store is OracleDataStore)
				return;
			
			fetchedTable = store.FetchRows(new FetchSpecification(emapFactory.GetMap(typeof(Job)), Qualifier.Format("JobId", 1))).Tables["jobs"];
			Assert.AreEqual(1, fetchedTable.Rows.Count, "Wrong number of rows.");
			fetchedTable.Rows[0]["max_lvl"] = 20;
			store.BeginTransaction();
			store.ProcessUpdates(fetchedTable);
			fetchedTable = store.FetchRows(new FetchSpecification(emapFactory.GetMap(typeof(Job)), Qualifier.Format("JobId", 1))).Tables["jobs"];
			store.RollbackTransaction();

			Assert.AreEqual(1, fetchedTable.Rows.Count, "Wrong number of rows.");
			Assert.AreEqual(20, fetchedTable.Rows[0]["max_lvl"], "Value not updated.");
		}


		[Test]
		public void ShouldSerialize()
		{
			// If this test fails and you use username/password authentication make sure 
			// that you have 'Persist Security Info=true' in you connection string. 
			// The first loadTitle forces the connection to open and if you do not have 
			// the persist setting the connection will "forget" the password component 
			// of the connection string, which makes it impossible to re-use the string.

			loadTitle("TC7777"); 
			store = (DbDataStore)RunThroughSerialization(store);
			loadTitle("TC7777");
		}

		[Test]
		public void ShouldSetCommandTimeout()
		{
			IMock implFactoryMock = new DynamicMock(typeof(IDbImplementationFactory));
			IMock commandMock = new DynamicMock(typeof(IDbCommand));
			int commandTimeout = 60;
			commandMock.Expect("CommandTimeout", commandTimeout);
			implFactoryMock.SetupResult("CreateCommand", (IDbCommand)commandMock.MockInstance);
			
			DataStoreForTest store = new DataStoreForTest((IDbImplementationFactory)implFactoryMock.MockInstance);
			store.CommandTimeout = commandTimeout;
			store.PublicCreateCommand();
			
			commandMock.Verify();
		}

		[Test]
		public void ShouldSetCommandTimeoutToDefaultIfNotExplicitlySet()
		{
			IMock implFactoryMock = new DynamicMock(typeof(IDbImplementationFactory));
			IMock commandMock = new DynamicMock(typeof(IDbCommand));
			commandMock.Expect("CommandTimeout", 30);
			implFactoryMock.SetupResult("CreateCommand", (IDbCommand)commandMock.MockInstance);
			
			DataStoreForTest store = new DataStoreForTest((IDbImplementationFactory)implFactoryMock.MockInstance);
			store.PublicCreateCommand();
			
			commandMock.Verify();
		}

		#region Helper class: DataStoreForTest

		private class DataStoreForTest : DbDataStore
		{
			internal DataStoreForTest(IDbImplementationFactory implFactory) : base()
			{
				base.implFactory = implFactory;
			}

			internal void PublicCreateCommand()
			{
				this.CreateCommand("Unused");
			}

			protected override object GetFinalPk(DataRow row, IDbCommandBuilder builder)
			{
				throw new NotImplementedException();
			}
		}

		#endregion

		[Test]
		public void FetchesEntitiesGivenInPropertySpan()
		{
			FetchSpecification spec = new FetchSpecification(emapFactory.GetMap(typeof(Title)), null);
			spec.Spans = new string[]{ "Publisher" };
			
			DataSet result = store.FetchRows(spec);

			DataTable titleTable = result.Tables["titles"];
			Assert.IsTrue(titleTable.Rows.Count > 0, "Should have returned title.");
			DataTable pubTable = result.Tables["publishers"];
			Assert.IsTrue(pubTable.Rows.Count > 0, "Should have returned publishers, too.");
		}


		[Test]
		public void FetchesEntitiesGivenInPropertySpanWithQualifierOnMainEntity()
		{
			FetchSpecification spec = new FetchSpecification(emapFactory.GetMap(typeof(Title)), Qualifier.Format("TitleId='TC7777'"));
			spec.Spans = new string[]{ "Publisher" };
			
			DataSet result = store.FetchRows(spec);

			DataTable titleTable = result.Tables["titles"];
			Assert.AreEqual(1, titleTable.Rows.Count, "Should have returned title.");
			DataTable pubTable = result.Tables["publishers"];
			Assert.AreEqual(1, pubTable.Rows.Count, "Should have returned publisher, too.");
		}		

		
		[Test]
		public void FetchesEntitiesGivenInPropertyPathSpan()
		{
			FetchSpecification spec = new FetchSpecification(emapFactory.GetMap(typeof(Title)), Qualifier.Format("TheTitle like 'Sushi%'"));
			spec.Spans = new string[]{ "TitleAuthors.Author" };

			DataSet result = store.FetchRows(spec);

			DataTable titleTable = result.Tables["titles"];
			Assert.AreEqual(1, titleTable.Rows.Count, "Should have returned title.");
			DataTable authorTable = result.Tables["authors"];
			Assert.AreEqual(3, authorTable.Rows.Count, "Should have returned authors, too.");
		}


		[Test]
		public void AttachesObjectIdToExceptionOnDeleteException()
		{
			ObjectContext context = new ObjectContext(store);
			PublisherFactory factory = new PublisherFactory(context);
			Publisher publisher = factory.FindFirst("PubId = '0877'");
			publisher.Delete();

			DataStoreSaveException exception = null;
			try
			{
				context.SaveChanges();		
				Assert.Fail("Should have thrown an exception.");
			}
			catch(DataStoreSaveException e)
			{
				exception = e;
			}

			Assert.IsNotNull(exception.ObjectId, "Should report id of object that caused problem.");
			Assert.AreEqual("publishers", exception.ObjectId.TableName, "Should report correct table name");
			Assert.AreEqual("0877", exception.ObjectId.PkValues[0], "Should report correct id");
		}


		[Test]
		public void AttachesObjectIdToExceptionOnInsertException()
		{
			ObjectContext context = new ObjectContext(store);
			PublisherFactory factory = new PublisherFactory(context);
			factory.CreateObject("0877");

			DataStoreSaveException exception = null;
			try
			{
				context.SaveChanges();		
				Assert.Fail("Should have thrown an exception.");
			}
			catch(DataStoreSaveException e)
			{
				exception = e;
			}

			Assert.IsNotNull(exception.ObjectId, "Should report id of object that caused problem.");
			Assert.AreEqual("publishers", exception.ObjectId.TableName, "Should report correct table name");
			Assert.AreEqual("0877", exception.ObjectId.PkValues[0], "Should report correct id");
		}


		[Test]
		public void AttachesErrorToExceptionOnDeleteProblems()
		{
			ObjectContext context = new ObjectContext(store);
			PublisherFactory factory = new PublisherFactory(context);
			
			Publisher publisher1 = factory.FindFirst("PubId = '0877'");
			
			// Make sure our original values does not match the db contents.
			publisher1.Row["state"] = "XX";  publisher1.Row.AcceptChanges();

			// Now make the modifications.
			publisher1.Row["state"] = "YY";

			DataStoreSaveException exception = null;
			try
			{
				context.SaveChanges();		
				Assert.Fail("Should have thrown an exception.");
			}
			catch(DataStoreSaveException e)
			{
				exception = e;
			}

			Assert.AreEqual(1, exception.Errors.Length, "Should report errors.");
			Assert.AreEqual("0877", exception.Errors[0].ObjectId.PkValues[0], "Should report object id.");
		}
		
		
		[Test]
		public void AttachesErrorsToExceptionOnMultipleUpdateProblems()
		{
			ObjectContext context = new ObjectContext(store);
			PublisherFactory factory = new PublisherFactory(context);
			
			Publisher publisher1 = factory.FindFirst("PubId = '0877'");
			Publisher publisher2 = factory.FindFirst("PubId = '1756'");
			
			// Make sure our original values does not match the db contents.
			publisher1.Row["state"] = "XX";  publisher1.Row.AcceptChanges();
			publisher2.Row["state"] = "XX";  publisher2.Row.AcceptChanges();

			// Now make the modifications.
			publisher1.Row["state"] = "YY";
			publisher2.Row["state"] = "YY";

			DataStoreSaveException exception = null;
			try
			{
				context.SaveChanges();		
				Assert.Fail("Should have thrown an exception.");
			}
			catch(DataStoreSaveException e)
			{
				exception = e;
			}

			Assert.AreEqual(2, exception.Errors.Length, "Should report errors.");
		}


		[Test]
		public void DataStoreSaveExceptionSerialisesErrorInfo()
		{
			ObjectId oid = new ObjectId("publisher", new object[] { "0877" } );
			DataStoreSaveException.ErrorInfo[] info = { new DataStoreSaveException.ErrorInfo(oid, "foo") };
			object exBefore = CreateInstance(typeof(DataStoreSaveException), "Message here", info);
			object exAfter = RunThroughSerialization(exBefore);

			Assert.AreEqual(typeof(DataStoreSaveException), exAfter.GetType());
			DataStoreSaveException ex = exAfter as DataStoreSaveException;
			Assert.IsNotNull(ex.Errors, "Should have serialised errors.");
			Assert.AreEqual("0877", ex.Errors[0].ObjectId.PkValues[0]);
			Assert.AreEqual("foo", ex.Errors[0].Message);
		}


		[Test]
		public void OpensAndClosesConnectionInCheckConnection()
		{
			Mock connectionMock = new DynamicMock(typeof(IDbConnection));
			connectionMock.ExpectAndReturn("State", ConnectionState.Closed);
			connectionMock.Expect("Open");
			connectionMock.ExpectAndReturn("State", ConnectionState.Open);
			connectionMock.Expect("Close");
			store = new FakeDataStoreForCheckConnectionTest((IDbConnection)connectionMock.MockInstance);
			store.CheckConnection();

			connectionMock.Verify();
		}

		
		#region Helper class for CheckConnection test

		public class FakeDataStoreForCheckConnectionTest : DbDataStore 
		{
			public FakeDataStoreForCheckConnectionTest(IDbConnection connection): base()
			{
				this.connection = connection;
			}

			protected override object GetFinalPk(DataRow row, IDbCommandBuilder builder)
			{
				throw new NotImplementedException();
			}
		}

		#endregion



		private void loadTitle(string title_id)
		{
			DataTable fetchedTable = store.FetchRows(new FetchSpecification(emapFactory.GetMap(typeof(Title)), Qualifier.Format("TitleId", title_id))).Tables["titles"];
			Assert.AreEqual(1, fetchedTable.Rows.Count, "Wrong number of rows.");
			dataset.Merge(fetchedTable, false, MissingSchemaAction.Ignore);
			Assert.AreEqual(1, titleTable.Rows.Count, "Merge with dataset failed.");
		}


		private Decimal getDifferentPrice(params Decimal[] tabooValues)
		{
			Array.Sort(tabooValues);
			Decimal newPrice = new Random().Next(10, 30);
			foreach(Decimal v in tabooValues)
			{
				if(v > newPrice)
					break;
				else if(newPrice == v)
					newPrice += 1;
			}
			return newPrice;
		}
	}
}
