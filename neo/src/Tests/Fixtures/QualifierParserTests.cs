using System;
using Neo.Core;
using Neo.Core.Parser;
using Neo.Core.Qualifiers;
using NUnit.Framework;


namespace Neo.Tests.Fixtures
{

	[TestFixture]
	public class QualifierParserTests : TestBase
	{

		[Test]
		public void TokenizerSplitsPropPredValue()
		{	
			Token t;

			Tokenizer tokenizer = new Tokenizer("royalties=20");
			t = tokenizer.GetNextToken();
		    Assert.AreEqual(TokenType.String, t.Type);
			Assert.AreEqual("royalties", t.Value);
			t = tokenizer.GetNextToken();
			Assert.AreEqual(TokenType.Operator, t.Type);
			Assert.AreEqual(typeof(EqualsPredicate), t.Value);
			t = tokenizer.GetNextToken();
			Assert.AreEqual(TokenType.Constant, t.Type);
			Assert.AreEqual(20, t.Value);
			t = tokenizer.GetNextToken();
			Assert.AreEqual(null, t);		
		}
		
		
		[Test]
		public void TokenizerSplitsVariousTypesWithBlanks()
		{	
			Tokenizer tokenizer;
			Token	  t;
			
			tokenizer = new Tokenizer("name='foo' and royalties > {15} ");
			t = tokenizer.GetNextToken();
			Assert.AreEqual(TokenType.String, t.Type);
			Assert.AreEqual("name", t.Value);
			t = tokenizer.GetNextToken();
			Assert.AreEqual(TokenType.Operator, t.Type);
			Assert.AreEqual(typeof(EqualsPredicate), t.Value);
			t = tokenizer.GetNextToken();
			Assert.AreEqual(TokenType.Constant, t.Type);
			Assert.AreEqual("foo", t.Value);
			t = tokenizer.GetNextToken();
			Assert.AreEqual(TokenType.Conjunctor, t.Type);
			t = tokenizer.GetNextToken();
			Assert.AreEqual(TokenType.String, t.Type);
			Assert.AreEqual("royalties", t.Value);
			t = tokenizer.GetNextToken();
			Assert.AreEqual(TokenType.Operator, t.Type);
			Assert.AreEqual(typeof(GreaterThanPredicate), t.Value);
			t = tokenizer.GetNextToken();
			Assert.AreEqual(TokenType.ParamRef, t.Type);
			Assert.AreEqual(15, t.Value);
			t = tokenizer.GetNextToken();
			Assert.AreEqual(null, t);		
		}


		[Test]
		public void TokenizerSplitsUnrelatedTokens()
		{	
			Tokenizer tokenizer;
			Token	  t;

			tokenizer = new Tokenizer("or''{17}1");
			t = tokenizer.GetNextToken();
			Assert.AreEqual(TokenType.Conjunctor, t.Type);
			t = tokenizer.GetNextToken();
			Assert.AreEqual(TokenType.Constant, t.Type);
			Assert.AreEqual("", t.Value);
			t = tokenizer.GetNextToken();
			Assert.AreEqual(TokenType.ParamRef, t.Type);
			Assert.AreEqual(17, t.Value);
			t = tokenizer.GetNextToken();
			Assert.AreEqual(TokenType.Constant, t.Type);
			Assert.AreEqual(1, t.Value);
			t = tokenizer.GetNextToken();
			Assert.AreEqual(null, t);		
		}


		[Test]
		public void TokenizerRecognizesNamesWithUnderscores()
		{
			Tokenizer tokenizer;
			Token	  token;

			tokenizer = new Tokenizer("_foo_bar");
			token = tokenizer.GetNextToken();
			Assert.AreEqual(TokenType.String, token.Type);
			Assert.AreEqual(token.Value, "_foo_bar");
		}


