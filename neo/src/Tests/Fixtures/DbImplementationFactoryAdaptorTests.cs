using System;
using System.Data;
using Foo;
using Neo.Database;
using NMock;
using NUnit.Framework;


namespace Neo.Tests.Fixtures
{
	[TestFixture]
	public class DbImplementationFactoryAdaptorTests : TestBase
	{
		[Test]
		public void testForwardsOpenRequests()
		{
			const String CONNSTRING = "connection string";

			IMock dbConnectionMock = new DynamicMock(typeof(IDbConnection));
			IMock implFactoryMock = new DynamicMock(typeof(IDbImplementationFactory));
			implFactoryMock.ExpectAndReturn("CreateConnection", (IDbConnection)dbConnectionMock.MockInstance, CONNSTRING);
			IDbConnectionFactory factory = new DbImplementationFactoryAdaptor((IDbImplementationFactory)implFactoryMock.MockInstance, CONNSTRING);

			IDbConnection connection = factory.CreateConnection();

			Assert.IsNotNull(connection, "Should return a connection.");
			Assert.AreEqual((IDbConnection)dbConnectionMock.MockInstance, connection, "Should return right connection.");
		}
	}
}
