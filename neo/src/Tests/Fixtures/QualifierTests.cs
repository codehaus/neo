using System.Collections;
using Neo.Core;
using Neo.Core.Qualifiers;
using Neo.Core.Util;
using NUnit.Framework;
using Pubs4.Model;


namespace Neo.Tests.Fixtures
{

	[NUnit.Framework.TestFixture]
	public class QualifierTests : TestBase
	{
		protected ObjectContext	context;
		protected Title			title;

		[NUnit.Framework.SetUp]
		public void LoadDataSetAndGetTitleObject()
		{
			SetupLog4Net();
			
			context = new ObjectContext();
			context.MergeData(GetTestDataSet());

			title = new TitleFactory(context).FindObject("TC7777");
		    Assertion.AssertNotNull("Failed to find title object.", title);
			Assertion.AssertEquals("Wrong value for royalties", 10, title.Royalty);
		}


		[Test]
		public void PropertyQualifierSimpleMatch()
		{
		    PropertyQualifier	q;
			
			q = new PropertyQualifier("TitleId", new EqualsPredicate("TC7777"));
			Assertion.Assert("Should match (String).", q.EvaluateWithObject(title));

			q = new PropertyQualifier("TitleId", new EqualsPredicate("XX2222"));
			Assertion.Assert("Should not match.", q.EvaluateWithObject(title) == false);

			q = new PropertyQualifier("TitleId", new NotEqualPredicate("XX2222"));
			Assertion.Assert("Should match.", q.EvaluateWithObject(title));

			q = new PropertyQualifier("Royalty", new EqualsPredicate(10));
			Assertion.Assert("Should match (Integer).", q.EvaluateWithObject(title));

			q = new PropertyQualifier("Publisher", new EqualsPredicate(title.Publisher));
			Assertion.Assert("Should match (Object).", q.EvaluateWithObject(title));
		}


		[Test]
		public void PropertyQualifierSimpleComparions()
		{
			PropertyQualifier	q;
			
			q = new PropertyQualifier("Royalty", new LessThanPredicate(15));
			Assertion.Assert("10 is not less than 15.", q.EvaluateWithObject(title));

			q = new PropertyQualifier("Royalty", new LessThanPredicate(9));
			Assertion.Assert("10 is less than 9.", q.EvaluateWithObject(title) == false);

			q = new PropertyQualifier("Royalty", new GreaterThanPredicate(9));
			Assertion.Assert("10 is not greater than 9.", q.EvaluateWithObject(title));

			q = new PropertyQualifier("Royalty", new GreaterThanPredicate(15));
			Assertion.Assert("10 is greater than 15.", q.EvaluateWithObject(title) == false);
		}

		[Test]
		public void PropertyQualifierCaseInsensitiveComparisons()
		{
			PropertyQualifier	q;
			
			q = new PropertyQualifier("TheTitle", new CaseInsensitiveEqualsPredicate("sUsHi, aNyOnE?"));
			Assertion.Assert("Should match regardless of case.", q.EvaluateWithObject(title));

			Assert.IsNotNull(new TitleFactory(context).FindFirst(q));
		}

		[Test]
		public void ColumnQualifierSimpleMatch()
		{
			ColumnQualifier	q;
			
			q = new ColumnQualifier("pub_id", new EqualsPredicate(title.Publisher.PubId));
			Assertion.Assert("Should match.", q.EvaluateWithObject(title));
		}


		[Test]
		public void AndQualifier()
		{
			PropertyQualifier	q1, q2;
			ClauseQualifier		q;
			
			q1 = new PropertyQualifier("Royalty", new LessThanPredicate(15));
			q2 = new PropertyQualifier("Royalty", new GreaterThanPredicate(9));
			q = new AndQualifier(q1, q2);
			Assertion.Assert("10 not inbetween 9 and 15.", q.EvaluateWithObject(title));

			q1 = new PropertyQualifier("Royalty", new LessThanPredicate(15));
			q2 = new PropertyQualifier("Royalty", new GreaterThanPredicate(11));
			q = new AndQualifier(q1, q2);
			Assertion.Assert("10 is inbetween 11 and 15.", q.EvaluateWithObject(title) == false);
		}


		[Test]
		public void OrQualifier()
		{
			PropertyQualifier	q1, q2;
			ClauseQualifier		q;
			
			q1 = new PropertyQualifier("TitleId", new EqualsPredicate("TC7777"));
			q2 = new PropertyQualifier("TitleId", new EqualsPredicate("XX2222"));
			q = new OrQualifier(q1, q2);
			Assertion.Assert("Did not match either title id.", q.EvaluateWithObject(title));

			q1 = new PropertyQualifier("TitleId", new EqualsPredicate("XX1111"));
			q2 = new PropertyQualifier("TitleId", new EqualsPredicate("XX2222"));
			q = new OrQualifier(q1, q2);
			Assertion.Assert("Did not match a title id.", q.EvaluateWithObject(title) == false);
		}


