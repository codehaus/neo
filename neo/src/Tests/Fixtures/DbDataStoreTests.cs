using System;
using System.Collections;
using System.Data;
using Neo.Core;
using Neo.Core.Util;
using Neo.Database;
using Neo.OracleClient;
using NUnit.Framework;
using Pubs4.Model;


namespace Neo.Tests.Fixtures
{
	// Of course, DbDataStore is abstract. This fixture will test either 
	// the SqlDataStore or the OracleDataStore, see test.config for details.

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
			Assertion.AssertEquals("Wrong title.", "TC7777", titleTable.Rows[0]["title_id"]);
		}


		[Test]
		public void FetchAll()
		{
			DataTable fetchedTable = store.FetchRows(new FetchSpecification(emapFactory.GetMap(typeof(Title))));
			Assertion.Assert("There should be more than 15 titles.", fetchedTable.Rows.Count > 15);
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
		    ArrayList tables = new ArrayList();
			tables.Add(new OrderTable(titleTable));
			store.ProcessUpdates(tables);
			store.CommitTransaction();
			Assertion.AssertEquals("Wrong row state.", DataRowState.Unchanged, titleTable.Rows[0].RowState);

			fetchedTable = store.FetchRows(new FetchSpecification(emapFactory.GetMap(typeof(Title)), Qualifier.Format("TitleId", "TC7777")));
			// Don't use AssertEquals with Decimals. (Would compare their string value which is precision dependent.)
			Assertion.Assert("Price not updated in database.", newPrice.Equals(fetchedTable.Rows[0]["price"]));
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
			ArrayList tables = new ArrayList();
			tables.Add(new OrderTable(dataset.Tables["titles"]));
			store.ProcessUpdates(tables);
			store.RollbackTransaction();
			Assertion.AssertEquals("Wrong row state.", DataRowState.Modified, titleTable.Rows[0].RowState);

			fetchedTable = store.FetchRows(new FetchSpecification(emapFactory.GetMap(typeof(Title)), Qualifier.Format("TitleId" , "TC7777")));
			Assertion.AssertEquals("Price was updated in database.", oldPrice, fetchedTable.Rows[0]["price"]);
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
			ArrayList tables = new ArrayList();
			tables.Add(new OrderTable(dataset.Tables["titles"]));
			store.ProcessUpdates(tables);

			store.CommitTransaction();

			titleRow = dataset2.Tables["titles"].Rows[0];
			newPrice2 = getDifferentPrice((Decimal)titleRow["price"], newPrice1);
			titleRow["price"] = newPrice2;
			store.BeginTransaction();
			ArrayList tables2 = new ArrayList();
			tables2.Add(new OrderTable(dataset2.Tables["titles"]));
			store.ProcessUpdates(tables2);
			hadErrors = dataset2.Tables["titles"].HasErrors;
			store.RollbackTransaction();
			Assertion.Assert("No error for conflicting concurrent updates.", hadErrors);
		}

		[Test]
		public void Delete()
		{
			DataTable	fetchedTable;

			loadTitle("MC3026"); // this doesn't have any authors and can be deleted
			dataset.Tables["titles"].Rows.Find("MC3026").Delete();
			store.BeginTransaction();

			ArrayList tables = new ArrayList();
			foreach(DataTable t in dataset.Tables)
			{
				tables.Add(new OrderTable(t));
			}
			store.ProcessDeletes(tables);
			fetchedTable = store.FetchRows(new FetchSpecification(emapFactory.GetMap(typeof(Title)), Qualifier.Format("TitleId", "MC3026")));
			store.RollbackTransaction();
			Assertion.Assert("Row not deleted from database.", fetchedTable.Rows.Count == 0);
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
			ArrayList tables = new ArrayList();
			foreach(DataTable t in jobOnlyDataSet.Tables)
			{
				tables.Add(new OrderTable(t));
			}
			store.ProcessInserts(tables, changeTables);
			changeTable = (PkChangeTable) changeTables[0];
			finalId = (Int16)newRow["job_id"];
			store.RollbackTransaction();

			Assertion.AssertEquals("Inconsistent old value in change table.", tempId, changeTable[0].OldValue);
			Assertion.AssertEquals("Inconsistent new value in change table.", finalId, changeTable[0].NewValue);
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
			ArrayList tables = new ArrayList();
			tables.Add(new OrderTable(jobTable));
			store.ProcessInserts(tables, new ArrayList());
			finalId = (Int16)newJobRow["job_id"];
			finalForeignId = (Int16)newJobRefRow["job_id"];
			store.RollbackTransaction();

			Assertion.Assert("Final id should be different from temp id.", finalId.Equals(tempId) == false);
			Assertion.AssertEquals("Value should be propagated to child table.", finalId, finalForeignId);
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
			ArrayList tables = new ArrayList();
			foreach(DataTable t in dataset.Tables)
			{
				tables.Add(new OrderTable(t));
			}

			store.ProcessInserts(tables, changeTables);

			Assertion.AssertEquals("Should not have created change tables", 0, changeTables.Count);
		}

		[Test] 
		public void AssignedOrderToInsertTablesIsCorrect() 
		{
			emapFactory.GetMap(typeof(Publisher)).UpdateSchemaInDataSet(dataset, SchemaUpdate.Basic | SchemaUpdate.Constraints);
			OrderTable titleTable = new OrderTable(dataset.Tables["titles"]);
			
			OrderTable publisherTable = new OrderTable(dataset.Tables["publishers"]);
			titleTable.Table.ParentRelations.Add(publisherTable.Table.Columns["pub_id"],titleTable.Table.Columns["pub_id"]);
	
			ArrayList insertTables = new ArrayList();		
			insertTables.Add(titleTable);
			insertTables.Add(publisherTable);

			store.AssignOrderToInsertTables(insertTables);

			Assertion.AssertEquals("publisher should has wrong order number", 1, publisherTable.Order );
			Assertion.AssertEquals("title should has wrong order number", 0, titleTable.Order );
			// not sorted yet :-)
			Assertion.AssertEquals("The first item should have order of 0", 0, ((OrderTable)insertTables[0]).Order);
			Assertion.AssertEquals("The first item should have order of 1", 1, ((OrderTable)insertTables[1]).Order);

		}

		[Test]
		public void SortedOrderOfInsertTablesIsCorrect()
		{
			emapFactory.GetMap(typeof(Publisher)).UpdateSchemaInDataSet(dataset, SchemaUpdate.Basic | SchemaUpdate.Constraints);
			OrderTable titleTable = new OrderTable(dataset.Tables["titles"]);
			
			OrderTable publisherTable = new OrderTable(dataset.Tables["publishers"]);
			titleTable.Table.ParentRelations.Add(publisherTable.Table.Columns["pub_id"],titleTable.Table.Columns["pub_id"]);
	
			ArrayList insertTables = new ArrayList();		
			insertTables.Add(titleTable);
			insertTables.Add(publisherTable);

			store.AssignOrderToInsertTables(insertTables);
			store.SortOrderOfInsertTables(insertTables);

			Assertion.AssertEquals("The first item should have order of 1", 1, ((OrderTable)insertTables[0]).Order);
			Assertion.AssertEquals("The first item should have order of 0", 0, ((OrderTable)insertTables[1]).Order);
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

			fetchedTable = store.FetchRows(spec);
			Assertion.Assert("Should have more than 2 rows", fetchedTable.Rows.Count > 2);

			prevRow = fetchedTable.Rows[0];
			foreach(DataRow currRow in fetchedTable.Rows)
			{
				Assertion.Assert("Should return titles in correct order.", (DateTime)prevRow["pubdate"] <= (DateTime)currRow["pubdate"]);
				if((DateTime)prevRow["pubdate"] == (DateTime)currRow["pubdate"])
					Assertion.Assert("Should return titles in correct order.", Comparer.Default.Compare((String)prevRow["type"], (String)currRow["type"]) >= 0);
				prevRow = currRow;
			}
		}


		[Test]
		public void UpdateTableWithIdentityColumn()
		{
			DataTable	  fetchedTable;

			if(store is OracleDataStore)
				return;
			
			fetchedTable = store.FetchRows(new FetchSpecification(emapFactory.GetMap(typeof(Job)), Qualifier.Format("JobId", 1)));
			Assertion.AssertEquals("Wrong number of rows.", 1, fetchedTable.Rows.Count);
			fetchedTable.Rows[0]["max_lvl"] = 20;
			store.BeginTransaction();
			ArrayList tables = new ArrayList();
			tables.Add(new OrderTable(fetchedTable));
			store.ProcessUpdates(tables);
			fetchedTable = store.FetchRows(new FetchSpecification(emapFactory.GetMap(typeof(Job)), Qualifier.Format("JobId", 1)));
			store.RollbackTransaction();

			Assertion.AssertEquals("Wrong number of rows.", 1, fetchedTable.Rows.Count);
			Assertion.AssertEquals("Value not updated.", 20, fetchedTable.Rows[0]["max_lvl"]);
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

		
		private void loadTitle(string title_id)
		{
			DataTable fetchedTable = store.FetchRows(new FetchSpecification(emapFactory.GetMap(typeof(Title)), Qualifier.Format("TitleId", title_id)));
			Assertion.AssertEquals("Wrong number of rows.", 1, fetchedTable.Rows.Count);
			dataset.Merge(fetchedTable, false, MissingSchemaAction.Ignore);
			Assertion.AssertEquals("Merge with dataset failed.", 1, titleTable.Rows.Count);
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