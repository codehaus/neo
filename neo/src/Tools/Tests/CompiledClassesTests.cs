using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Data;
using System.IO;
using System.Reflection;
using Microsoft.CSharp;
using Neo.Core;
using Neo.Framework;
using Neo.MetaModel.Reader;
using NUnit.Framework;
using CodeGenerator = Neo.Generator.CodeGen.CodeGenerator;


namespace Neo.Tools.Tests
{
	[TestFixture]
	public class CompiledClassesTests : Assertion
	{
		string[] sourceFileNames;
		Assembly assembly;


		[SetUp] 
		public void SetUp()
		{
			GenerateSourceFiles();
			CompileSourcesToAssembly();
		}

		[TearDown] 
		public void TearDown()
		{
			foreach(string fileName in sourceFileNames)
				File.Delete(fileName);
		}


		[Test] 
		public void NamespaceIsCorrect()
		{
			Type titleType = assembly.GetType("pubs4.Model.Title", true);

			AssertEquals("Namespace is wrong", "pubs4.Model", titleType.Namespace);
		}

		[Test] 
		public void EntityObjectInheritenceChainIsCorrect()
		{
			Type titleBaseType = assembly.GetType("pubs4.Model.TitleBase", true);
			Type titleType = assembly.GetType("pubs4.Model.Title", true);

			AssertEquals("TitleBase does not inherit from EntityObject", typeof(EntityObject), titleBaseType.BaseType);
			AssertEquals("Title does not inherit from TitleBase", titleBaseType, titleType.BaseType);
		}

		[Test] 
		public void SupportClassesExist()
		{
			//GetType will throw if any of these types aren't found, failing the test
			assembly.GetType("pubs4.Model.TitleList",		true);
			assembly.GetType("pubs4.Model.TitleFactory",	true);
			assembly.GetType("pubs4.Model.TitleMap",		true);
			assembly.GetType("pubs4.Model.TitleTemplate",	true);
			assembly.GetType("pubs4.Model.TitleRelation",	true);
		}

		[Test] 
		public void ConstructorsAreCorrect()
		{
			Type titleBaseType = assembly.GetType("pubs4.Model.Title", true);
			ConstructorInfo[] constructors = titleBaseType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
			Assert("Wrong number of constructors", constructors.Length == 1);

			ConstructorInfo constructor = constructors[0];
			Assert("Constructor should be protected internal", constructor.IsFamilyOrAssembly);

			ParameterInfo[] parameters = constructor.GetParameters();
			Assert("Wrong number of args", parameters.Length == 2);
			AssertEquals("Arg 1 type is wrong", typeof(DataRow), parameters[0].ParameterType);
			AssertEquals("Arg 2 type is wrong", typeof(ObjectContext), parameters[1].ParameterType);
		}

		[Test] 
		public void MultiValueCreateMethodsAreCorrect()
		{
			Type factoryType = assembly.GetType("pubs4.Model.JobRecordFactory", true);
			MethodInfo method = factoryType.GetMethod("CreateObject", BindingFlags.Instance | BindingFlags.Public);
			AssertNotNull("Should have generated CreateObject method on factory.", method);

			ParameterInfo[] parameters = method.GetParameters();
			AssertEquals("Wrong number of args", 2, parameters.Length);
			AssertEquals("Arg 1 type is wrong", "Job", parameters[0].ParameterType.Name);
			AssertEquals("Arg 2 type is wrong", typeof(DateTime), parameters[1].ParameterType);
		}

		[Test] 
		public void MultiValueFindMethodsAreCorrect()
		{
			Type factoryType = assembly.GetType("pubs4.Model.JobRecordFactory", true);
			MethodInfo method = factoryType.GetMethod("CreateObject", BindingFlags.Instance | BindingFlags.Public);
			AssertNotNull("Should have generated FindObject method on factory.", method);

			ParameterInfo[] parameters = method.GetParameters();
			AssertEquals("Wrong number of args", 2, parameters.Length);
			AssertEquals("Arg 1 type is wrong", "Job", parameters[0].ParameterType.Name);
			AssertEquals("Arg 2 type is wrong", typeof(DateTime), parameters[1].ParameterType);
		}

		[Test] 
		public void MultiRelationCreateMethodsAreCorrect()
		{
			Type factoryType = assembly.GetType("pubs4.Model.CorrelationFactory", true);
			MethodInfo method1 = factoryType.GetMethod("CreateObject", GetTypes("Publisher", "Publisher"));
			AssertNotNull("Should have generated CreateObject method with two publishers on factory.", method1);

			MethodInfo method2 = factoryType.GetMethod("CreateObject", GetTypes("Publisher", "Title"));
			AssertNotNull("Should have generated CreateObject method with publisher/title on factory.", method2);

			MethodInfo method3 = factoryType.GetMethod("CreateObject", GetTypes("Title", "Publisher"));
			AssertNotNull("Should have generated CreateObject method with publisher/title on factory.", method3);
		
			MethodInfo method4 = factoryType.GetMethod("CreateObject", GetTypes("Title", "Title"));
			AssertNotNull("Should have generated CreateObject method with two titles on factory.", method4);
		}

		[Test] 
		public void QueryTemplatesImplementIFetchSpecification()
		{
			Type titleTemplateType = assembly.GetType("pubs4.Model.TitleTemplate", true);
			Type iFetchSpecificationType = titleTemplateType.GetInterface("IFetchSpecification");

			AssertNotNull("Template type should implement IFetchSpecification", iFetchSpecificationType);
		}

		[Test] 
		public void PropertiesAreCorrect()
		{
			Type jobBaseType = assembly.GetType("pubs4.Model.JobBase", true);

			PropertyInfo[] properties = jobBaseType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

			AssertEquals("Wrong number of properties", 4, properties.Length);
		}

		#region SupportMethods

		void GenerateSourceFiles()
		{
			CodeGenerator codeGenerator = new CodeGenerator();
			codeGenerator.ReaderType = typeof(NorqueReader);
			codeGenerator.ResourcePath = @"..\..\..\Generator\Resources";
			codeGenerator.GeneratesSupportClasses = true;
			codeGenerator.GeneratesUserClasses = true;
			codeGenerator.ForcesUserClassGen = true;
			IList files = codeGenerator.Generate(@"..\..\CompiledClassesTestsSchema.xml", @".");
			sourceFileNames = new string[files.Count];
			files.CopyTo(sourceFileNames, 0);
		}

		void CompileSourcesToAssembly()
		{
			ICodeCompiler compiler = new CSharpCodeProvider().CreateCompiler();
			string[] referencedAssemblies = new String[]{"Neo.dll", "System.dll", "System.Data.dll", "System.Xml.dll"};
			CompilerParameters options = new CompilerParameters(referencedAssemblies);
			options.OutputAssembly = "pubs4";
			options.GenerateInMemory = true;
			CompilerResults results = compiler.CompileAssemblyFromFileBatch(options , sourceFileNames);

			foreach(CompilerError error in results.Errors)
				Console.WriteLine(error.ToString());
			Assertion.Assert("Failed to compile assembly. (See console output for details.)", results.Errors.Count == 0);
			
			assembly = results.CompiledAssembly;
		}

		Type[] GetTypes(params string[] names)
		{
			Type[] types = new Type[names.Length];
			for(int i = 0; i < names.Length; i++)
				types[i] = assembly.GetType("pubs4.Model." + names[i]);
			return types;
		}

		#endregion


	}
}