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
            CmdLineOptions      options;
		    VelocityGenerator	generator;

		    options = new CmdLineOptions();
		    options.ProcessArgs(args);
			
			if(options.Debug)
				Console.ReadLine(); // give developer chance to attach debugger

			try
			{
				if(options.RemainingArguments.Length == 0)
					throw new ApplicationException("No input file.");

				generator = null;
				if(options.GenerateSupport || options.GenerateUser)
				{
					if(options.Template != null)
						throw new ApplicationException("Cannot use template and code generator at the same time.");

				    CodeGenerator codeGenerator = new CodeGenerator();
					codeGenerator.ForcesUserClassGen = options.Force;
					codeGenerator.GeneratesSupportClasses = options.GenerateSupport;
					codeGenerator.GeneratesUserClasses = options.GenerateUser;
					codeGenerator.DefaultNamespace = options.DefaultNamespace;
					generator = codeGenerator;
				}
				else if(options.Template != null)
				{
					generator = new VelocityGenerator();
					generator.ReaderType = typeof(NorqueReader);
					generator.Template = options.Template;
				}
			
				if(generator != null)
				{
					generator.ReaderType = typeof(NorqueReader);
					if(options.ResourcePath != null)
						generator.ResourcePath = options.ResourcePath;

					foreach(string input in options.RemainingArguments)
					{
						generator.Generate(input, options.OutputPath);
					}
				} 
                else
                {
                    throw new ApplicationException("You should either specify -o, -g, or -t");
				}

			}
			catch(Exception e)
			{
				Console.Error.WriteLine(e.Message);
				if(options.Debug)
					Console.WriteLine("--\n{0}", e.StackTrace);
			}
		}

	}
}
