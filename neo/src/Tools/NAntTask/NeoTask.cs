using System;
using System.IO;
using System.Text;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

using Neo.Generator.CodeGen;
using Neo.Generator.Core;
using Neo.MetaModel.Reader;

namespace Neo.NAntTasks {
    [TaskName("neo")]
	public class NeoTask : Task {
        private bool force;
        private bool generateUser;
        private bool generateSupport;
        private string defaultNamespace;
        private string resourcePath;
        private string outputPath;
        private string template;
        private FileSet schemas = new FileSet();

        [TaskAttribute("force")]
        [BooleanValidator]
        public bool Force {
            get { return force; }
            set { force = value; }
        }

        [TaskAttribute("user")]
        [BooleanValidator]
        public bool GenerateUser {
            get { return generateUser; }
            set { generateUser = value; }
        }

        [TaskAttribute("support")]
        [BooleanValidator]
        public bool GenerateSupport {
            get { return generateSupport; }
            set { generateSupport = value; }
        }
        
        [TaskAttribute("namespace")]
        [StringValidator(AllowEmpty=false)]
        public string DefaultNamespace {
            get { return defaultNamespace; }
            set { defaultNamespace = value; }
        }

        [BuildElement("schemas", Required=true)]
        public FileSet Schemas {
            get { return schemas; }
            set { schemas = value; }
        }

        [TaskAttribute("resources")]
        [StringValidator(AllowEmpty=false)]
        public string ResourcePath {
            get { return resourcePath; }
            set { resourcePath = value; }
        }

        [TaskAttribute("out")]
        [StringValidator(AllowEmpty=false)]
        public string OutputPath {
            get { return outputPath; }
            set { outputPath = value; }
        }

        [TaskAttribute("template")]
        [StringValidator(AllowEmpty=false)]
        public string Template {
            get { return template; }
            set { template = value; }
        }

        protected override void ExecuteTask() {
            VelocityGenerator generator = null;
            if (OutputPath == null) 
                OutputPath = Project.BaseDirectory;

            if(Schemas.FileNames.Count == 0)
                throw new BuildException("No schemas specified");

            if(GenerateSupport || GenerateUser)
            {
                if(Template != null)
                    throw new BuildException("Cannot use template and code generator at the same time.");

                CodeGenerator codeGenerator = new CodeGenerator();
                codeGenerator.ForcesUserClassGen = Force;
                codeGenerator.GeneratesSupportClasses = GenerateSupport;
                codeGenerator.GeneratesUserClasses = GenerateUser;
                codeGenerator.DefaultNamespace = DefaultNamespace;
                generator = codeGenerator;
            }
            if(generator != null)
            {
                generator.ReaderType = typeof(NorqueReader);
                if(ResourcePath != null)
                    generator.ResourcePath = ResourcePath;

                string outputMessage = GenerateOutputMessage();

                Log(Level.Info, LogPrefix + "Generating {0} for {1} schema{2}.", outputMessage, Schemas.FileNames.Count, (Schemas.FileNames.Count != 1) ? "s" : "");

                Console.Out.Flush();
                TextWriter oldStdOut = Console.Out;
                TextWriter stdOut = new StringWriter();
                Console.SetOut(stdOut);
                try 
                {
                    foreach (string input in Schemas.FileNames) 
                    {
                        generator.Generate(input, OutputPath);
                    }
                } 
                catch (Exception e)
                {
                    throw new BuildException(String.Format("Could not generate {0} for schema{1}", outputMessage, (Schemas.FileNames.Count != 1) ? "s" : ""), e);
                        
                }
                finally
                {
                    Console.SetOut(oldStdOut);
                    stdOut.Close();
                    DumpOutput(stdOut.ToString());
                }

                

            } 
            else 
            {
                throw new BuildException(@"Specify either user=""true"", support=""true"", or template=""true""");
            }
        }

        private string GenerateOutputMessage() {
            if (GenerateUser && GenerateSupport) 
            {
                return "user and support classes";
            }
            else if (GenerateUser) 
            {
                return "user classes";
            } 
            else if (GenerateSupport) 
            {
                return "support classes";
            } 
            else if (Template != null) 
            {
                return "template output";
            }
            else
            {
                return "";
            }
        }

        private void DumpOutput(string stdOut)
        {
            if (Verbose)
            {
                string[] lines = stdOut.Split('\n','\r');
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (line.Length > 0)
                    {
                        Log(Level.Verbose, LogPrefix + line);
                    }
                }
            }
        }
	}
}
