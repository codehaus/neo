using Movies.Model;
using Neo.Core;
using NUnit.Framework;


namespace Movies.Tests.Model
{
	public class MovieTests : TestBase
	{
		private ObjectContext context;
		private Person carrie;
		private Movie starwars;

		[SetUp]
		public void SetUp()
		{
			SetupLog4Net();

			context = new ObjectContext();
			
			carrie = new PersonFactory(this.context).CreateObject();
			starwars = new MovieFactory(this.context).CreateObject();
		}

		[Test]
		public void CreatesAndAssingsLinkObjectWhenAddingActor()
		{
			starwars.AddActor(carrie);

			MovieActorLinkList links = new MovieActorLinkFactory(this.context).FindAllObjects();
			Assert.AreEqual(1, links.Count, "Should have created exactly one link object");
			Assert.AreEqual(carrie, links[0].Actor, "Should have assigned right actor to link object");
			Assert.AreEqual(starwars, links[0].Movie, "Should have assigned right movie to link object");
		}

		[Test]
		public void ReturnsLinkedActorObjectsFromActorProperty()
		{
			starwars.AddActor(carrie);
			PersonList actors = starwars.Actors;

			Assert.AreEqual(1, actors.Count, "Should have returned list with one actor");
			Assert.AreEqual(carrie, actors[0], "Should have returned linked actor");
		}


	}
}
