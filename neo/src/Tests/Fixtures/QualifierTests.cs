using System;
using Neo.Core;
using Neo.Core.Util;
using NUnit.Framework;
using Pubs4.Model;


namespace Neo.Tests
{

	[TestFixture]
	public class QualifierTests : TestBase
	{
		protected ObjectContext	context;
		protected Title			title;

		[SetUp]
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
			
			q = new PropertyQualifier("TitleId", QualifierOperator.Equal, "TC7777");
			Assertion.Assert("Should match (String).", q.EvaluateWithObject(title));

			q = new PropertyQualifier("TitleId", QualifierOperator.Equal, "XX2222");
			Assertion.Assert("Should not match.", q.EvaluateWithObject(title) == false);

			q = new PropertyQualifier("TitleId", QualifierOperator.NotEqual, "XX2222");
			Assertion.Assert("Should match.", q.EvaluateWithObject(title));

			q = new PropertyQualifier("Royalty", QualifierOperator.Equal, 10);
			Assertion.Assert("Should match (Integer).", q.EvaluateWithObject(title));

			q = new PropertyQualifier("Publisher", QualifierOperator.Equal, title.Publisher);
			Assertion.Assert("Should match (Object).", q.EvaluateWithObject(title));
		}


		[Test]
		public void PropertyQualifierSimpleComparions()
		{
			PropertyQualifier	q;
			
			q = new PropertyQualifier("Royalty", QualifierOperator.LessThan, 15);
			Assertion.Assert("10 is not less than 15.", q.EvaluateWithObject(title));

			q = new PropertyQualifier("Royalty", QualifierOperator.LessThan, 9);
			Assertion.Assert("10 is less than 9.", q.EvaluateWithObject(title) == false);

			q = new PropertyQualifier("Royalty", QualifierOperator.GreaterThan, 9);
			Assertion.Assert("10 is not greater than 9.", q.EvaluateWithObject(title));

			q = new PropertyQualifier("Royalty", QualifierOperator.GreaterThan, 15);
			Assertion.Assert("10 is greater than 15.", q.EvaluateWithObject(title) == false);
		}


		[Test]
		public void ColumnQualifierSimpleMatch()
		{
			ColumnQualifier	q;
			
			q = new ColumnQualifier("pub_id", QualifierOperator.Equal, title.Publisher.PubId);
			Assertion.Assert("Should match.", q.EvaluateWithObject(title));
		}


		[Test]
		public void AndClauses()
		{
			PropertyQualifier	q1, q2;
			ClauseQualifier		q;
			
			q1 = new PropertyQualifier("Royalty", QualifierOperator.LessThan, 15);
			q2 = new PropertyQualifier("Royalty", QualifierOperator.GreaterThan, 9);
			q = new AndQualifier(q1, q2);
			Assertion.Assert("10 not inbetween 9 and 15.", q.EvaluateWithObject(title));

			q1 = new PropertyQualifier("Royalty", QualifierOperator.LessThan, 15);
			q2 = new PropertyQualifier("Royalty", QualifierOperator.GreaterThan, 11);
			q = new AndQualifier(q1, q2);
			Assertion.Assert("10 is inbetween 11 and 15.", q.EvaluateWithObject(title) == false);
		}


		[Test]
		public void OrClauses()
		{
			PropertyQualifier	q1, q2;
			ClauseQualifier		q;
			
			q1 = new PropertyQualifier("TitleId", QualifierOperator.Equal, "TC7777");
			q2 = new PropertyQualifier("TitleId", QualifierOperator.Equal, "XX2222");
			q = new OrQualifier(q1, q2);
			Assertion.Assert("Did not match either title id.", q.EvaluateWithObject(title));

			q1 = new PropertyQualifier("TitleId", QualifierOperator.Equal, "XX1111");
			q2 = new PropertyQualifier("TitleId", QualifierOperator.Equal, "XX2222");
			q = new OrQualifier(q1, q2);
			Assertion.Assert("Did not match a title id.", q.EvaluateWithObject(title) == false);
		}


