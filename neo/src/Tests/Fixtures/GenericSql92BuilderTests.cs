using System;
using System.Data;
using Neo.Core;
using Neo.Core.Qualifiers;
using Neo.Core.Util;
using Neo.Database;
using Neo.SqlClient;
using NMock;
using NUnit.Framework;
using Pubs4.Model;

namespace Neo.Tests.Fixtures
{
	// There are not many tests here (yet) as this class was split off during
	// a restructuring and most of its functionality is tested indirectly through
	// other classes.

	// We're somtimes using the SqlImplFactory which introduces a dependency on 
	// how it names its parameters but that's not what we are testing.

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
			IMock fetchSpecMock = new DynamicMock(typeof(IFetchSpecification));
			fetchSpecMock.SetupResult("EntityMap", null);
			fetchSpecMock.SetupResult("Qualifier", null);
			fetchSpecMock.SetupResult("SortOrderings", null);
			fetchSpecMock.SetupResult("FetchLimit", -1);

			GenericSql92Builder builder = new GenericSql92Builder(table, null);
			builder.WriteSelect((IFetchSpecification)fetchSpecMock.MockInstance);

			Assert.AreEqual("SELECT name, count FROM foo", builder.Command, "Should have created correct statement.");
		}


		[Test]
		public void CreatesSimpleSelectStatementWithDelimitedIdentifiers()
		{
			IMock fetchSpecMock = new DynamicMock(typeof(IFetchSpecification));
			fetchSpecMock.SetupResult("EntityMap", null);
			fetchSpecMock.SetupResult("Qualifier", null);
			fetchSpecMock.SetupResult("SortOrderings", null);
			fetchSpecMock.SetupResult("FetchLimit", -1);

			GenericSql92Builder builder = new GenericSql92Builder(table, null);
			builder.UsesDelimitedIdentifiers = true;
			builder.WriteSelect((IFetchSpecification)fetchSpecMock.MockInstance);

			Assert.AreEqual("SELECT \"name\", \"count\" FROM \"foo\"", builder.Command, "Should have created correct statement.");
		}


		[Test]
		public void CreatesStatementWithNotEqualOperator()
		{
			Qualifier q = new ColumnQualifier("count", new NotEqualPredicate(10));

			IMock fetchSpecMock = new DynamicMock(typeof(IFetchSpecification));
			fetchSpecMock.SetupResult("EntityMap", null);
			fetchSpecMock.SetupResult("Qualifier", q);
			fetchSpecMock.SetupResult("SortOrderings", null);
			fetchSpecMock.SetupResult("FetchLimit", -1);

			GenericSql92Builder builder = new GenericSql92Builder(table, new SqlImplFactory());
			builder.WriteSelect((IFetchSpecification)fetchSpecMock.MockInstance);
			Assert.AreEqual("SELECT name, count FROM foo WHERE count<>count0", builder.Command, "Should have created correct statement.");
		}


		[Test]
		public void CreatesOptimisticLockMatchWithLike()
		{
			table.Columns["name"].ExtendedProperties.Add("LockStrategy", "LIKE");
			DataRow row = table.NewRow();
			table.Rows.Add(row);
			row["count"] = 5;
			row["name"] = "foobar";
			row.AcceptChanges();
			row.Delete();

			GenericSql92Builder builder = new GenericSql92Builder(table, new SqlImplFactory());
			builder.WriteDelete(row);
			Assert.IsTrue(builder.Command.IndexOf("name LIKE name_ORIG") != -1, "Should have created correct statement.");
		}


		[Test]
		public void CreatesValidOptimisticLockMatchWhenFirstColumnIsIgnored()
		{
			table.Columns["name"].ExtendedProperties.Add("LockStrategy", "NONE");
			DataRow row = table.NewRow();
			row["count"] = 5;
			row["name"] = "foobar";
			table.Rows.Add(row);
			row.AcceptChanges();
			row.Delete();

			GenericSql92Builder builder = new GenericSql92Builder(table, new SqlImplFactory());
			builder.WriteDelete(row);
			Assert.IsTrue(builder.Command.IndexOf("WHERE ((count = count_ORIG") != -1, "Should have created correct statement.");
		}

		[Test]
		public void AbbreviatesSubselectsForJoinColumn()
		{
			// The dummy table is not sufficient, we need the full test model
			DataSet dataset = new DataSet("testds");
			IEntityMap taMap = DefaultEntityMapFactory.SharedInstance.GetMap(typeof(TitleAuthor));
			taMap.UpdateSchemaInDataSet(dataset, SchemaUpdate.Full);

			ColumnQualifier leafq = new ColumnQualifier("title_id", new EqualsPredicate(10));
			PathQualifier pathq = new PathQualifier("Title", leafq);

			IMock fetchSpecMock = new DynamicMock(typeof(IFetchSpecification));
			fetchSpecMock.SetupResult("EntityMap", taMap);
			fetchSpecMock.SetupResult("Qualifier", pathq);
			fetchSpecMock.SetupResult("SortOrderings", null);
			fetchSpecMock.SetupResult("FetchLimit", -1);

			GenericSql92Builder builder = new GenericSql92Builder(dataset.Tables["titleauthor"], new SqlImplFactory());
			builder.WriteSelect((IFetchSpecification)fetchSpecMock.MockInstance);
			Assert.AreEqual("SELECT au_id, title_id FROM titleauthor WHERE title_id=title_id0", builder.Command, "Should have created correct statement.");		
		}

		[Test]
		public void HandlesPathQualifiersWithoutQualiferAtEnd()
		{
			// PathQualifiers without qualifier at the end are not generally supported 
			// by other Neo classes. It is a bit of cheat to help DbDataStore create
			// fetch specs for spans.
			DataSet dataset = new DataSet("testds");
			IEntityMap pubMap = DefaultEntityMapFactory.SharedInstance.GetMap(typeof(Publisher));
			pubMap.UpdateSchemaInDataSet(dataset, SchemaUpdate.Full);

			PathQualifier pathq = new PathQualifier("Titles", null);

			IMock fetchSpecMock = new DynamicMock(typeof(IFetchSpecification));
			fetchSpecMock.SetupResult("EntityMap", pubMap);
			fetchSpecMock.SetupResult("Qualifier", pathq);
			fetchSpecMock.SetupResult("SortOrderings", null);
			fetchSpecMock.SetupResult("FetchLimit", -1);

			GenericSql92Builder builder = new GenericSql92Builder(dataset.Tables["publishers"], new SqlImplFactory());
			builder.WriteSelect((IFetchSpecification)fetchSpecMock.MockInstance);
			Assert.AreEqual("SELECT pub_id, pub_name, city, state, country FROM publishers WHERE pub_id IN ( SELECT DISTINCT pub_id FROM titles )", builder.Command, "Should have created correct statement.");
		
		}

		[Test]
		public void WritesNotNullForNotEqualWithNullValues()
		{
			ColumnQualifier q = new ColumnQualifier("name", new NotEqualPredicate(DBNull.Value));
			GenericSql92Builder builder = new GenericSql92Builder(table, new SqlImplFactory());
			
			builder.VisitColumnQualifier(q);

			Assert.AreEqual("name IS NOT NULL ", builder.Command);
		}

	}
}
