using System;

using Mono.GetOptions;

namespace Neo.CmdLineTool {
	public class CmdLineOptions : Options
    {
        private bool debug;
        private bool force;
        private bool generateUser;
        private bool generateSupport;
        private string defaultNamespace;
        private string resourcePath;
        private string outputPath;
	    private string template;

	    [Option("Enable debug mode", 'd', "debug")]
	    public bool Debug
        {
	        get { return debug; }
	        set { debug = value; }
	    }

        [Option("Force output", 'f', "force")]
	    public bool Force
        {
	        get { return force; }
	        set { force = value; }
	    }

        [Option("Generate user classes", "user")]
	    public bool GenerateUser
        {
	        get { return generateUser; }
	        set { generateUser = value; }
	    }

        [Option("Generate support classes", "support")]
	    public bool GenerateSupport 
        {
	        get { return generateSupport; }
	        set { generateSupport = value; }
	    }

        [Option("The namespace for generated class files", "namespace")]
	    public string DefaultNamespace
        {
	        get { return defaultNamespace; }
	        set { defaultNamespace = value; }
	    }

        [Option("The directory that contains the .vtl templates", 'r', "resources")]
	    public string ResourcePath {
	        get { return resourcePath; }
	        set { resourcePath = value; }
	    }

        [Option("The output directory to create files in", 'o', "out")]
        public string OutputPath {
	        get { return outputPath; }
	        set { outputPath = value; }
	    }

        [Option("The template to use for generation", 't', "template")]
        public string Template {
            get { return template; }
            set { template = value; }
            }
    }
}
