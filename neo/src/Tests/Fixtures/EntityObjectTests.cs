using System;
using log4net;
using Neo.Core;
using Neo.Framework;
using NUnit.Framework;
using Pubs4.Model;


namespace Neo.Tests.Fixtures
{
	// This fixture tests functionality that is unique to the concrete EntityObject 
	// implementation in the framework. Generic functionality tests should go into
	// the scenario tests fixture to ensure they are run against various context
	// configurations/implementations.

	[TestFixture]
	public class EntityObjectLoggingTests : TestBase
	{	
		ObjectContext	context;
		Title			title;
		
		[SetUp]
		public void SetUp()
		{
			SetupLog4Net();

			context = new ObjectContext(GetTestDataSet());
			title = new TitleFactory(context).FindObject("TC7777");
			Assertion.AssertNotNull("Failed to find title object.", title);
		}
		
			
		[Test]
		[ExpectedException(typeof(InvalidDbNullException))]
		public void ReadValueTypeAttributeWithNull()
		{
			title.Row["royalty"] = DBNull.Value;
			title.Royalty++;
		}


		[Test]
		public void ReadValueTypeAttributeWithNullHandler()
		{
			/* If this test fails, check that the Title class really implementes the required
			 * handler. (The class file might have been overwritten accidentally.) The class
			 * should contain the following method:
			 * 
			 * 	protected override object HandleNullValueForProperty(string propName)
			 *	{
			 *		if(propName == "YtdSales")
			 *			return 0;
			 *		return base.HandleNullValueForProperty(propName);
			 *	}
			 */
			title = new TitleFactory(context).FindObject("MC3026");
			Assert.AreEqual(title.YtdSales, 0, "Should have converted NULL to zero.");
		}


		[Test]
		public void IsSame()
		{
			ObjectContext	childContext;
			Title			titleInChild, otherTitleInChild;

			childContext = new ObjectContext(context);
			titleInChild = new TitleFactory(childContext).FindUnique("TitleId = 'TC7777'");

			Assertion.Assert("Not same.", titleInChild.IsSame(title));
			Assertion.Assert("Not same.", title.IsSame(titleInChild));

			otherTitleInChild = new TitleFactory(context).FindUnique("TitleId = 'BU1032'");
			Assertion.Assert("Same but shouldn't.", otherTitleInChild.IsSame(title) == false);
			Assertion.Assert("Same but shouldn't.", title.IsSame(otherTitleInChild) == false);
		}


		[Test]
		public void LoggerIsCached()
		{
			ILog		logger, logger2;

			logger = MyEntityObject.GetLogger(typeof(Title));
			Assertion.AssertNotNull("Did not get logger.", logger);

			logger2 = MyEntityObject.GetLogger(typeof(Title));
			Assertion.AssertEquals("Logger not cached.", logger, logger2);
		}


		[Test]
		public void LoggerPerClass()
		{
			ILog		logger, logger2;

			logger = MyEntityObject.GetLogger(typeof(Title));
			logger2 = MyEntityObject.GetLogger(typeof(Publisher));
			Assertion.Assert("Logger must be different for different classes.", logger.Equals(logger2) == false);
		}

		
		[Test]
		public void ToStringWorksOnDeletedObjects()
		{
			String	descriptionBeforeDelete;

			descriptionBeforeDelete = title.ToString();
			title.Delete();
			Assertion.AssertEquals("Should have same description.", descriptionBeforeDelete, title.ToString());
		}
	
	}


	#region Accessor class

	public class MyEntityObject : EntityObject
	{
		public static new ILog GetLogger(Type t)
		{
			return EntityObject.GetLogger(t);
		}

		public MyEntityObject() : base(null, null)
		{
		}

	}

	#endregion



}