		[Test]
		public void CreatesQualifierForLiteralStringComparison()
		{	
			QualifierParser parser = new QualifierParser("TitleId = 'TC7777'");
			Qualifier q = parser.GetQualifier();

			Assert.IsNotNull(q, "Parser failed.");
			Assert.AreEqual(typeof(PropertyQualifier), q.GetType(), "Wrong qualifier.");
			PropertyQualifier propertyQualifier = q as PropertyQualifier;
			Assert.AreEqual("TitleId", propertyQualifier.Property);
			Assert.AreEqual(typeof(EqualsPredicate), propertyQualifier.Predicate.GetType());
			Assert.AreEqual("TC7777", propertyQualifier.Predicate.Value);
		}


		[Test]
		public void CreatesQualifierForLiteralNumericComparison()
		{	
			QualifierParser parser = new QualifierParser("Royalties > 10");
			Qualifier q = parser.GetQualifier();

			Assert.IsNotNull(q, "Parser failed.");
			Assert.AreEqual(typeof(PropertyQualifier), q.GetType(), "Wrong qualifier.");
			PropertyQualifier propertyQualifier = q as PropertyQualifier;
			Assert.AreEqual("Royalties", propertyQualifier.Property);
			Assert.AreEqual(typeof(GreaterThanPredicate), propertyQualifier.Predicate.GetType());
			Assert.AreEqual(10, propertyQualifier.Predicate.Value);
		}


		[Test]
		public void CreatesQualiferWithTwoCharacterComparisonOperator()
		{	
			QualifierParser parser = new QualifierParser("Royalties >= 12");
			Qualifier q = parser.GetQualifier();

			Assert.IsNotNull(q, "Parser failed.");
			Assert.AreEqual(typeof(PropertyQualifier), q.GetType(), "Wrong qualifier.");
			PropertyQualifier cq = q as PropertyQualifier;
			Assert.AreEqual("Royalties", cq.Property);
			Assert.AreEqual(typeof(GreaterOrEqualPredicate), cq.Predicate.GetType());
			Assert.AreEqual(12, cq.Predicate.Value);
		}


		[Test]
		public void CreatesQualifierForNotEqualComparison()
		{
			QualifierParser parser = new QualifierParser("Royalties != 10");
			Qualifier q = parser.GetQualifier();

			Assert.IsNotNull(q, "Parser failed.");
			Assert.AreEqual(typeof(PropertyQualifier), q.GetType(), "Wrong qualifier.");
			PropertyQualifier propertyQualifier = q as PropertyQualifier;
			Assert.AreEqual("Royalties", propertyQualifier.Property);
			Assert.AreEqual(typeof(NotEqualPredicate), propertyQualifier.Predicate.GetType());
			Assert.AreEqual(10, propertyQualifier.Predicate.Value);
		}

		
		[Test]
		public void ReplacesTrueKeywordWithBooleanValue()
		{
			QualifierParser parser = new QualifierParser("IsEditable = true");
			Qualifier q = parser.GetQualifier();

			Assert.IsNotNull(q, "Parser failed.");
			Assert.AreEqual(typeof(PropertyQualifier), q.GetType(), "Wrong qualifier.");
			PropertyQualifier propertyQualifier = q as PropertyQualifier;
			Assert.AreEqual("IsEditable", propertyQualifier.Property);
			Assert.AreEqual(typeof(EqualsPredicate), propertyQualifier.Predicate.GetType());
			Assert.AreEqual(true, propertyQualifier.Predicate.Value);
		}


		[Test]
		public void ReplacesNullKeywordWithNullObject()
		{
			QualifierParser parser = new QualifierParser("Date = null");
			Qualifier q = parser.GetQualifier();

			Assert.IsNotNull(q, "Parser failed.");
			Assert.AreEqual(typeof(PropertyQualifier), q.GetType(), "Wrong qualifier.");
			PropertyQualifier propQualifier = q as PropertyQualifier;
			Assert.AreEqual(propQualifier.Predicate.Value, null);
		}

	
		[Test]
		public void CreatesQualifierWithLikePredicate()
		{
			QualifierParser parser = new QualifierParser("TheTitle like 'Sushi%'");
			Qualifier q = parser.GetQualifier();

			Assert.IsNotNull(q, "Parser failed.");
			Assert.AreEqual(typeof(PropertyQualifier), q.GetType(), "Wrong qualifier.");
			PropertyQualifier propertyQualifier = q as PropertyQualifier;
			Assert.AreEqual("TheTitle", propertyQualifier.Property);
			Assert.AreEqual(typeof(LikePredicate), propertyQualifier.Predicate.GetType());
			Assert.AreEqual("Sushi%", propertyQualifier.Predicate.Value);
		}


