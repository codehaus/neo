using System;
using Neo.CodeGen.Core;
using Neo.SqlGen;


namespace Neo.CmdLineTool
{
	class MainStub
	{
		[STAThread]
		static void Main(string[] args)
		{
			CmdLineArguments	arguments;
			CodeGenerator		codeGenerator;
			SqlGenerator		sqlGenerator;
			SqlDropGenerator	sqlDropGenerator;
			string				rpath, opath;
			bool				force, debug, genSql, genSqlDrop, genSupport, genUser;

			arguments = new CmdLineArguments(args);
			force = (arguments["f"] == "true");
			debug = (arguments["d"] == "true");
			genSql = (arguments["sql"] == "true");
			genSqlDrop = (arguments["sqldrop"] == "true");
			genSupport = (arguments["support"] == "true");
			genUser = (arguments["user"] == "true");
			rpath = arguments["r"];
			opath = arguments["o"];
			
			if(debug)
				Console.ReadLine(); // give developer chance to attach debugger

			try
			{
				if(arguments.Filenames.Count == 0)
					throw new ApplicationException("No input file.");

				codeGenerator = null;
				if(genSupport || genUser)
				{
					codeGenerator = new CodeGenerator();
					codeGenerator.ReaderType = typeof(Neo.Model.Reader.NorqueReader);
					if(rpath != null)
						codeGenerator.ResourcePath = rpath;
					if(opath != null) 
						codeGenerator.OutputPath = opath;
					codeGenerator.ForceUserClassGen = force;
				}

				sqlGenerator = null;
				if(genSql)
				{
					sqlGenerator = new SqlGenerator();
					if(rpath != null)
						sqlGenerator.ResourcePath = rpath;
					if(opath != null)
						sqlGenerator.OutputPath = opath;
				}

				sqlDropGenerator = null;
				if(genSqlDrop)
				{
					sqlDropGenerator = new SqlDropGenerator();
					if(rpath != null)
						sqlDropGenerator.ResourcePath = rpath;
					if(opath != null)
						sqlDropGenerator.OutputPath = opath;
				}

				foreach(string filename in arguments.Filenames)
				{
					if(genUser)
						codeGenerator.GenerateUserClassFiles(filename);
					if(genSupport)
						codeGenerator.GenerateSupportClassFiles(filename);
					if(genSql)
						sqlGenerator.GenerateSqlFile(filename);
					if(genSqlDrop)
						sqlDropGenerator.GenerateSqlFile(filename);
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
