using System;
using System.Data;
using NUnit.Framework;
using Neo.Core;
using Neo.Core.Util;
using Neo.SqlClient;
using Pubs4.Model;


namespace Neo.Tests
{
	[TestFixture]
	public class SqlDataStoreTests : TestBase
	{
		IEntityMapFactory emapFactory;
		SqlDataStore	  store;
		DataSet			  dataset;
		DataTable		  titleTable, jobTable;


		[SetUp]
		public void SetupStoreAndDataset()
		{
			SetupLog4Net();

			emapFactory = DefaultEntityMapFactory.SharedInstance;
			store = GetDataStore();

			dataset = new DataSet();
			emapFactory.GetMap(typeof(Title)).UpdateSchemaInDataSet(dataset, SchemaUpdate.Basic | SchemaUpdate.Constraints);
			titleTable = dataset.Tables["titles"];
			emapFactory.GetMap(typeof(Job)).UpdateSchemaInDataSet(dataset, SchemaUpdate.Basic | SchemaUpdate.Constraints);
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
			store.Save(titleTable);
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
			store.Save(dataset.Tables["titles"]);
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
			store.Save(dataset.Tables["titles"]);
			store.CommitTransaction();

			titleRow = dataset2.Tables["titles"].Rows[0];
			newPrice2 = getDifferentPrice((Decimal)titleRow["price"], newPrice1);
			titleRow["price"] = newPrice2;
			store.BeginTransaction();
			store.Save(dataset2.Tables["titles"]);
			hadErrors = dataset2.Tables["titles"].HasErrors;
			store.RollbackTransaction();
			Assertion.Assert("No error for conflicting concurrent updates.", hadErrors);
		}


		[Test]
		public void Insert()
		{
			DataTable	fetchedTable;
			DataRow		newRow;
			
			fetchedTable = store.FetchRows(new FetchSpecification(emapFactory.GetMap(typeof(Title)), Qualifier.Format("TitleId", "XX9999")));
			Assertion.Assert("Row with test primary key exists in database.", fetchedTable.Rows.Count == 0);

			newRow = titleTable.NewRow();
			newRow["title_id"] = "XX9999";
			newRow["title"] = "Test row for automated tests";
			newRow["type"] = "TEST";
			newRow["pubdate"] = new DateTime(2000, 1, 1);
			titleTable.Rows.Add(newRow);

			store.BeginTransaction();
			store.Save(dataset.Tables["titles"]);
			fetchedTable = store.FetchRows(new FetchSpecification(emapFactory.GetMap(typeof(Title)), Qualifier.Format("TitleId", "XX9999")));
			store.RollbackTransaction();
			Assertion.Assert("New row not in database.", fetchedTable.Rows.Count > 0);
		}


		[Test]
		public void Delete()
		{
			DataTable	fetchedTable;

			loadTitle("MC3026"); // this doesn't have any authors and can be deleted
			dataset.Tables["titles"].Rows.Find("MC3026").Delete();
			store.BeginTransaction();
			store.Save(dataset.Tables["titles"]);
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

			newRow = jobTable.NewRow();
			newRow["job_desc"] = "Test row for automated tests";
			newRow["min_lvl"] = 100;
			newRow["max_lvl"] = 200;
			jobTable.Rows.Add(newRow);
			tempId = (System.Int16)newRow["job_id"];

			store.BeginTransaction();
			changeTable = store.Save(dataset.Tables["jobs"]);
			finalId = (System.Int16)newRow["job_id"];
			store.RollbackTransaction();
			Assertion.AssertEquals("Inconsistent old value in change table.", tempId, changeTable[0].OldValue);
			Assertion.AssertEquals("Inconsistent new value in change table.", finalId, changeTable[0].NewValue);
		}


		[Test]
		public void UpdateTableWithIdentityColumn()
		{
			DataTable	  fetchedTable;

			fetchedTable = store.FetchRows(new FetchSpecification(emapFactory.GetMap(typeof(Job)), Qualifier.Format("JobId", 1)));
			Assertion.AssertEquals("Wrong number of rows.", 1, fetchedTable.Rows.Count);
			fetchedTable.Rows[0]["max_lvl"] = 20;
			store.BeginTransaction();
			store.Save(fetchedTable);
			fetchedTable = store.FetchRows(new FetchSpecification(emapFactory.GetMap(typeof(Job)), Qualifier.Format("JobId", 1)));
			store.RollbackTransaction();

			Assertion.AssertEquals("Wrong number of rows.", 1, fetchedTable.Rows.Count);
			Assertion.AssertEquals("Value not updated.", 20, fetchedTable.Rows[0]["max_lvl"]);
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