		[Test]
		public void CreatesPredicateWithValueFromParameterReference()
		{	
			QualifierParser parser = new QualifierParser("TitleId = {1}", "XX2222", "TC7777");
			Qualifier q = parser.GetQualifier();

			Assert.IsNotNull(q, "Parser failed.");
			Assert.AreEqual(typeof(PropertyQualifier), q.GetType(), "Wrong qualifier.");
			PropertyQualifier propertyQualifier = q as PropertyQualifier;
			Assert.AreEqual("TitleId", propertyQualifier.Property);
			Assert.AreEqual(typeof(EqualsPredicate), propertyQualifier.Predicate.GetType());
			Assert.AreEqual("TC7777", propertyQualifier.Predicate.Value);
		}


		[Test]
		public void CreatesPropertQualifierForAttributePredicateString()
		{
			QualifierParser parser = new QualifierParser("TitleId = {0}", "TC7777");
			Qualifier q = parser.GetQualifier();

			Assert.AreEqual(typeof(PropertyQualifier), q.GetType(), "Wrong class.");
		}


		[Test]
		public void CreatesClauseQualifierForTwoClauses()
		{	
			QualifierParser parser = new QualifierParser("TitleId = {0} and TitleId = {1}", "XX2222", "TC7777");
			Qualifier q = parser.GetQualifier();

			Assert.IsNotNull(q, "Parser failed.");
			Assert.AreEqual(typeof(AndQualifier), q.GetType(), "Wrong qualifier.");
			ClauseQualifier andQualifier = q as AndQualifier;
			Assert.AreEqual(2, andQualifier.Qualifiers.Length);
			Assert.AreEqual("TC7777", ((PropertyQualifier)andQualifier.Qualifiers[1]).Predicate.Value);
		}


		[Test]
		public void CreatesSingleClauseQualifierForThreeClausesOnOneLevel()
		{	
			QualifierParser parser = new QualifierParser("TitleId = 'TC7777' or Royalties < 7 or Royalties > 20");
			Qualifier q = parser.GetQualifier();

			Assert.IsNotNull(q, "Parser failed.");
			Assert.AreEqual(typeof(OrQualifier), q.GetType(), "Wrong qualifier.");
			ClauseQualifier orQualifier = q as ClauseQualifier;
			Assert.AreEqual(3, orQualifier.Qualifiers.Length);
			Assert.AreEqual(7, ((PropertyQualifier)orQualifier.Qualifiers[1]).Predicate.Value);
		}


		[Test]
		public void CreatesNestedClauseQualifiersForNestedClauses()
		{	
			QualifierParser parser = new QualifierParser("TitleId = 'TC7777' and (Royalties < 7 or Royalties > 20)");
			Qualifier q = parser.GetQualifier();

			Assert.IsNotNull(q, "Parser failed.");
			Assert.AreEqual(typeof(AndQualifier), q.GetType(), "Should have AND qualifier at root.");
			ClauseQualifier cq = q as ClauseQualifier;

			Assert.AreEqual(2, cq.Qualifiers.Length, "Root qualifier should have two sub qualifiers.");
			Assert.AreEqual(typeof(PropertyQualifier), cq.Qualifiers[0].GetType(), "Should preserve sequence.");
			Assert.AreEqual(typeof(OrQualifier), cq.Qualifiers[1].GetType(), "Should preserve sequence.");
		}


		[Test]
		public void JoinsBracketedClausesOfSameTypeFromLeft()
		{	
			// The parser will ignore the brackets in this case. This is intentional!
			QualifierParser parser = new QualifierParser("Royalties < 7 or (Royalties > 20 or Royalties = 10)");
			Qualifier q = parser.GetQualifier();

			Assert.IsNotNull(q, "Parser failed.");
			Assert.AreEqual(typeof(OrQualifier), q.GetType(), "Should have OR qualifier at root.");
			ClauseQualifier cq = q as ClauseQualifier;
			Assert.AreEqual(3, cq.Qualifiers.Length, "Root qualifier should have three sub qualifiers.");
		}


