namespace Neo.SqlGen 
{
	using System;
	using System.IO;
	using System.Xml;
	using System.Xml.Xsl;

	public class SqlDropGenerator 
	{
		string _resourcePath = "";
		string _outputPath = "";

		public string ResourcePath
		{
			get{ return _resourcePath; }
			set{ _resourcePath = value; }
		}

		public string OutputPath 
		{
			get { return _outputPath; }
			set { _outputPath = value; }
		}

		public void GenerateSqlFile(string inputPath) 
		{
			string inputFile = Path.GetFileName(inputPath) ;
			string outputFile = Path.ChangeExtension(inputFile, "drop.sql");

			StreamWriter writer = File.CreateText(Path.Combine(_outputPath, outputFile));
			writer.Write(ProcessFile(inputPath));
			writer.Close();
		}

		public string ProcessFile(string inputPath) 
		{
			string styleSheetFile = "NeoSchemaDropGen.xslt";

			StringWriter sqlCommand = new StringWriter();
			XmlDocument input = new XmlDocument();
			XslTransform xslt = new XslTransform();

			input.Load(inputPath);
			xslt.Load(Path.Combine(_resourcePath, styleSheetFile));

			xslt.Transform(input, /*XsltArgumentList*/ null, sqlCommand, /*XmlResolver*/ null);

			return sqlCommand.ToString();
		}
	}
}