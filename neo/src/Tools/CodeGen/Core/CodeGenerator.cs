using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using NVelocity;
using NVelocity.App;
using NVelocity.Runtime;
using Neo.Model;


namespace Neo.CodeGen.Core
{	
	public class CodeGenerator
	{

		//--------------------------------------------------------------------------------------
		//	fields and constructor
		//--------------------------------------------------------------------------------------
		
		private static bool	velocityInitialized;
		
		private string	userClassTemplate;
		private string	supportClassesTemplate;
		private bool	forceUserClassGen;
		private string	outputPath;
		private Type	readerType;

		
		public CodeGenerator()
		{
			Velocity.SetProperty(RuntimeConstants_Fields.RUNTIME_LOG_LOGSYSTEM_CLASS, "NVelocity.Runtime.Log.NullLogSystem");
			userClassTemplate = "NeoClass.vtl";
			supportClassesTemplate = "NeoSupport.vtl";
		}


		//--------------------------------------------------------------------------------------
		//	properties
		//--------------------------------------------------------------------------------------

		public string ResourcePath
		{
			set { Velocity.SetProperty(RuntimeConstants_Fields.FILE_RESOURCE_LOADER_PATH, value); }
		}

		public string UserClassTemplate
		{
			set { userClassTemplate = value; }
			get { return userClassTemplate; }
		}

		public string SupportClassesTemplate
		{
			set { supportClassesTemplate = value; }
			get { return supportClassesTemplate; }
		}

		public Type ReaderType
		{
			set { readerType = value; }
			get { return readerType; }
		}

		public bool ForceUserClassGen
		{
			set { forceUserClassGen = value; }
			get { return forceUserClassGen; }
		}

		public string OutputPath
		{
			set { outputPath = value; }
			get { return outputPath; }
		}


		//--------------------------------------------------------------------------------------
		//	generate files
		//--------------------------------------------------------------------------------------
	
		public virtual void GenerateSupportClassFiles(string inputFile)
		{
			generateClassFiles(inputFile, false, true);
		}

		
		public virtual void GenerateUserClassFiles(string inputFile)
		{
			generateClassFiles(inputFile, true, ForceUserClassGen);
		}

	
		private void generateClassFiles(string inputFile, bool genUser, bool forceWrite)
		{
			IModelReader modelReader;
			IEntity		 entity;
			string		 dir;

			handleProcessingInstructions(inputFile);
			if((dir = outputPath) == null)
				if((dir = Path.GetDirectoryName(inputFile)) == null)
					dir = "";

			modelReader = (IModelReader)Activator.CreateInstance(readerType);
			modelReader.LoadModel(inputFile);
			while((entity = modelReader.GetNextEntity()) != null)
			{
				GenerationContext ctx;
				string			  subDir, outputFile;
				TextWriter		  writer;

				ctx = new GenerationContext(entity);

				if(entity.SubPackageName != null)
					subDir = Path.Combine(dir, entity.SubPackageName);
				else
					subDir = dir;
				outputFile = Path.Combine(subDir, (genUser ? ctx.FileName : ctx.SupportFileName));

				if((forceWrite) || (File.Exists(outputFile) == false))
				{
					Console.WriteLine("Writing {0}", outputFile);
					writer = new StreamWriter(outputFile);
					generateClass(genUser ? userClassTemplate : supportClassesTemplate , ctx, writer);   	
					writer.Close();
				}
			}
		}
		

		//--------------------------------------------------------------------------------------
		//	generate support classes into a stream
		//--------------------------------------------------------------------------------------

		public virtual void GenerateSupportClasses(string inputFile, TextWriter output)
		{
			IModelReader modelReader;
			IEntity		 entity;

			handleProcessingInstructions(inputFile);

			modelReader = (IModelReader)Activator.CreateInstance(readerType);
			modelReader.LoadModel(inputFile);
			while((entity = modelReader.GetNextEntity()) != null)
			{
				GenerationContext ctx = new GenerationContext(entity);
				generateClass(supportClassesTemplate, ctx, output);
			}
		}


		//--------------------------------------------------------------------------------------
		//	helper methods
		//--------------------------------------------------------------------------------------

		private void handleProcessingInstructions(string inputFile)
		{
			XmlReader	reader;
			Regex		attrExp;
			
			attrExp = new Regex("(?<attr>[a-z]+?)\\s*=\\s*['\"](?<val>.+?)['\"]");
			reader = null;
			try
			{	
				reader = new XmlTextReader(inputFile);
				while(reader.Read())
				{
					if((reader.NodeType != XmlNodeType.ProcessingInstruction) || (reader.Name != "neo"))
						continue;
					Match match = attrExp.Match(reader.Value);
					while(match.Success)
					{
						setPiAttribute(inputFile, match.Result("${attr}"), match.Result("${val}"));
						match = match.NextMatch();
					}
				}
				reader.Close();
			}
			catch(Exception e)
			{
				if(reader != null)
					reader.Close();
				throw e;
			}
		}


		private void setPiAttribute(string filename, string attr, string val)
		{
			switch(attr)
			{
				case "path":
					ResourcePath = Path.Combine(Path.GetDirectoryName(filename), val);
					break;
				case "user":
					UserClassTemplate = val;
					break;
				case "support":
					SupportClassesTemplate = val;
					break;
				default:
					throw new ApplicationException("Invalid attribute in neo processing instruction; found " + attr);
			}
		}


		private void generateClass(string templateName, GenerationContext neoContext, TextWriter writer)
		{
			VelocityContext	  velocityContext;

			if(velocityInitialized == false)
			{
				velocityInitialized = true;
				Velocity.Init();
			}
			velocityContext = new VelocityContext();
			velocityContext.Put("Neo", neoContext);
			Velocity.MergeTemplate(templateName, velocityContext, writer);
		}
	
	}
}
