using System;
using Neo.Core;
using NUnit.Framework;
using Pubs4.Model;


namespace Neo.Tests
{
	[TestFixture]
	public class ScenarioTestsWithStore : ScenarioTests
	{

		protected override ObjectContext GetContext()
		{
			return new ObjectContext(GetDataStore());
		}

		
		protected override DateTime GetDate(int y, int m, int d)
		{
			return new DateTime(y, m, d);
		}


		// This should really be in the base class but we need to extend testdata.xml first!

		[Test]
		public void ReadRelationshipWithNulls()
		{
			Discount	discount;
		
			discount = new DiscountFactory(context).FindUnique("DiscountType = {0}", "Initial Customer");
			Assertion.AssertNotNull("Initial customer discount not found.", discount);
			Assertion.AssertNull("No store should be associated with this discount.", discount.Store);

		}

	}

}
