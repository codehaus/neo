using System;
using System.Collections;
using System.IO;
using Neo.MetaModel;
using NVelocity;
using NVelocity.App;
using NVelocity.Runtime;


namespace Neo.Generator.Core
{	
	public class VelocityGenerator
	{

		//--------------------------------------------------------------------------------------
		//	fields and constructor
		//--------------------------------------------------------------------------------------
		
		private static bool	velocityInitialized;
		
		protected Type		readerType;
		protected string	template;

		
		public VelocityGenerator()
		{
			Velocity.SetProperty(RuntimeConstants_Fields.RUNTIME_LOG_LOGSYSTEM_CLASS, "NVelocity.Runtime.Log.NullLogSystem");
		}


		//--------------------------------------------------------------------------------------
		//	properties
		//--------------------------------------------------------------------------------------

		public string ResourcePath
		{
			set { Velocity.SetProperty(RuntimeConstants_Fields.FILE_RESOURCE_LOADER_PATH, value); }
		}

		public Type ReaderType
		{
			set { readerType = value; }
			get { return readerType; }
		}

		public string Template
		{
			set { template = value; }
			get { return template; }
		}


		//--------------------------------------------------------------------------------------
		//	generate files
		//--------------------------------------------------------------------------------------
	
		public virtual IList Generate(string inputPath, string outputFile)
		{
			IModelReader modelReader;
			TextWriter	 writer;

			modelReader = (IModelReader)Activator.CreateInstance(readerType);
			modelReader.ReadConfiguration(inputPath, new ConfigDelegate(SetConfigurationAttribute));
			modelReader.LoadModel(inputPath);

			Console.WriteLine("Writing {0}", outputFile);
			
			writer = new StreamWriter(outputFile);
			GenerationContext ctx = new GenerationContext(modelReader.Model);
			Generate(template, ctx, writer);   		
			writer.Close();

			return new ArrayList(new object[] { outputFile });
		}
		

		protected virtual void Generate(string templateName, GenerationContext neoContext, TextWriter writer)
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
		
		
		//--------------------------------------------------------------------------------------
		//	helper methods
		//--------------------------------------------------------------------------------------

		protected virtual void SetConfigurationAttribute(string inputPath, string attr, string val)
		{
			switch(attr)
			{
				case "path":
					ResourcePath = Path.Combine(Path.GetDirectoryName(inputPath), val);
					break;
				default:
					throw new ApplicationException("Invalid configuration attribute; found " + attr);
			}
		}

	
	}
}
