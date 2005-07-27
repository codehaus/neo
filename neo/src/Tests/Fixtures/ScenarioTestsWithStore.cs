using System;
using Neo.Core;
using Neo.Database;
using NUnit.Framework;
using Pubs4.Model;

namespace Neo.Tests.Fixtures
{
	[TestFixture]
	public class ScenarioTestsWithStore : ScenarioTests
	{
		protected DbDataStore	store;


		protected override ObjectContext GetContext()
		{
			store = (DbDataStore)GetDataStore();
			return new ObjectContext(store);
		}

		
		protected override DateTime GetDate(int y, int m, int d)
		{
			return new DateTime(y, m, d);
		}


		[Test]
		public void StoreShouldUseExistingTransaction()
		{
			title.Publisher = null; // change something, anything

			store.BeginTransaction();
			context.SaveChanges();
			store.RollbackTransaction();
		}


		[Test]
		public void ContextShouldUpdateObjectTableForStoreGeneratedKeys()
		{
			JobFactory factory = new JobFactory(context);
			int jobCountBefore = factory.FindAllObjects().Count;

			Job aJob = factory.CreateObject();
			aJob.Description = "test";
			aJob.MaxLevel = 10;
			aJob.MinLevel = 10;
			
			store.BeginTransaction();
			context.SaveChanges();
			int jobCountAfter = factory.FindAllObjects().Count;
			store.RollbackTransaction();

			Assert.AreEqual(jobCountBefore + 1, jobCountAfter, "There should be only one job-object more.");
		}


		[Test]
		public void FetchesWithSpansRetrieveAdditionalObjects()
		{
			context.IgnoresDataStore = true;
			Assert.AreEqual(0, new PublisherFactory(context).FindAllObjects().Count, "Should not contain publishers before fetching.");

			context.IgnoresDataStore = false;
			FetchSpecification fetchSpec = new FetchSpecification(context.EntityMapFactory.GetMap(typeof(Title)));
			fetchSpec.Spans = new string[]{ "Publisher" };
			context.GetObjects(fetchSpec);

			context.IgnoresDataStore = true;
			Assert.IsTrue(new PublisherFactory(context).FindAllObjects().Count > 0, "Should have fetched publisher with titles.");
		}


	}

}