		[Test]
		public void NestedClauses()
		{
			PropertyQualifier	q1, q2, qb;
			ClauseQualifier		q, qa;
			
			q1 = new PropertyQualifier("TitleId", QualifierOperator.Equal, "TC7777");
			q2 = new PropertyQualifier("TitleId", QualifierOperator.Equal, "XX2222");
			qa = new OrQualifier(q1, q2);
			qb = new PropertyQualifier("Royalty", QualifierOperator.LessThan, 15);
			q = new AndQualifier(qa, qb);

			Assertion.Assert("Did not match.", q.EvaluateWithObject(title));
		}


		[Test]
		public void AddingToClauses()
		{
			Qualifier		q1, q2;
			ClauseQualifier	q;
			
			q1 = new PropertyQualifier("TitleId", QualifierOperator.Equal, "XX2222");
			q = new OrQualifier(q1);
			q2 = new PropertyQualifier("TitleId", QualifierOperator.Equal, "TC7777");
			q.AddToQualifiers(q2);

			Assertion.Assert("Did not match.", q.EvaluateWithObject(title));
		}


		[Test]
		public void OneElementPathQualifiers()
		{
			Qualifier		q1;
			PathQualifier	q;

			q1 = new PropertyQualifier("Name", QualifierOperator.Equal, title.Publisher.Name);
			q = new PathQualifier("Publisher", q1);

			Assertion.Assert("Did not match.", q.EvaluateWithObject(title));
		}


		[Test]
		public void MultiElementPathQualifiers()
		{
			Qualifier		q1;
			PathQualifier	q;

			q1 = new PropertyQualifier("Name", QualifierOperator.Equal, title.Publisher.Name);
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
		public void ValueTypesAndNull()
		{
			Title		otherTitle;
			Qualifier	q;

			otherTitle = new TitleFactory(context).FindObject("MC3026");
			q = Qualifier.Format("Royalty < 15");
			Assertion.Assert("Null should not be less than anything.", q.EvaluateWithObject(otherTitle) == false);
			q = Qualifier.Format("Royalty > 15");
			Assertion.Assert("Null should not be greater than anything.", q.EvaluateWithObject(otherTitle) == false);
		}


		[Test]
		public void NumericConversions()
		{
			Qualifier	q;

			q = Qualifier.Format("Advance < 5000");
			Assertion.Assert("Matched when it shouldn't.", q.EvaluateWithObject(title) == false);
			q = Qualifier.Format("Advance > {0}", 5000);
			Assertion.Assert("Didn't match when it should've.", q.EvaluateWithObject(title));
		}


		[Test]
		public void PropertyColumnConversion()
		{
			Publisher			pub;
			PropertyQualifier	propQualifier;
			ColumnQualifier		colQualifier;
			
			pub = title.Publisher;
			propQualifier = new PropertyQualifier("Publisher", QualifierOperator.Equal, pub);
			colQualifier = new ColumnQualifier(propQualifier, title.Context.EntityMapFactory.GetMap(typeof(Title)));

			Assertion.AssertEquals("Wrong column.", "pub_id", colQualifier.Column);
			Assertion.AssertEquals("Wrong operator.", QualifierOperator.Equal, colQualifier.Operator);
			Assertion.AssertEquals("Wrong value.", pub.PubId, colQualifier.Value);
		}


		[Test]
		public void PropertyColumnConversionOnClause()
		{
			Publisher			pub;
			ClauseQualifier		clauseQualifier, convertedQualifer;
			PropertyQualifier	propQualifier;
			ColumnQualifier		colQualifier;
			
			pub = title.Publisher;
			propQualifier = new PropertyQualifier("Publisher", QualifierOperator.Equal, pub);
			clauseQualifier = new AndQualifier(propQualifier);
			convertedQualifer = clauseQualifier.GetWithColumnQualifiers(title.Context.EntityMapFactory.GetMap(typeof(Title)));
			
			Assertion.AssertEquals("Wrong number of qualifiers", 1, convertedQualifer.Qualifiers.Length);
			colQualifier = (ColumnQualifier)convertedQualifer.Qualifiers[0];
			Assertion.AssertEquals("Wrong column.", "pub_id", colQualifier.Column);
			Assertion.AssertEquals("Wrong operator.", QualifierOperator.Equal, colQualifier.Operator);
			Assertion.AssertEquals("Wrong value.", pub.PubId, colQualifier.Value);
		}



	}
}
