using System.Collections;
using Neo.Core.Util;
using NUnit.Framework;


namespace Neo.Tests.Fixtures
{
	[NUnit.Framework.TestFixture]
	public class ObjectIdTests
	{
		[NUnit.Framework.Test]
		public void Equality()
		{
		    ObjectId	oid1, oid2, oid3, oid4;

			oid1 = new ObjectId("table1", new object[] { "foo" });
			oid2 = new ObjectId("table1", new object[] { "foo" });
			oid3 = new ObjectId("table1", new object[] { "bar" });
			oid4 = new ObjectId("table2", new object[] { "foo" });

		    Assertion.Assert("oid1.Equals(null) must be false", oid1.Equals(null) == false);
			Assertion.Assert("oid1.Equals(this) must be false", oid1.Equals(this) == false);

			Assertion.Assert("oid1.Equals(oid2) must be true", oid1.Equals(oid2) == true);
			Assertion.Assert("oid1.GetHashCode() must be equal to oid2.GetHashCode()", oid1.GetHashCode() == oid2.GetHashCode());

			Assertion.Assert("oid1.Equals(oid3) must be false", oid1.Equals(oid3) == false);
			Assertion.Assert("oid1.GetHashCode() must not be equal to oid3.GetHashCode()", oid1.GetHashCode() != oid3.GetHashCode());

			Assertion.Assert("oid1.Equals(oid4) must be false", oid1.Equals(oid4) == false);
			Assertion.Assert("oid1.GetHashCode() must not be equal to oid4.GetHashCode()", oid1.GetHashCode() != oid4.GetHashCode());
		}


		[Test]
		public void EqualityWithCompoundKey()
		{
			ObjectId	oid1, oid2, oid3;

			oid1 = new ObjectId("titleauthor", new object[] { "486-29-1786", "TC7777" });
			oid2 = new ObjectId("titleauthor", new object[] { "486-29-1786", "TC7777" });
			oid3 = new ObjectId("titleauthor", new object[] { "672-71-3249", "TC7777" });

			Assertion.Assert("oid1.Equals(null) must be false", oid1.Equals(null) == false);
			Assertion.Assert("oid1.Equals(this) must be false", oid1.Equals(this) == false);

			Assertion.Assert("oid1.Equals(oid2) must be true", oid1.Equals(oid2) == true);
			Assertion.Assert("oid1.GetHashCode() must be equal to oid2.GetHashCode()", oid1.GetHashCode() == oid2.GetHashCode());

			Assertion.Assert("oid1.Equals(oid3) must be false", oid1.Equals(oid3) == false);
			Assertion.Assert("oid1.GetHashCode() must not be equal to oid3.GetHashCode()", oid1.GetHashCode() != oid3.GetHashCode());
		}


		[Test]
		public void HashtableUsage()
		{
		    Hashtable	table;
			ObjectId	oid1, oid2;

			table = new Hashtable();
			
			oid1 = new ObjectId("titleauthor", new object[] { "486-29-1786", "TC7777" });
			table[oid1] = "foo";
			Assertion.Assert("Not found in table.", table[oid1].Equals("foo"));

			oid1 = new ObjectId("titleauthor", new object[] { "672-71-3249", "TC7777" });
			table[oid1] = "foo";
			Assertion.Assert("Not found in table.", table[oid1].Equals("foo"));

			oid1 = new ObjectId("titleauthor", new object[] { "472-27-2349", "TC7777" });
			table[oid1] = "foo";
			Assertion.Assert("Not found in table.", table[oid1].Equals("foo"));

			oid1 = new ObjectId("titleauthor", new object[] { "267-41-2394", "TC7777" });
			table[oid1] = "foo";
			Assertion.Assert("Not found in table.", table[oid1].Equals("foo"));

			oid2 = new ObjectId("titleauthor", new object[] { "486-29-1786", "TC7777" });
			Assertion.Assert("Not found in table using new oid.", table[oid2].Equals("foo"));
		}


		[Test]
		public void ToStringMethod()
		{
			ObjectId	oid1;

			oid1 = new ObjectId("table1", new object[] { "foo", "bar" });
			Assertion.AssertEquals("ToString() wrong.", "{table1: foo, bar}", oid1.ToString());
		}
	}
}
