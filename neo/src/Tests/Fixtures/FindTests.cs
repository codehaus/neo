using System.Collections;
using Neo.Core;
using NUnit.Framework;
using Pubs4.Model;


namespace Neo.Tests.Fixtures
{
	// Some of these tests might duplicate tests in QualifierTests. Not sure yet
	// whether it is sufficient to test in-memory evaluation from here (which also
	// tests code in ObjectContext) or whether we want some base tests that work
	// directly with Qualifiers.

	[TestFixture]
	public class FindTests : TestBase
	{
		protected ObjectContext	context;
		protected Publisher		publisher;


		[SetUp]
		public void SetupStoreAndDataset()
		{
			SetupLog4Net();

			context = GetContext();
			publisher = new PublisherFactory(context).FindObject("1389");
		}


		// We call out to a method to create the context to allow subclasses
		// to run with contexts that use stores or with context subclasses.

		protected virtual ObjectContext GetContext()
		{
			return new ObjectContext(GetTestDataSet());
		}

		
		[Test]
		public void PrimaryKey()
		{
			Assert.IsNotNull(publisher, "Failed to find publisher by primary key.");
		}

		
		[Test]
		public void FetchAllWithLimit()
		{
			IFetchSpecification fSpec;
			IList				result;
			
			fSpec = new FetchSpecification(context.EntityMapFactory.GetMap(typeof(Title)), null, 5);
			result = context.GetObjects(fSpec);
			Assert.AreEqual(5, result.Count, "Should only fetch up to limit.");
		}


		[Test]
		public void SearchForNull() 
		{
			TitleList		titles;
			Title			otherTitle;

			otherTitle = new TitleFactory(context).FindObject("MC3026");
			titles = new TitleFactory(context).Find("Royalty = {0}", null);
			Assert.IsTrue(titles.Count > 0, "Should return some titles.");
			Assert.IsTrue(titles.Contains(otherTitle), "Should return title MC3026 which has NULL royalty.");							 
		}


		[Test]
		public void SearchUsingRelationalOperators()
		{
			TitleList		titles;

			titles = new TitleFactory(context).Find("Advance >= 10125");
			Assert.AreEqual(2, titles.Count, "Should return titles correctly matching.");

		}


		[Test]
		public void SearchUsingLikePredicate()
		{
			TitleList		titles;

			titles = new TitleFactory(context).Find("TheTitle like '%Cooking%'");
			Assert.AreEqual(3, titles.Count, "Should return titles correctly matching.");
		}


		[Test]
		public void ClauseQualifierWithRepeatedColumn()
		{
			TitleList result;

			result = new TitleFactory(context).Find("TitleId = 'TC7777' or TitleId = 'MC3026'");
			Assert.AreEqual(2, result.Count, "Should have found both titles.");
		}


		[Test]
		public void SimpleQualifierForOneToManyRelation()
		{
			TitleList	result;

			result = new TitleFactory(context).Find("Publisher = {0}", publisher);

			Assert.IsTrue(result.Count > 0, "Did not match any rows.");
		}


		[Test]
		public void ClauseQualifierForOneToManyRelation()
		{
			TitleList	result;

			result = new TitleFactory(context).Find("Publisher = {0} and Advance = {1}", publisher, 5000);

			Assert.IsTrue(result.Count > 0, "Did not match any rows.");
		}


		[Test]
		public void SimplePathQualifier()
		{
			TitleList	result;

			result = new TitleFactory(context).Find("Publisher.Name = {0}", publisher.Name);

			Assert.IsTrue(result.Count > 0, "Did not match any rows.");
			foreach(Title title in result)
				Assert.AreEqual(publisher.Name, title.Publisher.Name, "Wrong publisher on title.");
		}

		
		[Test]
		public void SimplePathQualifierOverNullRelationship()
		{
			DiscountList	result;

			result = new DiscountFactory(context).Find("Store.Name = 'Bookbeat'");
			
			Assert.AreEqual(1, result.Count, "Should have found one discount for this store.");
		}


		[Test]
		public void MultiElementPathQualifier()
		{
			TitleAuthorList	result;

			result = new TitleAuthorFactory(context).Find("Title.Publisher.Name = {0}", publisher.Name);

			Assert.IsTrue(result.Count > 0, "Did not match any rows.");
			foreach(TitleAuthor ta in result)
				Assert.AreEqual(publisher.Name, ta.Title.Publisher.Name, "Wrong publisher.");
		}


		[Test]
		public void MultiElementPathWithClauseQualifier()
		{
			TitleAuthorList	result;

			result = new TitleAuthorFactory(context).Find("Title.Publisher.Name = {0} or Title.Publisher.Country = {1}", publisher.Name, publisher.Country);

			Assert.IsTrue(result.Count > 0, "Did not match any rows.");
			foreach(TitleAuthor ta in result)
				Assert.IsTrue(publisher.Name == ta.Title.Publisher.Name || publisher.Country == ta.Title.Publisher.Country, "Invalid ta.");
		}


