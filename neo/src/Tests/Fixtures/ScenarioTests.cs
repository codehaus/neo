using System;
using System.Collections;
using System.Data;
using NUnit.Framework;
using Neo.Core;
using Pubs4.Model;


namespace Neo.Tests
{
	[TestFixture]
	public class ScenarioTests : TestBase
	{
		protected ObjectContext	context;
		protected Title			title;


		[SetUp]
		public void LoadDataSetAndGetTitleObject()
		{
			SetupLog4Net();
			
			context = GetContext();
			title = new TitleFactory(context).FindObject("TC7777");
			Assertion.AssertNotNull("Failed to find title object.", title);
		}


		// We call out to a method to create the context to allow subclasses
		// to run with contexts that use stores or with context subclasses.

		protected virtual ObjectContext GetContext()
		{
			return new ObjectContext(GetTestDataSet());
		}

		// DataSets convert to local time when reading from XML. We call
		// this to convert our test dates into what the DataSet will have.

		protected virtual DateTime GetDate(int y, int m, int d)
		{
			return TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(y, m, d));
		}
		

		[Test]
		public void ReadAttributes()
		{
			Assertion.AssertEquals("Wrong title.", "Sushi, Anyone?", title.TheTitle);
			Assertion.AssertEquals("Wrong royalty.", 10, title.Royalty);
			Assertion.AssertEquals("Wrong date.", GetDate(1991, 06, 12), title.PublicationDate);
		}

		[Test]
		public void WriteAttributes()
		{
			title.TheTitle = "SUSHI";
			Assertion.AssertEquals("String prop not changed.", "SUSHI", title.TheTitle);
			title.PublicationDate = new DateTime(2003, 01, 01);
			Assertion.AssertEquals("DateTime prop not changed.", new DateTime(2003, 01, 01), title.PublicationDate);
		}

			
		[Test]
		public void RefetchedObjectsAreIdenticalAndUnmodified()
		{
			Title		title;
			TitleList	result;

			title = new TitleFactory(context).FindObject("TC7777");
			Assertion.Assert("Object should not have changes.", title.HasChanges() == false);
			// now refetch and cause a merge
			result = new TitleFactory(context).FindAllObjects();
			
			Assertion.Assert("Explicitly fetched title should be in list.", result.Contains(title));
			Assertion.Assert("Object should not have changes.", title.HasChanges() == false);

		}


		[Test]
		public void RefetchDoesNotClobberUpdates()
		{
			Assertion.Assert("Unexpected initial type.", title.Type != "foo");
			title.Type = "foo";
			Assertion.AssertEquals("Unexpected type after change.", "foo", title.Type);
			new TitleFactory(context).FindAllObjects();
			
			Assertion.AssertEquals("Refetch should not revert object to database state.", "foo", title.Type);
		}


		[Test]
		public void ReadRelationships()
		{
			Publisher	publisher, otherPublisher;
			Title		otherTitle;

			// to-one
			publisher = title.Publisher;
			Assertion.AssertNotNull("Failed to find publisher for title.", publisher);
			Assertion.AssertEquals("Wrong publisher for title.", "Binnet & Hardley", publisher.Name);

			// to-many
			Assertion.AssertNotNull("Failed to find titles for publisher.", publisher.Titles);
			Assertion.AssertEquals("Wrong number of titles for publisher.", 7, publisher.Titles.Count);
		
			// get another title/publisher
			otherTitle = (Title)context.GetObjectFromTable("titles", new object[] { "BU1032" });
			Assertion.AssertNotNull("Failed to find other title object.", otherTitle);
			otherPublisher = otherTitle.Publisher;
			Assertion.Assert("Other title is by same publisher.", otherPublisher != publisher);
		}


		[Test]
		public void WriteRelationships()
		{
			Publisher	publisher, otherPublisher;
			Title		otherTitle;
			int			countBefore, countAfter;

			publisher = title.Publisher;
			otherTitle = new TitleFactory(context).FindObject("BU1032");
			otherPublisher = otherTitle.Publisher;

			// set other title's publisher to b&h and check
			otherTitle.Publisher = publisher;
			Assertion.Assert("Other title's publisher is not B&H now.", otherTitle.Publisher == publisher);
			Assertion.Assert("Original publisher's title list still contains title.", otherPublisher.Titles.Contains(otherTitle) == false);
			Assertion.Assert("Publisher's title list does not contain new title", publisher.Titles.Contains(otherTitle));
			
			// to-many / set, i.e. use  publisher.addToTitles to change back and check
			otherPublisher.Titles.Add(otherTitle);
			Assertion.Assert("Other title's publisher is not original publisher", otherTitle.Publisher == otherPublisher);
			Assertion.Assert("B&H title list still contains title", publisher.Titles.Contains(otherTitle) == false);

			// add the title another time. this should duplivate the entry
			countBefore = otherPublisher.Titles.Count;
			otherPublisher.Titles.Add(otherTitle);
			countAfter = otherPublisher.Titles.Count;
			Assertion.AssertEquals("Wrong number of titles after redundant add.", countBefore, countAfter);
		}

		
		[Test]
		public void RelationshipsAndNewObjects()
		{
			Publisher	publisher;
			Title		newTitle;

			publisher = title.Publisher;
			
			// add an object to a relationship that has been loaded
			publisher.Titles.Touch();
			newTitle = new TitleFactory(context).CreateObject("XX1111");
			newTitle.Publisher = publisher;
			Assertion.Assert("Publisher does not contain new title.", publisher.Titles.Contains(newTitle));

			// new try adding with an "untouched" list in publisher
			publisher.Titles.InvalidateCache();
			newTitle = new TitleFactory(context).CreateObject("XX2222");
			newTitle.Publisher = publisher;
			Assertion.Assert("Publisher does not contain new title.", publisher.Titles.Contains(newTitle));
		}


