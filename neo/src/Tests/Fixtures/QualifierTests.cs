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
		    Assert.IsNotNull(title, "Failed to find title object.");
			Assert.AreEqual(10, title.Royalty, "Wrong value for royalties");
		}


		[Test]
		public void PropertyQualifierSimpleMatch()
		{
		    PropertyQualifier	q;
			
			q = new PropertyQualifier("TitleId", new EqualsPredicate("TC7777"));
			Assert.IsTrue(q.EvaluateWithObject(title), "Should match (String).");

			q = new PropertyQualifier("TitleId", new EqualsPredicate("XX2222"));
			Assert.IsTrue(q.EvaluateWithObject(title) == false, "Should not match.");

			q = new PropertyQualifier("TitleId", new NotEqualPredicate("XX2222"));
			Assert.IsTrue(q.EvaluateWithObject(title), "Should match.");

			q = new PropertyQualifier("Royalty", new EqualsPredicate(10));
			Assert.IsTrue(q.EvaluateWithObject(title), "Should match (Integer).");

			q = new PropertyQualifier("Publisher", new EqualsPredicate(title.Publisher));
			Assert.IsTrue(q.EvaluateWithObject(title), "Should match (Object).");
		}


		[Test]
		public void PropertyQualifierSimpleComparions()
		{
			PropertyQualifier	q;
			
			q = new PropertyQualifier("Royalty", new LessThanPredicate(15));
			Assert.IsTrue(q.EvaluateWithObject(title), "10 is not less than 15.");

			q = new PropertyQualifier("Royalty", new LessThanPredicate(9));
			Assert.IsTrue(q.EvaluateWithObject(title) == false, "10 is less than 9.");

			q = new PropertyQualifier("Royalty", new GreaterThanPredicate(9));
			Assert.IsTrue(q.EvaluateWithObject(title), "10 is not greater than 9.");

			q = new PropertyQualifier("Royalty", new GreaterThanPredicate(15));
			Assert.IsTrue(q.EvaluateWithObject(title) == false, "10 is greater than 15.");
		}

		[Test]
		public void PropertyQualifierCaseInsensitiveComparisons()
		{
			PropertyQualifier	q;
			
			q = new PropertyQualifier("TheTitle", new CaseInsensitiveEqualsPredicate("sUsHi, aNyOnE?"));
			Assert.IsTrue(q.EvaluateWithObject(title), "Should match regardless of case.");

			Assert.IsNotNull(new TitleFactory(context).FindFirst(q));
		}

		[Test]
		public void ColumnQualifierSimpleMatch()
		{
			ColumnQualifier	q;
			
			q = new ColumnQualifier("pub_id", new EqualsPredicate(title.Publisher.PubId));
			Assert.IsTrue(q.EvaluateWithObject(title), "Should match.");
		}


		[Test]
		public void AndQualifier()
		{
			PropertyQualifier	q1, q2;
			ClauseQualifier		q;
			
			q1 = new PropertyQualifier("Royalty", new LessThanPredicate(15));
			q2 = new PropertyQualifier("Royalty", new GreaterThanPredicate(9));
			q = new AndQualifier(q1, q2);
			Assert.IsTrue(q.EvaluateWithObject(title), "10 not inbetween 9 and 15.");

			q1 = new PropertyQualifier("Royalty", new LessThanPredicate(15));
			q2 = new PropertyQualifier("Royalty", new GreaterThanPredicate(11));
			q = new AndQualifier(q1, q2);
			Assert.IsTrue(q.EvaluateWithObject(title) == false, "10 is inbetween 11 and 15.");
		}


		[Test]
		public void OrQualifier()
		{
			PropertyQualifier	q1, q2;
			ClauseQualifier		q;
			
			q1 = new PropertyQualifier("TitleId", new EqualsPredicate("TC7777"));
			q2 = new PropertyQualifier("TitleId", new EqualsPredicate("XX2222"));
			q = new OrQualifier(q1, q2);
			Assert.IsTrue(q.EvaluateWithObject(title), "Did not match either title id.");

			q1 = new PropertyQualifier("TitleId", new EqualsPredicate("XX1111"));
			q2 = new PropertyQualifier("TitleId", new EqualsPredicate("XX2222"));
			q = new OrQualifier(q1, q2);
			Assert.IsTrue(q.EvaluateWithObject(title) == false, "Did not match a title id.");
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

			Assert.IsTrue(q.EvaluateWithObject(title), "Did not match.");
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

			Assert.IsTrue(q.EvaluateWithObject(title), "Did not match.");
		}


		[Test]
		public void OneElementPathQualifiers()
		{
			Qualifier		q1;
			PathQualifier	q;

			q1 = new PropertyQualifier("Name", new EqualsPredicate(title.Publisher.Name));
			q = new PathQualifier("Publisher", q1);

			Assert.IsTrue(q.EvaluateWithObject(title), "Did not match.");
		}


		[Test]
		public void MultiElementPathQualifiers()
		{
			Qualifier		q1;
			PathQualifier	q;

			q1 = new PropertyQualifier("Name", new EqualsPredicate(title.Publisher.Name));
			q = new PathQualifier("Title.Publisher", q1);

			Assert.IsTrue(q.EvaluateWithObject(title.TitleAuthors[0]), "Did not match.");
		}


		[Test]
		public void ConstructionFromFormat()
		{
			Qualifier	q;

			q = Qualifier.Format("TitleId = 'TC7777' and Royalty < 15");
			Assert.IsNotNull(q, "Did not create a qualifier.");
			Assert.IsTrue(q.EvaluateWithObject(title), "Did not match.");
		}


		[Test]
		public void PropertyColumnConversion()
		{
			Publisher			pub;
			PropertyQualifier	propQualifier;
			ColumnQualifier		colQualifier;
			
			pub = title.Publisher;
			propQualifier = new PropertyQualifier("Publisher", new EqualsPredicate(pub));
			colQualifier = new QualifierConverter(title.Context.EntityMapFactory.GetMap(typeof(Title))).ConvertToColumnQualifier(propQualifier);

			Assert.AreEqual("pub_id", colQualifier.Column, "Wrong column.");
			Assert.AreEqual(typeof(EqualsPredicate), colQualifier.Predicate.GetType(), "Wrong operator.");
			Assert.AreEqual(pub.PubId, colQualifier.Predicate.Value, "Wrong value.");
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
			convertedQualifer = new QualifierConverter(title.Context.EntityMapFactory.GetMap(typeof(Title))).ConvertToColumnQualifiersRecursively(clauseQualifier);
			
			Assert.IsNotNull(convertedQualifer, "Should return a qualifier.");
			Assert.AreEqual(typeof(AndQualifier), convertedQualifer.GetType(), "Should have top-level AND qualifier.");
			andQualifier = convertedQualifer as AndQualifier;
			Assert.AreEqual(1, andQualifier.Qualifiers.Length, "Wrong number of qualifiers");
			colQualifier = (ColumnQualifier)andQualifier.Qualifiers[0];
			Assert.AreEqual("pub_id", colQualifier.Column, "Wrong column.");
			Assert.AreEqual(typeof(EqualsPredicate), colQualifier.Predicate.GetType(), "Wrong operator.");
			Assert.AreEqual(pub.PubId, colQualifier.Predicate.Value, "Wrong value.");
		}


		[Test]
		public void ConversionFromDictionaryCreatesNoQualifierForEmptyDictionary()
		{
			IDictionary	values;
			Qualifier	q;

			values = new Hashtable();
			q = Qualifier.FromPropertyDictionary(values);
			Assert.IsNull(q, "Should return no qualifier.");
		}


		[Test]
		public void ConversionFromDictionaryCreatesLikeWhenRequired()
		{
		    IDictionary			values;
			PropertyQualifier	q;

			values = new Hashtable();
			values.Add("foo", "bar%");
			q = Qualifier.FromPropertyDictionary(values) as PropertyQualifier;
			Assert.IsNotNull(q, "Should create PropertyQualifier.");
			Assert.IsTrue(q.Predicate is LikePredicate, "Should create like when required.");
		}
			

		[Test]
		public void ConversionFromDictionaryDoesNotCreateLikeWhenNotRequired()
		{
			IDictionary			values;
			PropertyQualifier	q;

			values = new Hashtable();
			values.Add("foo", "bar");
			q = Qualifier.FromPropertyDictionary(values) as PropertyQualifier;
			Assert.IsNotNull(q, "Should create PropertyQualifier.");
			Assert.IsTrue(q.Predicate is LikePredicate == false, "Should not create like when not required.");
		}

	}
}
