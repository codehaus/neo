using System;
using Neo.Core.Qualifiers;
using NUnit.Framework;


namespace Neo.Tests.Fixtures
{
	[TestFixture]
	public class PredicateTests : TestBase
	{
		[SetUp]
		public void SetupLogging()
		{
			SetupLog4Net();
		}
		

		// For historic reasons, some of the predicates are tested indirectly from QualifierTests

		[Test]
		public void RelationalPredicatesCanHandleNull()
		{
			IPredicate p = new LessThanPredicate(15);
		    Assert.IsTrue(p.IsTrueForValue(null, null) == false, "Null should not be less than anything.");
			p = new GreaterThanPredicate(15);
			Assert.IsTrue(p.IsTrueForValue(null, null) == false, "Null should not be greater than anything.");
		}


		[Test]
		public void RelationalPredicatesCanConvertNumbers()
		{
			IPredicate p = new LessThanPredicate(5000);
			Assert.IsTrue(p.IsTrueForValue(new Decimal(6000), null) == false, "Should convert and eval to false");
			p = new GreaterThanPredicate(5000);
			Assert.IsTrue(p.IsTrueForValue(new Decimal(6000), null), "Should convert and eval to true");
		}


		[Test]
		public void GreaterOrEqualPredicateImplementsGreaterOrEqualLogic()
		{
			IPredicate	p;

			p = new GreaterOrEqualPredicate(3500);
			Assert.IsTrue(p.IsTrueForValue(new Decimal(3499), null) == false);
			Assert.IsTrue(p.IsTrueForValue(new Decimal(3500), null));
			Assert.IsTrue(p.IsTrueForValue(new Decimal(3501), null));
		}


		[Test]
		public void LessOrEqualPredicateImplementsLessOrEqualLogic()
		{
			IPredicate	p;

			p = new LessOrEqualPredicate(1200);
			Assert.IsTrue(p.IsTrueForValue(new Decimal(1199), null));
			Assert.IsTrue(p.IsTrueForValue(new Decimal(1200), null));
			Assert.IsTrue(p.IsTrueForValue(new Decimal(1201), null) == false);
		}

	
		[Test]
		public void LikePredicateImplementsPercentAndQmark()
		{
			IPredicate	p;

			p = new LikePredicate("Sushi%");
			Assert.IsTrue(p.IsTrueForValue("Sushi, anyone?", null));
			p = new LikePredicate("sushi%");
			Assert.IsTrue(p.IsTrueForValue("Sushi, anyone?", null));
			p = new LikePredicate("sushi %");
			Assert.IsTrue(p.IsTrueForValue("Sushi, anyone?", null) == false);

			p = new LikePredicate("%one?");
			Assert.IsTrue(p.IsTrueForValue("Sushi, anyone?", null));
			p = new LikePredicate("%any%");
			Assert.IsTrue(p.IsTrueForValue("Sushi, anyone?", null));
			p = new LikePredicate("S%?");
			Assert.IsTrue(p.IsTrueForValue("Sushi, anyone?", null));
		}


		[Test]
		public void StartsWithPredicateMatchesBeginningOfStringCaseInsensitive() 
		{
			IPredicate	p;

			p = new StartsWithPredicate("Sushi");
			Assert.IsTrue(p.IsTrueForValue("Sushi, anyone?", null));
			p = new StartsWithPredicate("sushi");
			Assert.IsTrue(p.IsTrueForValue("Sushi, anyone?", null));
			p = new StartsWithPredicate("sushi ");
			Assert.IsTrue(p.IsTrueForValue("Sushi, anyone?", null) == false);
		}


		[Test]
		public void EndsWithPredicateMatchesEndOfStringCaseInsensitive() 
		{
			IPredicate	p;

			p = new EndsWithPredicate("one?");
			Assert.IsTrue(p.IsTrueForValue("Sushi, anyone?", null));
			p = new EndsWithPredicate("ONE?");
			Assert.IsTrue(p.IsTrueForValue("Sushi, anyone?", null));
			p = new EndsWithPredicate("ONE");
			Assert.IsTrue(p.IsTrueForValue("Sushi, anyone?", null) == false);
		}


		[Test]
		public void ContainsPredicateMatchesAnywhereInStringCaseInsensitive()
		{
			IPredicate	p;
			
			p = new ContainsPredicate("hi, Any");
			Assert.IsTrue(p.IsTrueForValue("Sushi, anyone?", null));
			p = new ContainsPredicate("hi, any");
			Assert.IsTrue(p.IsTrueForValue("Sushi, anyone?", null));
		}


	}
}
