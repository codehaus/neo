using Neo.Core;
using NUnit.Framework;
using Pubs4.Model;


namespace Neo.Tests.Fixtures
{
	[NUnit.Framework.TestFixture]
	public class ObjectFactoryTests : TestBase
	{
		protected ObjectContext context;


		[NUnit.Framework.SetUp]
		public void SetUp()
		{
			SetupLog4Net();
			
			context = new ObjectContext();
			context.MergeData(GetTestDataSet());
		}


		[NUnit.Framework.Test]
		public void ObjectCreation()
		{
		    Title	title;

			title = new TitleFactory(context).CreateObject("TC1234");
		    Assert.IsTrue(title != null, "Couldn't create title object.");
		}


		[Test]
		public void PrimaryKeyFind()
		{
			TitleFactory	factory;
			Title			title;

			factory = new TitleFactory(context);
			title = factory.FindObject("TC7777");
			Assert.IsNotNull(title, "Did not find title with key TC7777.");
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
			Assert.AreEqual(1, titleList.Count, "Matched zero or too many titles.");
		}

		
		[Test]
		public void EmptyTemplateBasedFind()
		{
			TitleFactory	factory;
			TitleTemplate	template;
			TitleList		titleList;

			factory = new TitleFactory(context);
			template = factory.GetQueryTemplate();
			titleList = factory.Find(template);
			Assert.AreEqual(18, titleList.Count, "Empty Template should fetch all records.");
		}


		[Test]
		public void TemplateBasedFindWithRelation()
		{
			TitleFactory	factory;
			PublisherList	pubList;
			TitleTemplate	template;
			TitleList		titleList;

			pubList = new PublisherFactory(context).Find("PubId = '0877'"); 
			Assert.AreEqual(1, pubList.Count, "Matched zero or too many publishers.");

			factory = new TitleFactory(context);
			template = factory.GetQueryTemplate();
			template.Publisher = pubList[0];
			titleList = factory.Find(template);
			Assert.AreEqual(7, titleList.Count, "Wrong number of titles for publisher.");
		}


		[Test]
		public void QualifierBasedFind()
		{
			TitleFactory	factory;
			TitleList		titleList;

			factory = new TitleFactory(context);
			titleList = factory.Find("TitleId = 'TC7777'");
			Assert.AreEqual(1, titleList.Count, "Wrong number of titles matching.");
			Assert.AreEqual("TC7777", titleList[0].TitleId, "Wrong title.");
		}


	}
}
