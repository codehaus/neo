using System;
using System.Data;
using Neo.Core;
using Neo.Core.Qualifiers;
using Neo.Database;
using Neo.SqlClient;
using NMock;
using NUnit.Framework;


namespace Neo.Tests.Fixtures
{
	// There are not many tests here (yet) as this class was split off during
	// a restructuring and most of its functionality is tested indirectly through
	// other classes.

	[TestFixture]
	public class GenericSql92BuilderTests : TestBase
	{
		protected DataTable	table;

		[SetUp]
		public void SetUpTable()
		{
			table = new DataTable("foo");
			table.Columns.Add("name", typeof(String));
			table.Columns.Add("count", typeof(Int32));
		}


		[Test]
		public void CreatesSimpleSelectStatement()
		{
			GenericSql92Builder	builder;
			IMock				fetchSpecMock;

			fetchSpecMock = new DynamicMock(typeof(IFetchSpecification));
			fetchSpecMock.SetupResult("EntityMap", null);
			fetchSpecMock.SetupResult("Qualifier", null);
			fetchSpecMock.SetupResult("SortOrderings", null);
			fetchSpecMock.SetupResult("FetchLimit", -1);

			builder = new GenericSql92Builder(table, null);
			builder.WriteSelect((IFetchSpecification)fetchSpecMock.MockInstance);

			Assertion.AssertEquals("Should have created correct statement.", "SELECT name, count FROM foo", builder.Command);
		}


		[Test]
		public void CreatesSimpleSelectStatementWithDelimitedIdentifiers()
		{
			GenericSql92Builder	builder;
			IMock				fetchSpecMock;

			fetchSpecMock = new DynamicMock(typeof(IFetchSpecification));
			fetchSpecMock.SetupResult("EntityMap", null);
			fetchSpecMock.SetupResult("Qualifier", null);
			fetchSpecMock.SetupResult("SortOrderings", null);
			fetchSpecMock.SetupResult("FetchLimit", -1);

			builder = new GenericSql92Builder(table, null);
			builder.UsesDelimitedIdentifiers = true;
			builder.WriteSelect((IFetchSpecification)fetchSpecMock.MockInstance);

			Assertion.AssertEquals("Should have created correct statement.", "SELECT \"name\", \"count\" FROM \"foo\"", builder.Command);
		}


		[Test]
		public void CreatesStatementWithNotEqualOperator()
		{
			GenericSql92Builder	builder;
			IMock				fetchSpecMock;
			Qualifier			q;

			q = new ColumnQualifier("count", new NotEqualPredicate(10));

			fetchSpecMock = new DynamicMock(typeof(IFetchSpecification));
			fetchSpecMock.SetupResult("EntityMap", null);
			fetchSpecMock.SetupResult("Qualifier", q);
			fetchSpecMock.SetupResult("SortOrderings", null);
			fetchSpecMock.SetupResult("FetchLimit", -1);

			// Using the SqlImplFactory introduces a dependency on how it names its 
			// parameters but that's not what we are testing.
			builder = new GenericSql92Builder(table, new SqlImplFactory());
			builder.WriteSelect((IFetchSpecification)fetchSpecMock.MockInstance);
			Assertion.AssertEquals("Should have created correct statement.", "SELECT name, count FROM foo WHERE count<>count0", builder.Command);
		}

	}
}