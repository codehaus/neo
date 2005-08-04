using Neo.Core;
using NUnit.Framework;


namespace Neo.Tests.Fixtures
{
	[TestFixture]
	public class FetchSpecificationTests : TestBase
	{
		[Test]
		public void SetsUpSortOrderingUsingConvenienceMethod()
		{
			FetchSpecification spec = new FetchSpecification();
			spec.AddSortOrdering("FirstName", SortDirection.AscendingCaseInsensitive);

			Assert.AreEqual(1, spec.SortOrderings.Length);
			Assert.AreEqual("FirstName", spec.SortOrderings[0].Property);
			Assert.AreEqual(SortDirection.AscendingCaseInsensitive, spec.SortOrderings[0].SortDirection);
		}

		[Test]
		public void AddsSortOrderingUsingConvenienceMethod()
		{
			FetchSpecification spec = new FetchSpecification();
			PropertyComparer comparer = new PropertyComparer("LastName", SortDirection.AscendingCaseInsensitive);
			spec.SortOrderings = new PropertyComparer[]{ comparer };
			spec.AddSortOrdering("FirstName", SortDirection.AscendingCaseInsensitive);

			Assert.AreEqual(2, spec.SortOrderings.Length);
			Assert.AreEqual("FirstName", spec.SortOrderings[1].Property);
		}

		[Test]
		public void SetsUpSpanUsingConvenienceMethod()
		{
			FetchSpecification spec = new FetchSpecification();
			spec.AddSpan("TitleAuthor");

			Assert.AreEqual(1, spec.Spans.Length);
			Assert.AreEqual("TitleAuthor", spec.Spans[0]);
		}

		[Test]
		public void SetsUpMultipleSpansUsingConvenienceMethod()
		{
			FetchSpecification spec = new FetchSpecification();
			spec.AddSpans("TitleAuthor", "Publisher");

			Assert.AreEqual(2, spec.Spans.Length);
			Assert.AreEqual("TitleAuthor", spec.Spans[0]);
			Assert.AreEqual("Publisher", spec.Spans[1]);
		}

		[Test]
		public void AddsMultipleSpansUsingConvenienceMethod()
		{
			FetchSpecification spec = new FetchSpecification();
			spec.Spans = new string[] { "TitleAuthor" };
			spec.AddSpans("TitleAuthor.Author", "Publisher");

			Assert.AreEqual(3, spec.Spans.Length);
			Assert.AreEqual("TitleAuthor", spec.Spans[0]);
			Assert.AreEqual("TitleAuthor.Author", spec.Spans[1]);
			Assert.AreEqual("Publisher", spec.Spans[2]);
		}

	}
}
