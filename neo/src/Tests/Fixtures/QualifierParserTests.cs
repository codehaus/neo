using System;
using Neo.Core;
using Neo.Core.Util;
using NUnit.Framework;
using Pubs4.Model;

namespace Neo.Tests
{

	[TestFixture]
	public class QualifierParserTests : TestBase
	{

		[Test]
		public void TestTokenizer1()
		{	
			QualifierParser.Token t;

			QualifierParser parser = new QualifierParser("royalties=20");
			t = parser.GetNextToken();
			Assertion.AssertEquals(QualifierParser.TokenType.String, t.Type);
			Assertion.AssertEquals("royalties", t.Value);
			t = parser.GetNextToken();
			Assertion.AssertEquals(QualifierParser.TokenType.Operator, t.Type);
			Assertion.AssertEquals(QualifierOperator.Equal, t.Value);
			t = parser.GetNextToken();
			Assertion.AssertEquals(QualifierParser.TokenType.Number, t.Type);
			Assertion.AssertEquals(20, t.Value);
			t = parser.GetNextToken();
			Assertion.AssertEquals(null, t);		
		}
		
		
		[Test]
		public void TestTokenizer2()
		{	
			QualifierParser.Token t;

			QualifierParser parser = new QualifierParser("name='foo' and royalties > {15} ");
			t = parser.GetNextToken();
			Assertion.AssertEquals(QualifierParser.TokenType.String, t.Type);
			Assertion.AssertEquals("name", t.Value);
			t = parser.GetNextToken();
			Assertion.AssertEquals(QualifierParser.TokenType.Operator, t.Type);
			Assertion.AssertEquals(QualifierOperator.Equal, t.Value);
			t = parser.GetNextToken();
			Assertion.AssertEquals(QualifierParser.TokenType.QuotedString, t.Type);
			Assertion.AssertEquals("foo", t.Value);
			t = parser.GetNextToken();
			Assertion.AssertEquals(QualifierParser.TokenType.Conjunctor, t.Type);
			t = parser.GetNextToken();
			Assertion.AssertEquals(QualifierParser.TokenType.String, t.Type);
			Assertion.AssertEquals("royalties", t.Value);
			t = parser.GetNextToken();
			Assertion.AssertEquals(QualifierParser.TokenType.Operator, t.Type);
			Assertion.AssertEquals(QualifierOperator.GreaterThan, t.Value);
			t = parser.GetNextToken();
			Assertion.AssertEquals(QualifierParser.TokenType.ParamRef, t.Type);
			Assertion.AssertEquals(15, t.Value);
			t = parser.GetNextToken();
			Assertion.AssertEquals(null, t);		
		}


		[Test]
		public void TestTokenizer3()
		{	
			QualifierParser.Token t;

			QualifierParser parser = new QualifierParser("or''{17}1");
			t = parser.GetNextToken();
			Assertion.AssertEquals(QualifierParser.TokenType.Conjunctor, t.Type);
			t = parser.GetNextToken();
			Assertion.AssertEquals(QualifierParser.TokenType.QuotedString, t.Type);
			Assertion.AssertEquals("", t.Value);
			t = parser.GetNextToken();
			Assertion.AssertEquals(QualifierParser.TokenType.ParamRef, t.Type);
			Assertion.AssertEquals(17, t.Value);
			t = parser.GetNextToken();
			Assertion.AssertEquals(QualifierParser.TokenType.Number, t.Type);
			Assertion.AssertEquals(1, t.Value);
			t = parser.GetNextToken();
			Assertion.AssertEquals(null, t);		
		}


		[Test]
		public void TestLiteralComparison()
		{	
			QualifierParser		parser;
			Qualifier			q;
			PropertyQualifier	cq;

			parser = new QualifierParser("TitleId = 'TC7777'");
			q = parser.GetQualifier();

			Assertion.AssertNotNull("Parser failed.", q);
			Assertion.AssertEquals("Wrong qualifier.", typeof(PropertyQualifier), q.GetType());
			cq = q as PropertyQualifier;
			Assertion.AssertEquals(cq.Property, "TitleId");
			Assertion.AssertEquals(cq.Operator, QualifierOperator.Equal);
			Assertion.AssertEquals(cq.Value, "TC7777");
		}


		[Test]
		public void TestNumComparison()
		{	
			QualifierParser		parser;
			Qualifier			q;
			PropertyQualifier	cq;

			parser = new QualifierParser("Royalties > 10");
			q = parser.GetQualifier();

			Assertion.AssertNotNull("Parser failed.", q);
			Assertion.AssertEquals("Wrong qualifier.", typeof(PropertyQualifier), q.GetType());
			cq = q as PropertyQualifier;
			Assertion.AssertEquals(cq.Property, "Royalties");
			Assertion.AssertEquals(cq.Operator, QualifierOperator.GreaterThan);
			Assertion.AssertEquals(cq.Value, 10);
		}


