using System;
using Neo.Generator.CodeGen;
using Neo.Generator.Core;
using Neo.MetaModel.Reader;


namespace Neo.CmdLineTool
{
	class MainStub
	{
		[STAThread]
		static void Main(string[] args)
		{
			CmdLineArguments	arguments;
			VelocityGenerator	generator;
		    string				rpath, opath, template, defaultNamespace;
			bool				force, debug, genSupport, genUser;

			arguments = new CmdLineArguments(args);
			force = (arguments["f"] == "true");
			debug = (arguments["d"] == "true");
			genSupport = (arguments["support"] == "true");
			genUser = (arguments["user"] == "true");
			template = arguments["t"];
			rpath = arguments["r"];
			opath = arguments["o"];
			defaultNamespace = arguments["namespace"];
			
			if(debug)
				Console.ReadLine(); // give developer chance to attach debugger

			try
			{
				if(arguments.Filenames.Count == 0)
					throw new ApplicationException("No input file.");

				generator = null;
				if(genSupport || genUser)
				{
					if(template != null)
						throw new ApplicationException("Cannot use template and code generator at the same time.");

					CodeGenerator codeGenerator = new CodeGenerator();
					codeGenerator.ForcesUserClassGen = force;
					codeGenerator.GeneratesSupportClasses = genSupport;
					codeGenerator.GeneratesUserClasses = genUser;
					codeGenerator.DefaultNamespace = defaultNamespace;
					generator = codeGenerator;
				}
				else if(template != null)
				{
					generator = new VelocityGenerator();
					generator.ReaderType = typeof(NorqueReader);
					generator.Template = template;
				}
			
				if(generator != null)
				{
					generator.ReaderType = typeof(NorqueReader);
					if(rpath != null)
						generator.ResourcePath = rpath;

					foreach(string input in arguments.Filenames)
					{
						generator.Generate(input, opath);
					}
				}
			}
			catch(Exception e)
			{
				Console.Error.WriteLine(e.Message);
				if(debug)
					Console.WriteLine("--\n{0}", e.StackTrace);
			}
		}

	}
}
