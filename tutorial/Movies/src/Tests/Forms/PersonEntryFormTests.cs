using Movies.Forms;
using Movies.Model;
using Neo.Core;
using NUnit.Framework;


namespace Movies.Tests.Forms
{
	[TestFixture]
	public class PersonEntryFormTests : TestBase
	{
		[SetUp]
		public void SetUp()
		{
			SetupLog4Net();
		}

		[Test]
		public void AddsNewPersonOnOk()
		{
			ObjectContext context = new ObjectContext();
			PersonEntryForm form = new PersonEntryForm(context);
			form.nameTextBox.Text = "Ridley Scott";
			form.okButton_Click(this, null);

			PersonList persons = new PersonFactory(context).FindAllObjects();
			Assert.AreEqual(1, persons.Count, "Should have added exactly one person.");
			Assert.AreEqual("Ridley Scott", persons[0].Name, "Should have set the name.");
		}

	}
}