		[Test]
		public void TestNotEqualComparison()
		{
			QualifierParser		parser;
			Qualifier			q;
			PropertyQualifier	cq;

			parser = new QualifierParser("Royalties != 10");
			q = parser.GetQualifier();

			Assertion.AssertNotNull("Parser failed.", q);
			Assertion.AssertEquals("Wrong qualifier.", typeof(PropertyQualifier), q.GetType());
			cq = q as PropertyQualifier;
			Assertion.AssertEquals(cq.Property, "Royalties");
			Assertion.AssertEquals(cq.Operator, QualifierOperator.NotEqual);
			Assertion.AssertEquals(cq.Value, 10);
		}

		
		[Test]
		public void TestBoolComparison()
		{
			QualifierParser		parser;
			Qualifier			q;
			PropertyQualifier	cq;

			parser = new QualifierParser("IsEditable = true");
			q = parser.GetQualifier();

			Assertion.AssertNotNull("Parser failed.", q);
			Assertion.AssertEquals("Wrong qualifier.", typeof(PropertyQualifier), q.GetType());
			cq = q as PropertyQualifier;
			Assertion.AssertEquals(cq.Property, "IsEditable");
			Assertion.AssertEquals(cq.Operator, QualifierOperator.Equal);
			Assertion.AssertEquals(cq.Value, true);
		}


		[Test]
		public void TestParamComparison()
		{	
			QualifierParser		parser;
			Qualifier			q;
			PropertyQualifier	cq;

			parser = new QualifierParser("TitleId = {1}", "XX2222", "TC7777");
			q = parser.GetQualifier();

			Assertion.AssertNotNull("Parser failed.", q);
			Assertion.AssertEquals("Wrong qualifier.", typeof(PropertyQualifier), q.GetType());
			cq = q as PropertyQualifier;
			Assertion.AssertEquals(cq.Property, "TitleId");
			Assertion.AssertEquals(cq.Operator, QualifierOperator.Equal);
			Assertion.AssertEquals(cq.Value, "TC7777");
		}


		[Test]
		public void TestQualifierClassForComparisons()
		{
			QualifierParser		parser;
			Qualifier			q;

			parser = new QualifierParser("TitleId = {0}", "TC7777");
			q = parser.GetQualifier();

			Assertion.AssertEquals("Wrong class.", typeof(PropertyQualifier), q.GetType());
		}


		[Test]
		public void TestTwoWayClause()
		{	
			QualifierParser		parser;
			Qualifier			q;
			ClauseQualifier		aq;

			parser = new QualifierParser("TitleId = {0} and TitleId = {1}", "XX2222", "TC7777");
			q = parser.GetQualifier();

			Assertion.AssertNotNull("Parser failed.", q);
			Assertion.AssertEquals("Wrong qualifier.", typeof(AndQualifier), q.GetType());
			aq = q as AndQualifier;
			Assertion.AssertEquals(2, aq.Qualifiers.Length);
			Assertion.AssertEquals("TC7777", ((PropertyQualifier)aq.Qualifiers[1]).Value);
		}


		[Test]
		public void TestThreeWayClause()
		{	
			QualifierParser		parser;
			Qualifier			q;
			ClauseQualifier		oq;

			parser = new QualifierParser("TitleId = 'TC7777' or Royalties < 7 or Royalties > 20");
			q = parser.GetQualifier();

			Assertion.AssertNotNull("Parser failed.", q);
			Assertion.AssertEquals("Wrong qualifier.", typeof(OrQualifier), q.GetType());
			oq = q as ClauseQualifier;
			Assertion.AssertEquals(3, oq.Qualifiers.Length);
			Assertion.AssertEquals(7, ((PropertyQualifier)oq.Qualifiers[1]).Value);
		}

		
		[Test]
		public void TestEqualShortcut()
		{
			QualifierParser		parser;
			Qualifier			q;
			PropertyQualifier	cq;

			parser = new QualifierParser("TitleId", "TC7777");
			q = parser.GetQualifier();

			Assertion.AssertNotNull("Parser failed.", q);
			Assertion.AssertEquals("Wrong qualifier.", typeof(PropertyQualifier), q.GetType());
			cq = q as PropertyQualifier;
			Assertion.AssertEquals(cq.Property, "TitleId");
			Assertion.AssertEquals(cq.Operator, QualifierOperator.Equal);
			Assertion.AssertEquals(cq.Value, "TC7777");

		}


		[Test]
		public void TestPath()
		{
			QualifierParser		parser;
			Qualifier			q;
			PathQualifier		pathQualifier;
			PropertyQualifier	propQualifier;

			parser = new QualifierParser("Title.Publisher.PubId = '0899'");
			q = parser.GetQualifier();

			Assertion.AssertNotNull("Parser failed.", q);
			Assertion.AssertEquals("Wrong qualifier.", typeof(PathQualifier), q.GetType());
			pathQualifier = q as PathQualifier;
			Assertion.AssertEquals(pathQualifier.Path, "Title.Publisher");
			Assertion.AssertEquals("Wrong qualifier at end.", typeof(PropertyQualifier), pathQualifier.Qualifier.GetType());
			propQualifier = pathQualifier.Qualifier as PropertyQualifier;
			Assertion.AssertEquals("Wrong property.", "PubId", propQualifier.Property);
		}

	
	}
}
