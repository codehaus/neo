using System;
using System.Collections;
using System.IO;
using Neo.Generator.Core;
using Neo.MetaModel;


namespace Neo.Generator.CodeGen
{	
	public class CodeGenerator : VelocityGenerator
	{

		//--------------------------------------------------------------------------------------
		//	fields and constructor
		//--------------------------------------------------------------------------------------
		
		private string	userClassTemplate;
		private string	supportClassesTemplate;
		private bool	generatesUserClasses;
	    private bool	generatesSupportClasses;
		private bool	forcesUserClassGen;
		private string	defaultNamespace;

		
		public CodeGenerator() : base()
		{
			userClassTemplate = "NeoClass.vtl";
			supportClassesTemplate = "NeoSupport.vtl";
		}


		//--------------------------------------------------------------------------------------
		//	properties
		//--------------------------------------------------------------------------------------

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

		public bool GeneratesUserClasses
		{
			get { return generatesUserClasses; }
			set { generatesUserClasses = value; }
		}

		public bool GeneratesSupportClasses
		{
			get { return generatesSupportClasses; }
			set { generatesSupportClasses = value; }
		}

		public bool ForcesUserClassGen
		{
			set { forcesUserClassGen = value; }
			get { return forcesUserClassGen; }
		}

		public string DefaultNamespace
		{
			get { return defaultNamespace; }
			set { defaultNamespace = value; }
		}

		
		//--------------------------------------------------------------------------------------
		//	generate files
		//--------------------------------------------------------------------------------------
	
		public override IList Generate(string inputFile, string outputPath)
		{
			ArrayList files = new ArrayList();
			if(generatesUserClasses)
				files.AddRange(GenerateClassFiles(inputFile, outputPath, true, forcesUserClassGen));
			if(generatesSupportClasses)
				files.AddRange(GenerateClassFiles(inputFile, outputPath, false, true));
			return files;
		}

	
		public IList GenerateClassFiles(string inputFile, string outputPath, bool genUser, bool forceWrite)
		{
		    IModelReader modelReader;
			ArrayList	 fileList;
			Entity		 entity;
			string		 dir;

			modelReader = (IModelReader)Activator.CreateInstance(readerType);
			fileList = new ArrayList();

			modelReader.ReadConfiguration(inputFile, new ConfigDelegate(setPiAttribute));
			if((dir = outputPath) == null)
				if((dir = Path.GetDirectoryName(inputFile)) == null)
					dir = "";
			modelReader.LoadModel(inputFile);
			if(modelReader.Model.Namespace == null)
				modelReader.Model.Namespace = DefaultNamespace;

			while((entity = modelReader.GetNextEntity()) != null)
			{
			    CodeGenerationContext ctx;
				string subDir, outputFile;

				ctx = new CodeGenerationContext(entity);

				if(entity.SubPackageName != null)
					subDir = Path.Combine(dir, entity.SubPackageName);
				else
					subDir = dir;
				outputFile = Path.Combine(subDir, (genUser ? ctx.FileName : ctx.SupportFileName));

				if((forceWrite) || (File.Exists(outputFile) == false))
				{
					Console.WriteLine("Writing {0}", outputFile);
					using(TextWriter writer = new StreamWriter(outputFile))
					{
						Generate(genUser ? userClassTemplate : supportClassesTemplate , ctx, writer);   	
					}
				}
				fileList.Add(outputFile);
			}
			return fileList;
		}
		

		//--------------------------------------------------------------------------------------
		//	generate support classes into a stream
		//--------------------------------------------------------------------------------------

		public virtual void GenerateSupportClasses(string inputFile, TextWriter output)
		{
			IModelReader modelReader;
			Entity		 entity;

			modelReader = (IModelReader)Activator.CreateInstance(readerType);
			modelReader.ReadConfiguration(inputFile, new ConfigDelegate(setPiAttribute));
			modelReader.LoadModel(inputFile);
			if(modelReader.Model.Namespace == null)
				modelReader.Model.Namespace = DefaultNamespace;

			while((entity = modelReader.GetNextEntity()) != null)
			{
			    CodeGenerationContext ctx = new CodeGenerationContext(entity);
				Generate(supportClassesTemplate, ctx, output);
			}
		}


		//--------------------------------------------------------------------------------------
		//	helper methods
		//--------------------------------------------------------------------------------------

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

	
	}
}