		[Test]
		public void MultiElementPathAcrossToMany()
		{
			AuthorList result;
			Title	   title;

			result = new AuthorFactory(context).Find("TitleAuthors.Title.Publisher = {0}", publisher);
			Assert.AreEqual(9, result.Count, "Did not match right number of authors.");
			foreach(Author a in result)
			{
				title = null;
				foreach(TitleAuthor ta in a.TitleAuthors)
				{
					if(ta.Title.Publisher == publisher)
					{
						title = ta.Title;
						break;
					}
				}
				if(title == null)
					Assert.Fail("Wrong author.");
			}
		}


		[Test]
		public void PathQualifierWithComplexQualifierAtPathEnd()
		{
			Publisher		publisher2;
			TitleFactory	factory;
			IList			result;
		
			publisher2 = new PublisherFactory(context).FindObject("0736");
			factory = new TitleFactory(context);
			result = factory.Find("Publisher.(PubId = {0} or PubId = {1})", publisher.PubId, publisher2.PubId);

			Assert.AreEqual(11, result.Count, "Should return right number of titles.");
			foreach(Title t in result)
			{
				if(t.Publisher.PubId != publisher.PubId && t.Publisher.PubId != publisher2.PubId)
					Assert.Fail("Should only return relevant titles");
			}
		}


		[Test]
		public void PathQualifierWithNestedClauseAndPathQualifier()
		{
			AuthorList result;
			Title	   title;

			title = new TitleFactory(context).FindObject("MC3021");
			result = new AuthorFactory(context).Find("TitleAuthors.(Title = {0} or Title.Publisher = {1})", title, publisher);
			Assert.AreEqual(11, result.Count, "Did not match right number of authors.");
			foreach(Author a in result)
			{
				title = null;
				foreach(TitleAuthor ta in a.TitleAuthors)
				{
					if((ta.Title.Publisher == publisher) || (ta.Title.TitleId == "MC3021"))
					{
						title = ta.Title;
						break;
					}
				}
				if(title == null)
					Assert.Fail("Wrong author.");
			}
		}

		
		[Test]
		public void FindUnique()
		{
			TitleFactory	factory;
			Title			title;

			factory = new TitleFactory(context);
			title = factory.FindUnique("TitleId = 'TC7777'");
			Assert.AreEqual("TC7777", title.TitleId, "Wrong title.");
		}


		[Test]
		[ExpectedException(typeof(NotUniqueException))]
		public void FindUniqueNone()
		{
			TitleFactory	factory;

			factory = new TitleFactory(context);
			factory.FindUnique("TitleId = 'XX1111'");
		}

		
		[Test]
		[ExpectedException(typeof(NotUniqueException))]
		public void FindUniqueMany()
		{
			TitleFactory	factory;

			factory = new TitleFactory(context);
			factory.FindUnique("Type", "business    ");
		}


		[Test]
		public void FindFirstOne()
		{
			TitleFactory	factory;
			Title			title;

			factory = new TitleFactory(context);
			title = factory.FindFirst("TitleId = 'TC7777'");
			Assert.AreEqual("TC7777", title.TitleId, "Wrong title.");
		}


		[Test]
		public void FindFirstNone()
		{
			TitleFactory	factory;
			Title			title;

			factory = new TitleFactory(context);
			title = factory.FindFirst("TitleId = 'XX1111'");
			Assert.IsNull(title, "Wrong title returned.");
		}


		[Test]
		public void FindSortedWithNoLimit()
		{
			TitleFactory		factory;
			FetchSpecification	spec;
			TitleList			result;
			
			spec = new FetchSpecification();
			spec.SortOrderings = new PropertyComparer[] { new PropertyComparer("PublicationDate", SortDirection.Descending) };

			factory = new TitleFactory(context);
			result = factory.Find(spec);

			Assert.IsTrue(result.Count > 0, "Should return several titles.");
			Assert.AreEqual("PC9999", result[0].TitleId, "Should return title with lastest date.");
		}

		
		[Test]
		public void FindSortedWithExcessiveLimit()
		{
			TitleFactory		factory;
			FetchSpecification	spec;
			TitleList			result;
			
			spec = new FetchSpecification();
			spec.SortOrderings = new PropertyComparer[] { new PropertyComparer("PublicationDate", SortDirection.Descending) };
			spec.FetchLimit = 5000;

			factory = new TitleFactory(context);
			result = factory.Find(spec);

			Assert.IsTrue(result.Count > 0, "Should return several titles.");
			Assert.AreEqual("PC9999", result[0].TitleId, "Should return title with lastest date.");
		}
		
		
		[Test]
		public void FindSortedWithLimitAndQualifier()
		{
			TitleFactory		factory;
			FetchSpecification	spec;
			TitleList			result;
			
			spec = new FetchSpecification();
			spec.Qualifier = Qualifier.Format("Publisher.Name = {0}", publisher.Name);
			spec.SortOrderings = new PropertyComparer[] { new PropertyComparer("PublicationDate", SortDirection.Ascending) };
			spec.FetchLimit = 1;

			factory = new TitleFactory(context);
			result = factory.Find(spec);

			Assert.AreEqual(1, result.Count, "Should return just one.");
			Assert.AreEqual("BU1111", result[0].TitleId, "Should return title with earliest date for this publisher.");
		}


	}
}
