using System;
using System.Data;
using System.IO;
using System.CodeDom.Compiler;
using System.Reflection;
using Microsoft.CSharp;
using NUnit.Framework;
using Neo.CodeGen.Core;
using Neo.Framework;

namespace Neo.Tools.Tests
{
	[TestFixture]
	public class CompiledClassesTests : Assertion
	{
		#region Fields

		string schemaPath = @"..\..\MultiTableSchema.xml";
		string templateDir = @"..\..\..\CodeGen\Resources";
		string[] _sourceFiles = new String[]{"_Title.cs",
										   "_Publisher.cs",
										   "_Author.cs",
										   "_TitleAuthor.cs",
										   "_Job.cs",
										   "Title.cs",
										   "Publisher.cs",
										   "Author.cs",
										   "TitleAuthor.cs",
										   "Job.cs"};
		Assembly _assembly;

		#endregion

		#region SupportMethods

		void GenerateSourceFiles()
		{
			Neo.CodeGen.Core.CodeGenerator codeGenerator = new Neo.CodeGen.Core.CodeGenerator();
			codeGenerator.ReaderType = typeof(Neo.Model.Reader.NorqueReader);
			codeGenerator.ResourcePath = templateDir;
			codeGenerator.ForceUserClassGen = true;
			codeGenerator.OutputPath = ".";
			codeGenerator.GenerateUserClassFiles(schemaPath);
			codeGenerator.GenerateSupportClassFiles(schemaPath);
		}

		void CompileSourcesToAssembly()
		{
			ICodeCompiler compiler = new CSharpCodeProvider().CreateCompiler();

			string[] referencedAssemblies = new String[]{"Neo.dll", 
															"System.dll",
															"System.Data.dll",
															"System.Xml.dll"};

			CompilerParameters options = new CompilerParameters(referencedAssemblies);

			options.OutputAssembly = "pubs4";
			options.GenerateInMemory = true;
			CompilerResults results = null;

			results = compiler.CompileAssemblyFromFileBatch(options , _sourceFiles);

			foreach(CompilerError error in results.Errors)
			{
				Console.WriteLine(error.ToString());
			}

			_assembly = results.CompiledAssembly;
		}

		#endregion

		[SetUp] public void SetUp()
		{
			GenerateSourceFiles();
			CompileSourcesToAssembly();
		}

		[TearDown] public void TearDown()
		{
			foreach(string fileName in _sourceFiles)
			{
				File.Delete(fileName);
			}
		}


		[Test] public void NamespaceIsCorrect()
		{
			Type titleType = _assembly.GetType("pubs4.Model.Title", /*ThrowOnError = */ true);

			AssertEquals("Namespace is wrong", "pubs4.Model", titleType.Namespace);
		}

		[Test] public void EntityObjectInheritenceChainIsCorrect()
		{
			Type titleBaseType = _assembly.GetType("pubs4.Model.TitleBase", /*ThrowOnError = */ true);
			Type titleType = _assembly.GetType("pubs4.Model.Title", /*ThrowOnError = */ true);

			AssertEquals("TitleBase does not inherit from EntityObject", typeof(EntityObject), titleBaseType.BaseType);
			AssertEquals("Title does not inherit from TitleBase", titleBaseType, titleType.BaseType);
		}

		[Test] public void SupportClassesExist()
		{
			//GetType will throw if any of these types aren't found, failing the test
			_assembly.GetType("pubs4.Model.TitleList",		/*ThrowOnError = */ true);
			_assembly.GetType("pubs4.Model.TitleFactory",	/*ThrowOnError = */ true);
			_assembly.GetType("pubs4.Model.TitleMap",		/*ThrowOnError = */ true);
			_assembly.GetType("pubs4.Model.TitleTemplate",	/*ThrowOnError = */ true);
			_assembly.GetType("pubs4.Model.TitleRelation",	/*ThrowOnError = */ true);
		}

		[Test] public void ConstructorsAreCorrect()
		{
			Type titleBaseType = _assembly.GetType("pubs4.Model.Title", /*ThrowOnError = */ true);
			ConstructorInfo[] constructors = titleBaseType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);

			Assert("Wrong number of constructors", constructors.Length == 1);

			ConstructorInfo constructor = constructors[0];

			Assert("Constructor should be protected", constructor.IsFamily);

			ParameterInfo[] parameters = constructor.GetParameters();

			Assert("Wrong number of args", parameters.Length == 2);

			ParameterInfo rowParam = parameters[0];
			ParameterInfo contextParam = parameters[1];

			AssertEquals("Arg 1 type is wrong", typeof(DataRow), rowParam.ParameterType);
			AssertEquals("Arg 2 type is wrong", typeof(Neo.Core.ObjectContext), contextParam.ParameterType);
		}
		[Test] public void TemplatesImplementIFetchSpecification()
		{
			Type titleTemplateType = _assembly.GetType("pubs4.Model.TitleTemplate", /*ThrowOnError = */ true);
			Type iFetchSpecificationType = titleTemplateType.GetInterface("IFetchSpecification");

			AssertNotNull("Template type should implement IFetchSpecification", iFetchSpecificationType);
		}
		[Test] public void ClassesWithRelationsOverrideDelete()
		{
			Type titleType = _assembly.GetType("pubs4.Model.Title", /*ThrowOnError = */ true);
			MethodInfo deleteMethod = titleType.GetMethod("Delete", BindingFlags.Instance | BindingFlags.Public);
			
			AssertNotNull("Method not found in Assembly", deleteMethod);
		}

		[Test] public void ClassesWithNoRelationsDoNotOverrideDelete()
		{
			Type jobType = _assembly.GetType("pubs4.Model.Job", /*ThrowOnError = */ true);
			MethodInfo deleteMethod = jobType.GetMethod("Delete", BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

			AssertNull("Delete should not have been overridden", deleteMethod);
		}

		[Test] public void PropertiesAreCorrect()
		{
			Type jobBaseType = _assembly.GetType("pubs4.Model.JobBase", /*ThrowOnError = */ true);

			PropertyInfo[] properties = jobBaseType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

			AssertEquals("Wrong number of properties", 4, properties.Length);

			AssertNotNull("Missing property", jobBaseType.GetProperty("JobId"));
			AssertNotNull("Missing property", jobBaseType.GetProperty("Description"));
			AssertNotNull("Missing property", jobBaseType.GetProperty("MaxLevel"));
			AssertNotNull("Missing property", jobBaseType.GetProperty("MinLevel"));
		}
	}
}