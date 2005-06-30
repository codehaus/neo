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
			return new NonLoadingObjectContext(store);
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
			NonLoadingObjectContext nlcontext = (NonLoadingObjectContext)context;

			nlcontext.IgnoresDataStore = true;
			Assert.AreEqual(0, new PublisherFactory(nlcontext).FindAllObjects().Count, "Should not contain publishers before fetching.");

			nlcontext.IgnoresDataStore = false;
			IFetchSpecification fetchSpec = new FetchSpecification(context.EntityMapFactory.GetMap(typeof(Title)));
			context.GetObjects(fetchSpec, new string[]{ "Publisher" });

			nlcontext.IgnoresDataStore = true;
			Assert.IsTrue(new PublisherFactory(nlcontext).FindAllObjects().Count > 0, "Should have fetched publisher with titles.");
		}


		#region Helper class

		private class NonLoadingObjectContext : ObjectContext
		{
			private bool ignoresDataStore; 

			public NonLoadingObjectContext(IDataStore store) : base(store)
			{
			}

			protected override bool CanLoadFromStore
			{
				get	{ return ((this.ignoresDataStore == false) && base.CanLoadFromStore); }
			}

			public bool IgnoresDataStore
			{
				get { return ignoresDataStore; }
				set { ignoresDataStore = value; }
			}
		}

		#endregion

	}

}
