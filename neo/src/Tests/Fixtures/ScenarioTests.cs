using System;
using System.Data;
using Neo.Core;
using NUnit.Framework;
using Pubs4.Model;

namespace Neo.Tests.Fixtures
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
		    Assert.IsNotNull(title, "Failed to find title object.");
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
			Assert.AreEqual("Sushi, Anyone?", title.TheTitle, "Wrong title.");
			Assert.AreEqual(10, title.Royalty, "Wrong royalty.");
			Assert.AreEqual(GetDate(1991, 06, 12), title.PublicationDate, "Wrong date.");
		}

		[Test]
		public void WriteAttributes()
		{
			title.TheTitle = "SUSHI";
			Assert.AreEqual("SUSHI", title.TheTitle, "String prop not changed.");
			title.PublicationDate = new DateTime(2003, 01, 01);
			Assert.AreEqual(new DateTime(2003, 01, 01), title.PublicationDate, "DateTime prop not changed.");
		}

			
		[Test]
		public void RefetchedObjectsAreIdenticalAndUnmodified()
		{
			Title		title;
			TitleList	result;

			title = new TitleFactory(context).FindObject("TC7777");
			Assert.IsTrue(title.HasChanges() == false, "Object should not have changes.");
			// now refetch and cause a merge
			result = new TitleFactory(context).FindAllObjects();
			
			Assert.IsTrue(result.Contains(title), "Explicitly fetched title should be in list.");
			Assert.IsTrue(title.HasChanges() == false, "Object should not have changes.");

		}


		[Test]
		public void RefetchDoesNotClobberUpdates()
		{
			Assert.IsTrue(title.Type != "foo", "Unexpected initial type.");
			title.Type = "foo";
			Assert.AreEqual("foo", title.Type, "Unexpected type after change.");
			new TitleFactory(context).FindAllObjects();
			
			Assert.AreEqual("foo", title.Type, "Refetch should not revert object to database state.");
		}


		[Test]
		public void ReadRelationships()
		{
			Publisher	publisher, otherPublisher;
			Title		otherTitle;

			// to-one
			publisher = title.Publisher;
			Assert.IsNotNull(publisher, "Failed to find publisher for title.");
			Assert.AreEqual("Binnet & Hardley", publisher.Name, "Wrong publisher for title.");

			// to-many
			Assert.IsNotNull(publisher.Titles, "Failed to find titles for publisher.");
			Assert.AreEqual(7, publisher.Titles.Count, "Wrong number of titles for publisher.");
		
			// get another title/publisher
			otherTitle = (Title)context.GetObjectFromTable("titles", new object[] { "BU1032" });
			Assert.IsNotNull(otherTitle, "Failed to find other title object.");
			otherPublisher = otherTitle.Publisher;
			Assert.IsTrue(otherPublisher != publisher, "Other title is by same publisher.");
		}


		[Test]
		public void ReadRelationshipWithNulls()
		{
			Discount	discount;
		
			discount = new DiscountFactory(context).FindUnique("DiscountType = {0}", "Initial Customer");
			Assert.IsNotNull(discount, "Initial customer discount not found.");
			Assert.IsNull(discount.Store, "No store should be associated with this discount.");
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
			Assert.IsTrue(otherTitle.Publisher == publisher, "Other title's publisher is not B&H now.");
			Assert.IsTrue(otherPublisher.Titles.Contains(otherTitle) == false, "Original publisher's title list still contains title.");
			Assert.IsTrue(publisher.Titles.Contains(otherTitle), "Publisher's title list does not contain new title");
			
			// to-many / set, i.e. use  publisher.addToTitles to change back and check
			otherPublisher.Titles.Add(otherTitle);
			Assert.IsTrue(otherTitle.Publisher == otherPublisher, "Other title's publisher is not original publisher");
			Assert.IsTrue(publisher.Titles.Contains(otherTitle) == false, "B&H title list still contains title");

			// add the title another time. this should duplivate the entry
			countBefore = otherPublisher.Titles.Count;
			otherPublisher.Titles.Add(otherTitle);
			countAfter = otherPublisher.Titles.Count;
			Assert.AreEqual(countBefore, countAfter, "Wrong number of titles after redundant add.");
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
			Assert.IsTrue(publisher.Titles.Contains(newTitle), "Publisher does not contain new title.");

			// new try adding with an "untouched" list in publisher
			publisher.Titles.InvalidateCache();
			newTitle = new TitleFactory(context).CreateObject("XX2222");
			newTitle.Publisher = publisher;
			Assert.IsTrue(publisher.Titles.Contains(newTitle), "Publisher does not contain new title.");
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
		public void ShouldAllowRecreationOfObjectWithFixedPkAfterCascadingDelete()
		{
			Author		author;

			author = new AuthorFactory(context).FindObject("486-29-1786");
			new TitleAuthorFactory(context).CreateObject(author, title);
			title.Delete();

			title = new TitleFactory(context).CreateObject("TC7777");
			new TitleAuthorFactory(context).CreateObject(author, title);
		}


		[Test]
		public void ShouldRemoveDeletedObjectsFromRelations()
		{
			Publisher	publisher;

			publisher = title.Publisher;
			publisher.Titles.Touch(); // make sure the list is loaded
			title.Delete();
			Assert.IsTrue(publisher.Titles.Contains(title) == false, "Deleted object did not disappear from rel.");
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
			Assert.IsNotNull(author, "Did not find author.");

			corr = new TitleAuthorFactory(context).CreateObject(author, title);
			Assert.IsTrue(author.TitleAuthors.Contains(corr), "Title not in Author.");
			Assert.IsTrue(title.TitleAuthors.Contains(corr), "Author not in Title.");
			corr.Delete();

			// Now try the same but with the list touched before
			title.TitleAuthors.Touch();
			corr = new TitleAuthorFactory(context).CreateObject(author, title);
			Assert.IsTrue(author.TitleAuthors.Contains(corr), "Title not in Author.");
			Assert.IsTrue(title.TitleAuthors.Contains(corr), "Author not in Title.");
			corr.Delete();
		}


		[Test]
		public void GetReferencesIncludingCascades()
		{
			Publisher referencedPublisher = title.Publisher;
			Publisher unreferencedPublisher = new PublisherFactory(context).FindObject("1622");
			Title referencedTitle = title;
			Title unreferencedTitle = new TitleFactory(context).FindObject("MC3026");

			Assert.IsTrue(referencedPublisher.GetReferences().Count > 0, "The publisher should be referenced");
			Assert.IsTrue(unreferencedPublisher.GetReferences().Count == 0, "The publisher should not be referenced");
			Assert.IsTrue(referencedTitle.GetReferences().Count > 0, "The title should be referenced");
			Assert.IsTrue(unreferencedTitle.GetReferences().Count == 0, "The title should not be referenced");
		}


		[Test]
		public void GetReferencesIgnoringCascades()
		{
			Publisher referencedPublisher = title.Publisher;
			Publisher unreferencedPublisher = new PublisherFactory(context).FindObject("1622");
			Title referencedTitle = title;
			Title unreferencedTitle = new TitleFactory(context).FindObject("MC3026");

			Assert.IsTrue(referencedPublisher.GetReferences(true).Count > 0, "The publisher should be referenced");
			Assert.IsTrue(unreferencedPublisher.GetReferences(true).Count == 0, "The publisher should not be referenced");
			Assert.IsTrue(referencedTitle.GetReferences(true).Count == 0, "The title should not be referenced");
			Assert.IsTrue(unreferencedTitle.GetReferences(true).Count == 0, "The title should not be referenced");
		}

	}
}
