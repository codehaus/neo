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
	public class CompiledClassesTests
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

			Assert.AreEqual("pubs4.Model", titleType.Namespace, "Namespace is wrong");
		}

		[Test] 
		public void EntityObjectInheritenceChainIsCorrect()
		{
			Type titleBaseType = assembly.GetType("pubs4.Model.TitleBase", true);
			Type titleType = assembly.GetType("pubs4.Model.Title", true);

			Assert.AreEqual(typeof(EntityObject), titleBaseType.BaseType, "TitleBase does not inherit from EntityObject");
			Assert.AreEqual(titleBaseType, titleType.BaseType, "Title does not inherit from TitleBase");
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
			Assert.IsTrue(constructors.Length == 1, "Wrong number of constructors");

			ConstructorInfo constructor = constructors[0];
			Assert.IsTrue(constructor.IsFamilyOrAssembly, "Constructor should be protected internal");

			ParameterInfo[] parameters = constructor.GetParameters();
			Assert.IsTrue(parameters.Length == 2, "Wrong number of args");
			Assert.AreEqual(typeof(DataRow), parameters[0].ParameterType, "Arg 1 type is wrong");
			Assert.AreEqual(typeof(ObjectContext), parameters[1].ParameterType, "Arg 2 type is wrong");
		}

		[Test] 
		public void MultiValueCreateMethodsAreCorrect()
		{
			Type factoryType = assembly.GetType("pubs4.Model.JobRecordFactory", true);
			MethodInfo method = factoryType.GetMethod("CreateObject", BindingFlags.Instance | BindingFlags.Public);
			Assert.IsNotNull(method, "Should have generated CreateObject method on factory.");

			ParameterInfo[] parameters = method.GetParameters();
			Assert.AreEqual(2, parameters.Length, "Wrong number of args");
			Assert.AreEqual("Job", parameters[0].ParameterType.Name, "Arg 1 type is wrong");
			Assert.AreEqual(typeof(DateTime), parameters[1].ParameterType, "Arg 2 type is wrong");
		}

		[Test] 
		public void MultiValueFindMethodsAreCorrect()
		{
			Type factoryType = assembly.GetType("pubs4.Model.JobRecordFactory", true);
			MethodInfo method = factoryType.GetMethod("CreateObject", BindingFlags.Instance | BindingFlags.Public);
			Assert.IsNotNull(method, "Should have generated FindObject method on factory.");

			ParameterInfo[] parameters = method.GetParameters();
			Assert.AreEqual(2, parameters.Length, "Wrong number of args");
			Assert.AreEqual("Job", parameters[0].ParameterType.Name, "Arg 1 type is wrong");
			Assert.AreEqual(typeof(DateTime), parameters[1].ParameterType, "Arg 2 type is wrong");
		}

		[Test] 
		public void MultiRelationCreateMethodsAreCorrect()
		{
			Type factoryType = assembly.GetType("pubs4.Model.CorrelationFactory", true);
			MethodInfo method1 = factoryType.GetMethod("CreateObject", GetTypes("Publisher", "Publisher"));
			Assert.IsNotNull(method1, "Should have generated CreateObject method with two publishers on factory.");

			MethodInfo method2 = factoryType.GetMethod("CreateObject", GetTypes("Publisher", "Title"));
			Assert.IsNotNull(method2, "Should have generated CreateObject method with publisher/title on factory.");

			MethodInfo method3 = factoryType.GetMethod("CreateObject", GetTypes("Title", "Publisher"));
			Assert.IsNotNull(method3, "Should have generated CreateObject method with publisher/title on factory.");
		
			MethodInfo method4 = factoryType.GetMethod("CreateObject", GetTypes("Title", "Title"));
			Assert.IsNotNull(method4, "Should have generated CreateObject method with two titles on factory.");
		}

		[Test] 
		public void QueryTemplatesImplementIFetchSpecification()
		{
			Type titleTemplateType = assembly.GetType("pubs4.Model.TitleTemplate", true);
			Type iFetchSpecificationType = titleTemplateType.GetInterface("IFetchSpecification");

			Assert.IsNotNull(iFetchSpecificationType, "Template type should implement IFetchSpecification");
		}

		[Test] 
		public void PropertiesAreCorrect()
		{
			Type jobBaseType = assembly.GetType("pubs4.Model.JobBase", true);

			PropertyInfo[] properties = jobBaseType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

			Assert.AreEqual(4, properties.Length, "Wrong number of properties");
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
			Assert.IsTrue(results.Errors.Count == 0, "Failed to compile assembly. (See console output for details.)");
			
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
