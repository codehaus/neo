using System;
using Neo.Core;
using Neo.Core.Util;
using Neo.Database;
using NUnit.Framework;
using Pubs4.Model;


namespace Neo.Tests.Fixtures
{
	[NUnit.Framework.TestFixture]
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
			JobFactory	factory;
			int			jobCountBefore, jobCountAfter;

			factory = new JobFactory(context);
			jobCountBefore = factory.FindAllObjects().Count;

			Job aJob = factory.CreateObject();
			aJob.Description = "test";
			aJob.MaxLevel = 10;
			aJob.MinLevel = 10;
			
			store.BeginTransaction();
			context.SaveChanges();
			jobCountAfter = factory.FindAllObjects().Count;
			store.RollbackTransaction();

			Assert.AreEqual(jobCountBefore + 1, jobCountAfter, "There should be only one job-object more.");
		}


		private void PrintJobs(JobList jobs)
		{
			foreach(Job j in jobs)
				Console.WriteLine(j.ToStringAllProperties());
		}

	}

}
