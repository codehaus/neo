using System;
using System.Data;
using NUnit.Framework;
using Neo.Core;
using Pubs4.Model;


namespace Neo.Tests
{
	[TestFixture]
	public class ObjectFactoryTests : TestBase
	{
		protected ObjectContext context;


		[SetUp]
		public void SetUp()
		{
			SetupLog4Net();
			
			context = new ObjectContext();
			context.MergeData(GetTestDataSet());
		}


		[Test]
		public void ObjectCreation()
		{
			Title	title;

			title = new TitleFactory(context).CreateObject("TC1234");
			Assertion.Assert("Couldn't create title object.", title != null);
		}


		[Test]
		public void PrimaryKeyFind()
		{
			TitleFactory	factory;
			Title			title;

			factory = new TitleFactory(context);
			title = factory.FindObject("TC7777");
			Assertion.AssertNotNull("Did not find title with key TC7777.", title);
		}

		
		[Test]
		public void TemplateBasedFind()
		{
			TitleFactory	factory;
			TitleTemplate	template;
			TitleList		titleList;

			factory = new TitleFactory(context);
			template = factory.GetQueryTemplate();
			template.TitleId = "TC7777";
			titleList = factory.Find(template);
			Assertion.AssertEquals("Matched zero or too many titles.", 1, titleList.Count);
		}


		[Test]
		public void TemplateBasedFindWithRelation()
		{
			TitleFactory	factory;
			PublisherList	pubList;
			TitleTemplate	template;
			TitleList		titleList;

			pubList = new PublisherFactory(context).Find("PubId = '0877'"); 
			Assertion.AssertEquals("Matched zero or too many publishers.", 1, pubList.Count);

			factory = new TitleFactory(context);
			template = factory.GetQueryTemplate();
			template.Publisher = pubList[0];
			titleList = factory.Find(template);
			Assertion.AssertEquals("Wrong number of titles for publisher.", 7, titleList.Count);
		}


		[Test]
		public void QualifierBasedFind()
		{
			TitleFactory	factory;
			TitleList		titleList;

			factory = new TitleFactory(context);
			titleList = factory.Find("TitleId = 'TC7777'");
			Assertion.AssertEquals("Wrong number of titles matching.", 1, titleList.Count);
			Assertion.AssertEquals("Wrong title.", "TC7777", titleList[0].TitleId);
		}


	}
}
