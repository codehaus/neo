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
			Assertion.Assert("No file named Title.cs", File.Exists("Title.cs"));
			Assertion.Assert("No file named _Title.cs", File.Exists("_Title.cs"));
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
			Assertion.Assert("No/wrong namespace def.", contents.IndexOf("namespace pubs4.Model") > 0);
			Assertion.Assert("No/wrong class def.", contents.IndexOf("public class Title : TitleBase") > 0);
		}


		[Test]
		public void EndToEndBaseClass()
		{
			TextReader	reader;
			string		contents;

			reader = new StreamReader("_Title.cs");
			contents = reader.ReadToEnd();
			reader.Close();

		    Console.WriteLine(contents);

			// Check some random strings that should definitely be in the file
			Assertion.Assert("No/wrong namespace def.", contents.IndexOf("namespace pubs4.Model") > 0);
			Assertion.Assert("No/wrong class def.", contents.IndexOf("public class TitleBase : EntityObject") > 0);
			Assertion.Assert("No/wrong property def, TitleId signature.", contents.IndexOf("public virtual System.String TitleId") > 0);
		}
	}
}