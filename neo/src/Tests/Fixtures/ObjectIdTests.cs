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

		    Assert.IsTrue(oid1.Equals(null) == false, "oid1.Equals(null) must be false");
			Assert.IsTrue(oid1.Equals(this) == false, "oid1.Equals(this) must be false");

			Assert.IsTrue(oid1.Equals(oid2) == true, "oid1.Equals(oid2) must be true");
			Assert.IsTrue(oid1.GetHashCode() == oid2.GetHashCode(), "oid1.GetHashCode() must be equal to oid2.GetHashCode()");

			Assert.IsTrue(oid1.Equals(oid3) == false, "oid1.Equals(oid3) must be false");
			Assert.IsTrue(oid1.GetHashCode() != oid3.GetHashCode(), "oid1.GetHashCode() must not be equal to oid3.GetHashCode()");

			Assert.IsTrue(oid1.Equals(oid4) == false, "oid1.Equals(oid4) must be false");
			Assert.IsTrue(oid1.GetHashCode() != oid4.GetHashCode(), "oid1.GetHashCode() must not be equal to oid4.GetHashCode()");
		}


		[Test]
		public void EqualityWithCompoundKey()
		{
			ObjectId	oid1, oid2, oid3;

			oid1 = new ObjectId("titleauthor", new object[] { "486-29-1786", "TC7777" });
			oid2 = new ObjectId("titleauthor", new object[] { "486-29-1786", "TC7777" });
			oid3 = new ObjectId("titleauthor", new object[] { "672-71-3249", "TC7777" });

			Assert.IsTrue(oid1.Equals(null) == false, "oid1.Equals(null) must be false");
			Assert.IsTrue(oid1.Equals(this) == false, "oid1.Equals(this) must be false");

			Assert.IsTrue(oid1.Equals(oid2) == true, "oid1.Equals(oid2) must be true");
			Assert.IsTrue(oid1.GetHashCode() == oid2.GetHashCode(), "oid1.GetHashCode() must be equal to oid2.GetHashCode()");

			Assert.IsTrue(oid1.Equals(oid3) == false, "oid1.Equals(oid3) must be false");
			Assert.IsTrue(oid1.GetHashCode() != oid3.GetHashCode(), "oid1.GetHashCode() must not be equal to oid3.GetHashCode()");
		}


		[Test]
		public void HashtableUsage()
		{
		    Hashtable	table;
			ObjectId	oid1, oid2;

			table = new Hashtable();
			
			oid1 = new ObjectId("titleauthor", new object[] { "486-29-1786", "TC7777" });
			table[oid1] = "foo";
			Assert.IsTrue(table[oid1].Equals("foo"), "Not found in table.");

			oid1 = new ObjectId("titleauthor", new object[] { "672-71-3249", "TC7777" });
			table[oid1] = "foo";
			Assert.IsTrue(table[oid1].Equals("foo"), "Not found in table.");

			oid1 = new ObjectId("titleauthor", new object[] { "472-27-2349", "TC7777" });
			table[oid1] = "foo";
			Assert.IsTrue(table[oid1].Equals("foo"), "Not found in table.");

			oid1 = new ObjectId("titleauthor", new object[] { "267-41-2394", "TC7777" });
			table[oid1] = "foo";
			Assert.IsTrue(table[oid1].Equals("foo"), "Not found in table.");

			oid2 = new ObjectId("titleauthor", new object[] { "486-29-1786", "TC7777" });
			Assert.IsTrue(table[oid2].Equals("foo"), "Not found in table using new oid.");
		}


		[Test]
		public void ToStringMethod()
		{
			ObjectId	oid1;

			oid1 = new ObjectId("table1", new object[] { "foo", "bar" });
			Assert.AreEqual("{table1: foo, bar}", oid1.ToString(), "ToString() wrong.");
		}
	}
}
