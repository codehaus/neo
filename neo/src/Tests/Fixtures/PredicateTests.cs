using System;
using Neo.Core;
using Neo.Core.Util;
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
			IPredicate	p;

			p = new LessThanPredicate(15);
			Assertion.Assert("Null should not be less than anything.", p.IsTrueForValue(null, null) == false);
			p = new GreaterThanPredicate(15);
			Assertion.Assert("Null should not be greater than anything.", p.IsTrueForValue(null, null) == false);
		}


		[Test]
		public void RelationalPredicatesCanConvertNumbers()
		{
			IPredicate	p;

			p = new LessThanPredicate(5000);
			Assertion.Assert("Should convert and eval to false", p.IsTrueForValue(new Decimal(6000), null) == false);
			p = new GreaterThanPredicate(5000);
			Assertion.Assert("Should convert and eval to true", p.IsTrueForValue(new Decimal(6000), null));
		}


		[Test]
		public void GreaterOrEqualPredicate()
		{
			IPredicate	p;

			p = new GreaterOrEqualPredicate(3500);
			Assertion.Assert("Should eval to false", p.IsTrueForValue(new Decimal(3499), null) == false);
			Assertion.Assert("Should eval to true",  p.IsTrueForValue(new Decimal(3500), null));
			Assertion.Assert("Should eval to true",  p.IsTrueForValue(new Decimal(3501), null));
		}


		[Test]
		public void LessOrEqualPredicate()
		{
			IPredicate	p;

			p = new LessOrEqualPredicate(1200);
			Assertion.Assert("Should eval to true", p.IsTrueForValue(new Decimal(1199), null));
			Assertion.Assert("Should eval to true",  p.IsTrueForValue(new Decimal(1200), null));
			Assertion.Assert("Should eval to false",  p.IsTrueForValue(new Decimal(1201), null) == false);
		}

	
		[Test]
		public void LikePredicate()
		{
			IPredicate	p;

			p = new LikePredicate("Sushi%");
			Assertion.Assert("Should eval to true (Sushi%)", p.IsTrueForValue("Sushi, anyone?", null));
			p = new LikePredicate("sushi%");
			Assertion.Assert("Should eval to true (sushi%)", p.IsTrueForValue("Sushi, anyone?", null));
			p = new LikePredicate("sushi %");
			Assertion.Assert("Should eval to true (sushi %)", p.IsTrueForValue("Sushi, anyone?", null) == false);

			p = new LikePredicate("%one?");
			Assertion.Assert("Should eval to true (%one?)", p.IsTrueForValue("Sushi, anyone?", null));
			p = new LikePredicate("%any%");
			Assertion.Assert("Should eval to true (%any%)", p.IsTrueForValue("Sushi, anyone?", null));
			p = new LikePredicate("S%?");
			Assertion.Assert("Should eval to true (S%?)", p.IsTrueForValue("Sushi, anyone?", null));
		}


#if WANTS_STRING_PREDICATES
		[Test]
		public void StartsWithPredicate() 
		{
			IPredicate	p;

			p = new StartsWithPredicate("Sushi");
			Assertion.Assert("Should match (StartsWith).", p.IsTrueForValue("Sushi, anyone?", null));
			p = new StartsWithPredicate("sushi");
			Assertion.Assert("Should match (StartsWith, different case).", p.IsTrueForValue("Sushi, anyone?", null));
			p = new StartsWithPredicate("sushi ");
			Assertion.Assert("Should not match (StartsWith, different case).", p.IsTrueForValue("Sushi, anyone?", null) == false);
		}


		[Test]
		public void EndsWithPredicate() 
		{
			IPredicate	p;

			p = new EndsWithPredicate("one?");
			Assertion.Assert("Should match (EndsWith).", p.IsTrueForValue("Sushi, anyone?", null));
			p = new EndsWithPredicate("ONE?");
			Assertion.Assert("Should match (EndsWith, different case).", p.IsTrueForValue("Sushi, anyone?", null));
			p = new EndsWithPredicate("ONE");
			Assertion.Assert("Should not match (EndsWith, different case).", p.IsTrueForValue("Sushi, anyone?", null) == false);
		}


		[Test]
		public void ContainsPredicate()
		{
			IPredicate	p;
			
			p = new ContainsPredicate("hi, Any");
			Assertion.Assert("Should match (Contains).", p.IsTrueForValue("Sushi, anyone?", null));
			p = new ContainsPredicate("hi, any");
			Assertion.Assert("Should match (Contains, different case).", p.IsTrueForValue("Sushi, anyone?", null));
		}
#endif

	}
}