		[Test]
		public void NestedClauses()
		{
			PropertyQualifier	q1, q2, qb;
			ClauseQualifier		q, qa;
			
			q1 = new PropertyQualifier("TitleId", new EqualsPredicate("TC7777"));
			q2 = new PropertyQualifier("TitleId", new EqualsPredicate("XX2222"));
			qa = new OrQualifier(q1, q2);
			qb = new PropertyQualifier("Royalty", new LessThanPredicate(15));
			q = new AndQualifier(qa, qb);

			Assertion.Assert("Did not match.", q.EvaluateWithObject(title));
		}


		[Test]
		public void AddingToClauses()
		{
			Qualifier		q1, q2;
			ClauseQualifier	q;
			
			q1 = new PropertyQualifier("TitleId", new EqualsPredicate("XX2222"));
			q = new OrQualifier(q1);
			q2 = new PropertyQualifier("TitleId", new EqualsPredicate("TC7777"));
			q.AddToQualifiers(q2);

			Assertion.Assert("Did not match.", q.EvaluateWithObject(title));
		}


		[Test]
		public void OneElementPathQualifiers()
		{
			Qualifier		q1;
			PathQualifier	q;

			q1 = new PropertyQualifier("Name", new EqualsPredicate(title.Publisher.Name));
			q = new PathQualifier("Publisher", q1);

			Assertion.Assert("Did not match.", q.EvaluateWithObject(title));
		}


		[Test]
		public void MultiElementPathQualifiers()
		{
			Qualifier		q1;
			PathQualifier	q;

			q1 = new PropertyQualifier("Name", new EqualsPredicate(title.Publisher.Name));
			q = new PathQualifier("Title.Publisher", q1);

			Assertion.Assert("Did not match.", q.EvaluateWithObject(title.TitleAuthors[0]));
		}


		[Test]
		public void ConstructionFromFormat()
		{
			Qualifier	q;

			q = Qualifier.Format("TitleId = 'TC7777' and Royalty < 15");
			Assertion.AssertNotNull("Did not create a qualifier.", q);
			Assertion.Assert("Did not match.", q.EvaluateWithObject(title));
		}


		[Test]
		public void PropertyColumnConversion()
		{
			Publisher			pub;
			PropertyQualifier	propQualifier;
			ColumnQualifier		colQualifier;
			
			pub = title.Publisher;
			propQualifier = new PropertyQualifier("Publisher", new EqualsPredicate(pub));
			colQualifier = new QualifierConverter(title.Context.EntityMapFactory.GetMap(typeof(Title))).ConvertPropertyQualifier(propQualifier);

			Assertion.AssertEquals("Wrong column.", "pub_id", colQualifier.Column);
			Assertion.AssertEquals("Wrong operator.", typeof(EqualsPredicate), colQualifier.Predicate.GetType());
			Assertion.AssertEquals("Wrong value.", pub.PubId, colQualifier.Predicate.Value);
		}


		[Test]
		public void PropertyColumnConversionOnClause()
		{
			Publisher			pub;
			ClauseQualifier		clauseQualifier;
			Qualifier			convertedQualifer;
			AndQualifier		andQualifier;
			PropertyQualifier	propQualifier;
			ColumnQualifier		colQualifier;
			
			pub = title.Publisher;
			propQualifier = new PropertyQualifier("Publisher", new EqualsPredicate(pub));
			clauseQualifier = new AndQualifier(propQualifier);
			convertedQualifer = new QualifierConverter(title.Context.EntityMapFactory.GetMap(typeof(Title))).ConvertPropertyQualifiers(clauseQualifier);
			
			Assertion.AssertNotNull("Should return a qualifier.", convertedQualifer);
			Assertion.AssertEquals("Should have top-level AND qualifier.", typeof(AndQualifier), convertedQualifer.GetType());
			andQualifier = convertedQualifer as AndQualifier;
			Assertion.AssertEquals("Wrong number of qualifiers", 1, andQualifier.Qualifiers.Length);
			colQualifier = (ColumnQualifier)andQualifier.Qualifiers[0];
			Assertion.AssertEquals("Wrong column.", "pub_id", colQualifier.Column);
			Assertion.AssertEquals("Wrong operator.", typeof(EqualsPredicate), colQualifier.Predicate.GetType());
			Assertion.AssertEquals("Wrong value.", pub.PubId, colQualifier.Predicate.Value);
		}


		[Test]
		public void ConversionFromDictionaryCreatesLikeWhenRequired()
		{
		    IDictionary			values;
			PropertyQualifier	q;

			values = new Hashtable();
			values.Add("foo", "bar%");
			q = Qualifier.FromPropertyDictionary(values) as PropertyQualifier;
			Assertion.AssertNotNull("Should create PropertyQualifier.", q);
			Assertion.Assert("Should create like when required.", q.Predicate is LikePredicate);
		}
			

		[Test]
		public void ConversionFromDictionaryDoesNotCreateLikeWhenNotRequired()
		{
			IDictionary			values;
			PropertyQualifier	q;

			values = new Hashtable();
			values.Add("foo", "bar");
			q = Qualifier.FromPropertyDictionary(values) as PropertyQualifier;
			Assertion.AssertNotNull("Should create PropertyQualifier.", q);
			Assertion.Assert("Should not create like when not required.", q.Predicate is LikePredicate == false);
		}

	}
}
