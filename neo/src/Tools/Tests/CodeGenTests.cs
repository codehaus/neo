using System;
using System.IO;
using Neo.Generator.CodeGen;
using Neo.MetaModel.Reader;
using NUnit.Framework;


namespace Neo.Tools.Tests
{
	[TestFixture]
	public class CodeGenTests
	{
		string singleTableSchemaPath = @"..\..\SingleTableSchema.xml";
		string templateDir = @"..\..\..\Generator\Resources";

		[SetUp]
		public void SetUp()
		{
		    CodeGenerator codeGenerator = new CodeGenerator();
			codeGenerator.ReaderType = typeof(NorqueReader);
			codeGenerator.ResourcePath = templateDir;
			codeGenerator.GeneratesSupportClasses = true;
			codeGenerator.GeneratesUserClasses = true;
			codeGenerator.ForcesUserClassGen = true;
			codeGenerator.Generate(singleTableSchemaPath, ".");
		}

		[TearDown]
		public void TearDown()
		{
		    File.Delete("Title.cs");
			File.Delete("_Title.cs");
		}

		[Test]
		public void EndToEnd() 
		{
			Assert.IsTrue(File.Exists("Title.cs"), "No file named Title.cs");
			Assert.IsTrue(File.Exists("_Title.cs"), "No file named _Title.cs");
		}

		[Test]
		public void EndToEndUserClass()
		{
			TextReader	reader;
			string		contents;

			reader = new StreamReader("Title.cs");
			contents = reader.ReadToEnd();
			reader.Close();

			// Check some random strings that should definitely be in the file
			Assert.IsTrue(contents.IndexOf("namespace pubs4.Model") > 0, "No/wrong namespace def.");
			Assert.IsTrue(contents.IndexOf("public class Title : TitleBase") > 0, "No/wrong class def.");
		}


		[Test]
		public void EndToEndBaseClass()
		{
			TextReader	reader;
			string		contents;

			reader = new StreamReader("_Title.cs");
			contents = reader.ReadToEnd();
			reader.Close();

			// Check some random strings that should definitely be in the file
			Assert.IsTrue(contents.IndexOf("namespace pubs4.Model") > 0, "No/wrong namespace def.");
			Assert.IsTrue(contents.IndexOf("public class TitleBase : EntityObject") > 0, "No/wrong class def.");
			Assert.IsTrue(contents.IndexOf("public virtual System.String TitleId") > 0, "No/wrong property def, TitleId signature.");
		}
	}
}
