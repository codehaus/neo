using Movies.Forms;
using Movies.Model;
using Neo.Core;
using NUnit.Framework;


namespace Movies.Tests.Forms
{
	[TestFixture]
	public class MovieEntryFormTests : TestBase
	{
		[SetUp]
		public void SetUp()
		{
			SetupLog4Net();
		}

		[Test]
		public void LoadsDirectorList()
		{
			ObjectContext context = new ObjectContext();
			PersonFactory factory = new PersonFactory(context);
			factory.CreateObject().Name = "Ridley Scott";
			factory.CreateObject().Name = "George Lucas";

			MovieEntryForm form = new MovieEntryForm(context);
			Assert.AreEqual(2, form.directorComboBox.Items.Count, "Should display both directors.");
		}

		[Test]
		public void AddsNewMovieOnOk()
		{
			ObjectContext context = new ObjectContext();
			MovieEntryForm form = new MovieEntryForm(context);
			form.titleTextBox.Text = "Star Wars";
			form.yearTextBox.Text = "1977";
			form.okButton_Click(this, null);

			MovieList movies = new MovieFactory(context).FindAllObjects();
			Assert.AreEqual(1, movies.Count, "Should have added exactly one movie.");
			Assert.AreEqual("Star Wars", movies[0].Title, "Should have set title.");
			Assert.AreEqual(1977, movies[0].Year, "Should have set year.");
		}

		[Test]
		public void AddsNewMovieWithNoYearOnOk()
		{
			ObjectContext context = new ObjectContext();
			MovieEntryForm form = new MovieEntryForm(context);
			form.titleTextBox.Text = "Star Wars";
			form.okButton_Click(this, null);

			MovieList movies = new MovieFactory(context).FindAllObjects();
			Assert.AreEqual(1, movies.Count, "Should have added exactly one movie.");
			Assert.AreEqual(0, movies[0].Year, "Should not have set year.");
		}

		[Test]
		public void AddsNewMovieWithDirectorOnOk()
		{
			ObjectContext context = new ObjectContext();
			PersonFactory factory = new PersonFactory(context);
			factory.CreateObject().Name = "Ridley Scott";
			Person george = factory.CreateObject();
			george.Name = "George Lucas";

			MovieEntryForm form = new MovieEntryForm(context);
			form.titleTextBox.Text = "Star Wars";
			form.directorComboBox.SelectedItem = george;
			form.okButton_Click(this, null);

			MovieList movies = new MovieFactory(context).FindAllObjects();
			Assert.AreEqual(1, movies.Count, "Should have added exactly one movie.");
			Assert.AreEqual(george, movies[0].Director, "Should have set director.");
		}

	}
}
