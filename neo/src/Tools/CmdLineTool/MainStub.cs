using System;
using System.Collections;
using Neo.Generator.CodeGen;
using Neo.Generator.Core;
using Neo.MetaModel.Reader;
using Neo.SqlGen;


namespace Neo.CmdLineTool
{
	class MainStub
	{
		[STAThread]
		static void Main(string[] args)
		{
			CmdLineArguments	arguments;
		    VelocityGenerator	generator;
		    string				rpath, opath, template;
			bool				force, debug, genSql, genSqlDrop, genSupport, genUser;

			arguments = new CmdLineArguments(args);
			force = (arguments["f"] == "true");
			debug = (arguments["d"] == "true");
			genSql = (arguments["sql"] == "true");
			genSqlDrop = (arguments["sqldrop"] == "true");
			genSupport = (arguments["support"] == "true");
			genUser = (arguments["user"] == "true");
			template = arguments["t"];
			rpath = arguments["r"];
			opath = arguments["o"];
			
			if(debug)
				Console.ReadLine(); // give developer chance to attach debugger

			try
			{
				if(arguments.Filenames.Count == 0)
					throw new ApplicationException("No input file.");

				/* These are here for compatibility reasons. If you can use the velocity generator with a SQL DDL template instead. */
				if(genSql)
					GenSql(rpath, opath, arguments.Filenames);
				if(genSqlDrop)
					GenSqlDrop(rpath, opath, arguments.Filenames);
				
				generator = null;
				if(genSupport || genUser)
				{
					if(template != null)
						throw new ApplicationException("Cannot use template and code generator at the same time.");

					CodeGenerator codeGenerator = new CodeGenerator();
					codeGenerator.ForcesUserClassGen = force;
					codeGenerator.GeneratesSupportClasses = genSupport;
					codeGenerator.GeneratesUserClasses = genUser;
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

		
		
		public static void GenSql(string rpath, string opath, IList filenames)
		{
		    SqlGenerator sqlGenerator = new SqlGenerator();
			if(rpath != null)
				sqlGenerator.ResourcePath = rpath;
			if(opath != null)
				sqlGenerator.OutputPath = opath;
			foreach(string f in filenames)
				sqlGenerator.GenerateSqlFile(f);
		}

		public static void GenSqlDrop(string rpath, string opath, IList filenames)
		{
			SqlDropGenerator sqlDropGenerator = new SqlDropGenerator();
			if(rpath != null)
				sqlDropGenerator.ResourcePath = rpath;
			if(opath != null)
				sqlDropGenerator.OutputPath = opath;
			foreach(string f in filenames)
				sqlDropGenerator.GenerateSqlFile(f);
		}

	}
}
