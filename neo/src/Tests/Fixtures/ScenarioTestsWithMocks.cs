using System.Data;
using Neo.Core;
using NUnit.Framework;
using Pubs4.Model;


namespace Neo.Tests.Fixtures
{
	[TestFixture]
	public class ScenarioTestsWithMocks : ScenarioTests
	{

		protected override ObjectContext GetContext()
		{
			ObjectContext parentContext;
			
			// We are setting up the context to use a parent context with on-demand loading
			// because if we set it up from a dataset or with copy-in-ctor all objects would
			// be created before we can do our type substitution below.
			parentContext = new ObjectContext(GetTestDataSet());
			context = new ObjectContext(parentContext, false);
			
			context.EntityMapFactory.GetMap(typeof(Title)).ConcreteObjectType = typeof(TitleMock);
			context.EntityMapFactory.GetMap(typeof(Publisher)).ConcreteObjectType = typeof(PublisherMock);

			return context;
		}


		[Test]
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
