using System.Data;
using Neo.Core;
using NUnit.Framework;
using Pubs4.Model;


namespace Neo.Tests.Fixtures
{
	[NUnit.Framework.TestFixture]
	public class ScenarioTestsWithMocks : ScenarioTestsWithStore
	{

		protected override ObjectContext GetContext()
		{
			ObjectContext context;
			
			context = new ObjectContext(GetDataStore());
			context.EntityMapFactory.GetMap(typeof(Title)).ConcreteObjectType = typeof(TitleMock);
			context.EntityMapFactory.GetMap(typeof(Publisher)).ConcreteObjectType = typeof(PublisherMock);

			return context;
		}


		[NUnit.Framework.Test]
		public void ObjectsAreMocks()
		{
		    Assertion.AssertEquals("Object should be of mock type.", typeof(TitleMock), title.GetType());
			Assertion.AssertEquals("Object should be of mock type.", typeof(PublisherMock), title.Publisher.GetType());
		}

		
		[Test]
		public void LocalObjectsAlsoCreatesMocks()
		{
			ObjectContext childContext;
			Title		  titleInChildContext;

			childContext = new ObjectContext(context);
			titleInChildContext = (Title)childContext.GetLocalObject(title);

			Assertion.AssertEquals("Object should be of mock type.", typeof(TitleMock), titleInChildContext.GetType());
		}

	}


	#region Helper classes

	// Not really Mocks but serve to demonstrate that one can instantiate different
	// classes. It is actually possible to interface/impl separate the entity objects 
	// if one wishes to do so.
	
	public class TitleMock : Title
	{
		protected TitleMock(DataRow aRow, ObjectContext aContext) : base(aRow, aContext)
		{
		}

	}

	public class PublisherMock : Publisher
	{
		protected PublisherMock(DataRow aRow, ObjectContext aContext) : base(aRow, aContext)
		{
		}

	}

	#endregion

}
