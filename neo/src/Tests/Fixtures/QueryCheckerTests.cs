using Neo.Core;
using Neo.Core.Util;
using NMock;
using NUnit.Framework;
using Pubs4.Model;

namespace Neo.Tests.Fixtures
{
	[TestFixture]
	public class QueryCheckerTests : TestBase
	{


		[Test]
		[ExpectedException(typeof(InvalidQueryException))]
		public void ThrowsForUnqualifiedQueriesIfSoConfigured()
		{
			QueryChecker	checker;
			IEntityMap		entityMap;
			IMock			fetchSpecMock;

			entityMap = DefaultEntityMapFactory.SharedInstance.GetMap(typeof(Title));

			fetchSpecMock = new DynamicMock(typeof(IFetchSpecification));
			fetchSpecMock.SetupResult("EntityMap", entityMap);
			fetchSpecMock.ExpectAndReturn("Qualifier", null);
			fetchSpecMock.ExpectAndReturn("FetchLimit", -1);
			
			checker = new QueryChecker(null);
			checker.FetchRows((IFetchSpecification)fetchSpecMock.MockInstance);
		}


		[Test]
		public void DoesNotThrowForUnqualifiedQueriesIfTypeIsExempt()
		{
			QueryChecker	checker;
			IEntityMap		exemptEntityMap;
			IMock			fetchSpecMock;
			IMock			dataStoreMock;

			exemptEntityMap = DefaultEntityMapFactory.SharedInstance.GetMap(typeof(Title));

			fetchSpecMock = new DynamicMock(typeof(IFetchSpecification));
			fetchSpecMock.ExpectAndReturn("EntityMap", exemptEntityMap);
			fetchSpecMock.SetupResult("Qualifier", null);

			dataStoreMock = new DynamicMock(typeof(IDataStore));
			dataStoreMock.Expect("FetchRows", fetchSpecMock.MockInstance);

			checker = new QueryChecker((IDataStore)dataStoreMock.MockInstance);
			checker.TypeFilter = "Pubs4.Model.Title";
			checker.FetchRows((IFetchSpecification)fetchSpecMock.MockInstance);

			dataStoreMock.Verify();
		}
	}
}