		[Test]
		public void CreatesEqualsPredicateWhenOnlyPropertyIsSpecified()
		{
			QualifierParser parser = new QualifierParser("TitleId", "TC7777");
			Qualifier q = parser.GetQualifier();

			Assert.IsNotNull(q, "Parser failed.");
			Assert.AreEqual(typeof(PropertyQualifier), q.GetType(), "Wrong qualifier.");
			PropertyQualifier pq = q as PropertyQualifier;
			Assert.AreEqual("TitleId", pq.Property);
			Assert.AreEqual(typeof(EqualsPredicate), pq.Predicate.GetType());
			Assert.AreEqual("TC7777", pq.Predicate.Value);
		}


		[Test]
		public void CreatesQualifierChainFromPath()
		{
			QualifierParser parser = new QualifierParser("Title.Publisher.PubId = '0899'");
			Qualifier q = parser.GetQualifier();

			Assert.IsNotNull(q, "Parser failed.");
			Assert.AreEqual(typeof(PathQualifier), q.GetType(), "Wrong qualifier.");
			PathQualifier pathQualifier = q as PathQualifier;
			Assert.AreEqual(pathQualifier.Path, "Title.Publisher");
			Assert.AreEqual(typeof(PropertyQualifier), pathQualifier.Qualifier.GetType(), "Wrong qualifier at end.");
			PropertyQualifier propQualifier = pathQualifier.Qualifier as PropertyQualifier;
			Assert.AreEqual("PubId", propQualifier.Property, "Wrong property.");
		}

		[Test]
		public void CreatesAndFromEmbeddedSetRestriction()
		{
			String format = "Title[TheTitle = 'Foo'].TitleAuthor.Author.Name='Bar'";

			// same as "Title.(TheTitle = 'Foo' AND TitleAuthor.Author.Name = 'Bar')"

			QualifierParser parser = new QualifierParser(format);
			Qualifier q = parser.GetQualifier();

			Assert.AreEqual(typeof(PathQualifier), q.GetType());
			PathQualifier pathq = (PathQualifier)q;
			Assert.AreEqual("Title", pathq.Path);

			Assert.AreEqual(typeof(AndQualifier), pathq.Qualifier.GetType());
			AndQualifier andq = (AndQualifier)pathq.Qualifier;
			Assert.AreEqual(2, andq.Qualifiers.Length);

			Assert.AreEqual(typeof(PropertyQualifier), andq.Qualifiers[0].GetType());
			PropertyQualifier propq = (PropertyQualifier)andq.Qualifiers[0];
			Assert.AreEqual("TheTitle", propq.Property);
			Assert.AreEqual("Foo" , propq.Predicate.Value);

			Assert.AreEqual(typeof(PathQualifier), andq.Qualifiers[1].GetType());
			pathq = (PathQualifier)andq.Qualifiers[1];
			Assert.AreEqual("TitleAuthor.Author", pathq.Path);
		}


		[Test]
		public void CombinesPathWithEmbeddedSetRestriction()
		{
			String format = "Publisher.Title[TheTitle = 'Foo'].TitleAuthor.Author.Name='Bar'";

			// same as "Publisher.Title.(TheTitle = 'Foo' AND TitleAuthor.Author.Name = 'Bar')"

			QualifierParser parser = new QualifierParser(format);
			Qualifier q = parser.GetQualifier();

			Assert.AreEqual(typeof(PathQualifier), q.GetType());
			PathQualifier pathq = (PathQualifier)q;
			Assert.AreEqual("Publisher.Title", pathq.Path);

			Assert.AreEqual(typeof(AndQualifier), pathq.Qualifier.GetType());
			AndQualifier andq = (AndQualifier)pathq.Qualifier;
			Assert.AreEqual(2, andq.Qualifiers.Length);
		}



	}
}
