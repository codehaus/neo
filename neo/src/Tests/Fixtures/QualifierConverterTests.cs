using Neo.Core;
using Neo.Core.Qualifiers;
using Neo.Core.Util;
using NUnit.Framework;
using Pubs4.Model;


namespace Neo.Tests.Fixtures
{
	[TestFixture]
	public class QualifierConverterTests : TestBase
	{
		ObjectContext	context;

		[SetUp]
		public void SetUp()
		{
			SetupLog4Net();

			context = new ObjectContext();
		}


		[Test]
		public void ConvertsPropertyQualifierToColumnQualifier()
		{
			IEntityMap titleMap = context.EntityMapFactory.GetMap(typeof(Title));
			QualifierConverter converter = new QualifierConverter(titleMap);
			Publisher pub = new PublisherFactory(context).CreateObject("5555");
			PropertyQualifier propertyQualifier = new PropertyQualifier("Publisher", new EqualsPredicate(pub));
			
			Qualifier convertedQualifier = converter.ConvertToColumnQualifier(propertyQualifier);

			Assert.AreEqual(typeof(ColumnQualifier), convertedQualifier.GetType(), "Should have produced a column qualifier.");
			ColumnQualifier convertedAsColumnQualifier = convertedQualifier as ColumnQualifier;
			Assert.AreEqual("pub_id", convertedAsColumnQualifier.Column);
			Assert.AreEqual("5555", convertedAsColumnQualifier.Predicate.Value);
		}


		[Test]
		public void ConvertsPropertyQualifierToColumnQualifierRecursively()
		{
			IEntityMap titleMap = context.EntityMapFactory.GetMap(typeof(Title));
			QualifierConverter converter = new QualifierConverter(titleMap);
			Publisher pub = new PublisherFactory(context).CreateObject("5555");
			PropertyQualifier propertyQualifier = new PropertyQualifier("Publisher", new EqualsPredicate(pub));
			AndQualifier toplevelQualifier = new AndQualifier(propertyQualifier);

			Qualifier convertedQualifier = converter.ConvertToColumnQualifiersRecursively(toplevelQualifier);

			Assert.AreEqual(typeof(AndQualifier), convertedQualifier.GetType(), "Should have kept and qualifier.");
			Qualifier nestedQualifier = ((AndQualifier)convertedQualifier).Qualifiers[0];
			Assert.AreEqual(typeof(ColumnQualifier), nestedQualifier.GetType(), "Should have produced a column qualifier.");
			ColumnQualifier convertedAsColumnQualifier = nestedQualifier as ColumnQualifier;
			Assert.AreEqual("pub_id", convertedAsColumnQualifier.Column);
			Assert.AreEqual("5555", convertedAsColumnQualifier.Predicate.Value);
		}


		[Test]
		public void ConvertsEntityObjectsInPropertyQualifiers()
		{
			IEntityMap titleMap = context.EntityMapFactory.GetMap(typeof(Title));
			QualifierConverter converter = new QualifierConverter(titleMap);
			Publisher pub = new PublisherFactory(context).CreateObject("5555");
			PropertyQualifier propertyQualifier = new PropertyQualifier("Publisher", new EqualsPredicate(pub));
			ObjectContext childContext = new ObjectContext(context);
			
			Qualifier convertedQualifier = converter.MoveEntityObject(propertyQualifier, childContext);

			Assert.AreEqual(typeof(PropertyQualifier), convertedQualifier.GetType(), "Should have kept property qualifier.");
			PropertyQualifier convertedAsPropertyQualifier = convertedQualifier as PropertyQualifier;
			Publisher pubInChildContext = (Publisher)childContext.GetLocalObject(pub);
			Assert.AreEqual(pubInChildContext, convertedAsPropertyQualifier.Predicate.Value);
		}


		[Test]
		public void ConvertsEntityObjectsInPropertyQualifiersRecursively()
		{
			IEntityMap titleMap = context.EntityMapFactory.GetMap(typeof(Title));
			QualifierConverter converter = new QualifierConverter(titleMap);
			Publisher pub = new PublisherFactory(context).CreateObject("5555");
			PropertyQualifier propertyQualifier = new PropertyQualifier("Publisher", new EqualsPredicate(pub));
			AndQualifier toplevelQualifier = new AndQualifier(propertyQualifier);
			ObjectContext childContext = new ObjectContext(context);
			
			Qualifier convertedQualifier = converter.MoveEntityObjectsRecursively(toplevelQualifier, childContext);

			Assert.AreEqual(typeof(AndQualifier), convertedQualifier.GetType(), "Should have kept and qualifier.");
			Qualifier nestedQualifier = ((AndQualifier)convertedQualifier).Qualifiers[0];
			Assert.AreEqual(typeof(PropertyQualifier), nestedQualifier.GetType(), "Should have produced a property qualifier.");
			Publisher pubInChildContext = (Publisher)childContext.GetLocalObject(pub);
			Assert.AreEqual(pubInChildContext, ((PropertyQualifier)nestedQualifier).Predicate.Value	);
		}


		[Test]
		[ExpectedException(typeof(ObjectNotFoundException))]
		public void ConvertsEntityObjectsInPropertyQualifiersRecursivelyThrowsOnNonExistantObjects()
		{
			// Create child context before creating publisher so it doesn't have it.
			ObjectContext childContext = new ObjectContext(context);

			IEntityMap titleMap = context.EntityMapFactory.GetMap(typeof(Title));
			QualifierConverter converter = new QualifierConverter(titleMap);
			Publisher pub = new PublisherFactory(context).CreateObject("5555");
			PropertyQualifier propertyQualifier = new PropertyQualifier("Publisher", new EqualsPredicate(pub));
			AndQualifier toplevelQualifier = new AndQualifier(propertyQualifier);
			
			converter.MoveEntityObjectsRecursively(toplevelQualifier, childContext);
		}

	}

}