		[Test]
		[ExpectedException(typeof(DeletedRowInaccessibleException))]
		public void DeleteObject()
		{
			title.Delete();
			Console.WriteLine(title.TheTitle); // do something that won't be optimised away.
		}


		[Test]
		[ExpectedException(typeof(DeletedRowInaccessibleException))]
		public void CascadingDeletions()
		{
			TitleAuthor	ta = title.TitleAuthors[0];
			title.Delete();
			Console.WriteLine(ta.Title);	// do something that won't be optimised away.
		}

		
		[Test]
		[ExpectedException(typeof(RowNotInTableException))]
		public void CascadingDeletionsWithNewObjects()
		{
			Title		newTitle;
			Author		author;
			TitleAuthor	ta;

			author = new AuthorFactory(context).FindObject("486-29-1786");
			newTitle = new TitleFactory(context).CreateObject("ZZ1234");
			ta = new TitleAuthorFactory(context).CreateObject(author, newTitle);

			newTitle.Delete();
			// this will not throw a DeletedRowInaccessibleException but a 
			// RowNotInTableException because the row was new and, in ADO.NET terms,
			// was therefore not deleted but merely removed from the table...
			Console.WriteLine(ta.Title);	// do something that won't be optimised away.
		}


		[Test]
		public void CascadingDeletionsWithNewObjectsAndRecreations()
		{
			Author		author;
			TitleAuthor	ta;

			author = new AuthorFactory(context).FindObject("486-29-1786");
			ta = new TitleAuthorFactory(context).CreateObject(author, title);
			title.Delete();

			title = new TitleFactory(context).CreateObject("TC7777");
			ta = new TitleAuthorFactory(context).CreateObject(author, title);
		}


		[Test]
		public void SetNullDeletions()
		{
			Publisher	publisher;

			publisher = title.Publisher;
			publisher.Titles.Touch(); // make sure the list is loaded
			title.Delete();
			Assertion.Assert("Deleted object did not disappear from rel.", publisher.Titles.Contains(title) == false);
		}

		
		[Test]
		public void RepeatedAddRemovesWithRelation()
		{
			Publisher	publisher;
			Title		newTitle;

			publisher = title.Publisher;
			for(int i = 0; i < 10; i++)
			{
				newTitle = new TitleFactory(context).CreateObject("XX1111");
				publisher.Titles.Add(newTitle);
				newTitle.Delete();
			}
		}


		[Test]
		public void ManyToManyWithCorrelationObjects()
		{
			TitleAuthor	corr;
			Author      author;

			author = new AuthorFactory(context).FindObject("486-29-1786");
			Assertion.AssertNotNull("Did not find author.", author);

			corr = new TitleAuthorFactory(context).CreateObject(author, title);
			Assertion.Assert("Title not in Author.", author.TitleAuthors.Contains(corr));
			Assertion.Assert("Author not in Title.", title.TitleAuthors.Contains(corr));
			corr.Delete();

			// Now try the same but with the list touched before
			title.TitleAuthors.Touch();
			corr = new TitleAuthorFactory(context).CreateObject(author, title);
			Assertion.Assert("Title not in Author.", author.TitleAuthors.Contains(corr));
			Assertion.Assert("Author not in Title.", title.TitleAuthors.Contains(corr));
			corr.Delete();
		}


		[Test]
		public void GetReferencesIncludingCascades()
		{
			Publisher referencedPublisher = title.Publisher;
			Publisher unreferencedPublisher = new PublisherFactory(context).FindObject("1622");
			Title referencedTitle = title;
			Title unreferencedTitle = new TitleFactory(context).FindObject("MC3026");

			Assertion.Assert("The publisher should be referenced", referencedPublisher.GetReferences().Count > 0);
			Assertion.Assert("The publisher should not be referenced", unreferencedPublisher.GetReferences().Count == 0);
			Assertion.Assert("The title should be referenced", referencedTitle.GetReferences().Count > 0);
			Assertion.Assert("The title should not be referenced", unreferencedTitle.GetReferences().Count == 0);
		}


		[Test]
		public void GetReferencesIgnoringCascades()
		{
			Publisher referencedPublisher = title.Publisher;
			Publisher unreferencedPublisher = new PublisherFactory(context).FindObject("1622");
			Title referencedTitle = title;
			Title unreferencedTitle = new TitleFactory(context).FindObject("MC3026");

			Assertion.Assert("The publisher should be referenced", referencedPublisher.GetReferences(true).Count > 0);
			Assertion.Assert("The publisher should not be referenced", unreferencedPublisher.GetReferences(true).Count == 0);
			Assertion.Assert("The title should not be referenced", referencedTitle.GetReferences(true).Count == 0);
			Assertion.Assert("The title should not be referenced", unreferencedTitle.GetReferences(true).Count == 0);
		}


	}
}
