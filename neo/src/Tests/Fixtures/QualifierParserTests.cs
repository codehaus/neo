using System;
using Neo.Core;
using Neo.Core.Parser;
using Neo.Core.Qualifiers;
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
			Token t;

			Tokenizer tokenizer = new Tokenizer("royalties=20");
			t = tokenizer.GetNextToken();
			Assertion.AssertEquals(TokenType.String, t.Type);
			Assertion.AssertEquals("royalties", t.Value);
			t = tokenizer.GetNextToken();
			Assertion.AssertEquals(TokenType.Operator, t.Type);
			Assertion.AssertEquals(typeof(EqualsPredicate), t.Value);
			t = tokenizer.GetNextToken();
			Assertion.AssertEquals(TokenType.Constant, t.Type);
			Assertion.AssertEquals(20, t.Value);
			t = tokenizer.GetNextToken();
			Assertion.AssertEquals(null, t);		
		}
		
		
		[Test]
		public void TestTokenizer2()
		{	
			Tokenizer tokenizer;
			Token	  t;
			
			tokenizer = new Tokenizer("name='foo' and royalties > {15} ");
			t = tokenizer.GetNextToken();
			Assertion.AssertEquals(TokenType.String, t.Type);
			Assertion.AssertEquals("name", t.Value);
			t = tokenizer.GetNextToken();
			Assertion.AssertEquals(TokenType.Operator, t.Type);
			Assertion.AssertEquals(typeof(EqualsPredicate), t.Value);
			t = tokenizer.GetNextToken();
			Assertion.AssertEquals(TokenType.Constant, t.Type);
			Assertion.AssertEquals("foo", t.Value);
			t = tokenizer.GetNextToken();
			Assertion.AssertEquals(TokenType.Conjunctor, t.Type);
			t = tokenizer.GetNextToken();
			Assertion.AssertEquals(TokenType.String, t.Type);
			Assertion.AssertEquals("royalties", t.Value);
			t = tokenizer.GetNextToken();
			Assertion.AssertEquals(TokenType.Operator, t.Type);
			Assertion.AssertEquals(typeof(GreaterThanPredicate), t.Value);
			t = tokenizer.GetNextToken();
			Assertion.AssertEquals(TokenType.ParamRef, t.Type);
			Assertion.AssertEquals(15, t.Value);
			t = tokenizer.GetNextToken();
			Assertion.AssertEquals(null, t);		
		}


		[Test]
		public void TestTokenizer3()
		{	
			Tokenizer tokenizer;
			Token	  t;

			tokenizer = new Tokenizer("or''{17}1");
			t = tokenizer.GetNextToken();
			Assertion.AssertEquals(TokenType.Conjunctor, t.Type);
			t = tokenizer.GetNextToken();
			Assertion.AssertEquals(TokenType.Constant, t.Type);
			Assertion.AssertEquals("", t.Value);
			t = tokenizer.GetNextToken();
			Assertion.AssertEquals(TokenType.ParamRef, t.Type);
			Assertion.AssertEquals(17, t.Value);
			t = tokenizer.GetNextToken();
			Assertion.AssertEquals(TokenType.Constant, t.Type);
			Assertion.AssertEquals(1, t.Value);
			t = tokenizer.GetNextToken();
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
			Assertion.AssertEquals("TitleId", cq.Property);
			Assertion.AssertEquals(typeof(EqualsPredicate), cq.Predicate.GetType());
			Assertion.AssertEquals("TC7777", cq.Predicate.Value);
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
			Assertion.AssertEquals("Royalties", cq.Property);
			Assertion.AssertEquals(typeof(GreaterThanPredicate), cq.Predicate.GetType());
			Assertion.AssertEquals(10, cq.Predicate.Value);
		}


		[Test]
		public void TestGreaterOrEqualComparison()
		{	
			QualifierParser		parser;
			Qualifier			q;
			PropertyQualifier	cq;

			parser = new QualifierParser("Royalties >= 12");
			q = parser.GetQualifier();

			Assertion.AssertNotNull("Parser failed.", q);
			Assertion.AssertEquals("Wrong qualifier.", typeof(PropertyQualifier), q.GetType());
			cq = q as PropertyQualifier;
			Assertion.AssertEquals("Royalties", cq.Property);
			Assertion.AssertEquals(typeof(GreaterOrEqualPredicate), cq.Predicate.GetType());
			Assertion.AssertEquals(12, cq.Predicate.Value);
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
			Assertion.AssertEquals("Royalties", cq.Property);
			Assertion.AssertEquals(typeof(NotEqualPredicate), cq.Predicate.GetType());
			Assertion.AssertEquals(10, cq.Predicate.Value);
		}

		
		[Test]
		public void TestBoolComparison()
		{
			QualifierParser		parser;
			Qualifier			q;
			PropertyQualifier	pq;

			parser = new QualifierParser("IsEditable = true");
			q = parser.GetQualifier();

			Assertion.AssertNotNull("Parser failed.", q);
			Assertion.AssertEquals("Wrong qualifier.", typeof(PropertyQualifier), q.GetType());
			pq = q as PropertyQualifier;
			Assertion.AssertEquals("IsEditable", pq.Property);
			Assertion.AssertEquals(typeof(EqualsPredicate), pq.Predicate.GetType());
			Assertion.AssertEquals(true, pq.Predicate.Value);
		}


		[Test]
		public void TestLikeComparison()
		{
			QualifierParser		parser;
			Qualifier			q;
			PropertyQualifier	pq;

			parser = new QualifierParser("TheTitle like 'Sushi%'");
			q = parser.GetQualifier();

			Assertion.AssertNotNull("Parser failed.", q);
			Assertion.AssertEquals("Wrong qualifier.", typeof(PropertyQualifier), q.GetType());
			pq = q as PropertyQualifier;
			Assertion.AssertEquals("TheTitle", pq.Property);
			Assertion.AssertEquals(typeof(LikePredicate), pq.Predicate.GetType());
			Assertion.AssertEquals("Sushi%", pq.Predicate.Value);
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
			Assertion.AssertEquals("TitleId", cq.Property);
			Assertion.AssertEquals(typeof(EqualsPredicate), cq.Predicate.GetType());
			Assertion.AssertEquals("TC7777", cq.Predicate.Value);
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
			Assertion.AssertEquals("TC7777", ((PropertyQualifier)aq.Qualifiers[1]).Predicate.Value);
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
			Assertion.AssertEquals(7, ((PropertyQualifier)oq.Qualifiers[1]).Predicate.Value);
		}


		[Test]
		public void TestBracketsForClauseGrouping()
		{	
			QualifierParser		parser;
			Qualifier			q;
			ClauseQualifier		cq;

			parser = new QualifierParser("TitleId = 'TC7777' and (Royalties < 7 or Royalties > 20)");
			q = parser.GetQualifier();

			Assertion.AssertNotNull("Parser failed.", q);
			Assertion.AssertEquals("Should have AND qualifier at root.", typeof(AndQualifier), q.GetType());
			cq = q as ClauseQualifier;

			Assertion.AssertEquals("Root qualifier should have two sub qualifiers.", 2, cq.Qualifiers.Length);
			Assertion.AssertEquals("Should preserve sequence.", typeof(PropertyQualifier), cq.Qualifiers[0].GetType());
			Assertion.AssertEquals("Should preserve sequence.", typeof(OrQualifier), cq.Qualifiers[1].GetType());
		}


		[Test]
		public void TestBracketsForClauseGroupingCanJoinAcrossBracketsFromLeft()
		{	
			QualifierParser		parser;
			Qualifier			q;
			ClauseQualifier		cq;

			// The parser will ignore the brackets in this case. This is intentional!
			parser = new QualifierParser("Royalties < 7 or (Royalties > 20 or Royalties = 10)");
			q = parser.GetQualifier();

			Assertion.AssertNotNull("Parser failed.", q);
			Assertion.AssertEquals("Should have OR qualifier at root.", typeof(OrQualifier), q.GetType());
			cq = q as ClauseQualifier;

			Assertion.AssertEquals("Root qualifier should have three sub qualifiers.", 3, cq.Qualifiers.Length);
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
			Assertion.AssertEquals("TitleId", cq.Property);
			Assertion.AssertEquals(typeof(EqualsPredicate), cq.Predicate.GetType());
			Assertion.AssertEquals("TC7777", cq.Predicate.Value);

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
