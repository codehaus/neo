using System.Collections;
using Neo.Core;
using NUnit.Framework;
using Pubs4.Model;


namespace Neo.Tests
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
			Assertion.AssertNotNull("Failed to find publisher by primary key.", publisher);
		}

		
		[Test]
		public void FetchAllWithLimit()
		{
			IFetchSpecification fSpec;
			IList				result;
			
			fSpec = new FetchSpecification(context.EntityMapFactory.GetMap(typeof(Title)), null, 5);
			result = context.GetObjects(fSpec);
			Assertion.AssertEquals("Should only fetch up to limit.", 5, result.Count);
		}


		[Test]
		public void SearchForNull() 
		{
			TitleList		titles;
			Title			otherTitle;

			otherTitle = new TitleFactory(context).FindObject("MC3026");
			titles = new TitleFactory(context).Find("Royalty = {0}", null);
			Assertion.Assert("Should return some titles.", titles.Count > 0);
			Assertion.Assert("Should return title MC3026 which has NULL royalty.", titles.Contains(otherTitle));							 
		}


		[Test]
		public void SearchUsingRelationalOperators()
		{
			TitleList		titles;

			titles = new TitleFactory(context).Find("Advance >= 10125");
			Assertion.AssertEquals("Should return titles correctly matching.", 2, titles.Count);

		}


		[Test]
		public void SearchUsingLikePredicate()
		{
			TitleList		titles;

			titles = new TitleFactory(context).Find("TheTitle like '%Cooking%'");
			Assertion.AssertEquals("Should return titles correctly matching.", 3, titles.Count);
		}


		[Test]
		public void ClauseQualifierWithRepeatedColumn()
		{
			TitleList result;

			result = new TitleFactory(context).Find("TitleId = 'TC7777' or TitleId = 'MC3026'");
			Assertion.AssertEquals("Should have found both titles.", 2, result.Count);
		}


		[Test]
		public void SimpleQualifierForOneToManyRelation()
		{
			TitleList	result;

			result = new TitleFactory(context).Find("Publisher = {0}", publisher);

			Assertion.Assert("Did not match any rows.", result.Count > 0);
		}


		[Test]
		public void ClauseQualifierForOneToManyRelation()
		{
			TitleList	result;

			result = new TitleFactory(context).Find("Publisher = {0} and Advance = {1}", publisher, 5000);

			Assertion.Assert("Did not match any rows.", result.Count > 0);
		}


		[Test]
		public void SimplePathQualifier()
		{
			TitleList	result;

			result = new TitleFactory(context).Find("Publisher.Name = {0}", publisher.Name);

			Assertion.Assert("Did not match any rows.", result.Count > 0);
			foreach(Title title in result)
				Assertion.AssertEquals("Wrong publisher on title.", publisher.Name, title.Publisher.Name);
		}

		
		[Test]
		public void SimplePathQualifierOverNullRelationship()
		{
			DiscountList	result;

			result = new DiscountFactory(context).Find("Store.Name = 'Bookbeat'");
			
			Assertion.AssertEquals("Should have found one discount for this store.", 1, result.Count);
		}


		[Test]
		public void MultiElementPathQualifier()
		{
			TitleAuthorList	result;

			result = new TitleAuthorFactory(context).Find("Title.Publisher.Name = {0}", publisher.Name);

			Assertion.Assert("Did not match any rows.", result.Count > 0);
			foreach(TitleAuthor ta in result)
				Assertion.AssertEquals("Wrong publisher.", publisher.Name, ta.Title.Publisher.Name);
		}


		[Test]
		public void MultiElementPathWithClauseQualifier()
		{
			TitleAuthorList	result;

			result = new TitleAuthorFactory(context).Find("Title.Publisher.Name = {0} or Title.Publisher.Country = {1}", publisher.Name, publisher.Country);

			Assertion.Assert("Did not match any rows.", result.Count > 0);
			foreach(TitleAuthor ta in result)
				Assertion.Assert("Invalid ta.", publisher.Name == ta.Title.Publisher.Name || publisher.Country == ta.Title.Publisher.Country);
		}


		[Test]
		public void MultiElementPathAcrossToMany()
		{
			AuthorList result;
			Title	   title;

			result = new AuthorFactory(context).Find("TitleAuthors.Title.Publisher = {0}", publisher);
			Assertion.AssertEquals("Did not match right number of authors.", 9, result.Count);
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
					Assertion.Fail("Wrong author.");
			}
		}


		[Test]
		public void FindUnique()
		{
			TitleFactory	factory;
			Title			title;

			factory = new TitleFactory(context);
			title = factory.FindUnique("TitleId = 'TC7777'");
			Assertion.AssertEquals("Wrong title.", "TC7777", title.TitleId);
		}


		[Test]
		[ExpectedException(typeof(NotUniqueException))]
		public void FindUniqueNone()
		{
			TitleFactory	factory;
			Title			title;

			factory = new TitleFactory(context);
			title = factory.FindUnique("TitleId = 'XX1111'");
		}

		
		[Test]
		[ExpectedException(typeof(NotUniqueException))]
		public void FindUniqueMany()
		{
			TitleFactory	factory;
			Title			title;

			factory = new TitleFactory(context);
			try
			{
				title = factory.FindUnique("Type", "business    ");
			}
			catch (NotUniqueException e)
			{
				Assertion.AssertEquals("Incorrect exception message.",
					"[NEO] The query 'Type' for parameters 'business    ' was not unique.",
					e.Message);
				throw e;
			}

			Assertion.Fail("Should have thrown a NotUniqueException.");
		}


		[Test]
		public void FindFirstOne()
		{
			TitleFactory	factory;
			Title			title;

			factory = new TitleFactory(context);
			title = factory.FindFirst("TitleId = 'TC7777'");
			Assertion.AssertEquals("Wrong title.", "TC7777", title.TitleId);
		}


		[Test]
		public void FindFirstNone()
		{
			TitleFactory	factory;
			Title			title;

			factory = new TitleFactory(context);
			title = factory.FindFirst("TitleId = 'XX1111'");
			Assertion.AssertNull("Wrong title returned.", title);
		}


	}
}