using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Forms;
using Neo.Generator.CodeGen;


namespace Neo.VsTool
{
	[Guid("3A95AC49-6FE5-4841-A12A-16C4FBAFBB8D")]
	public class CodeGenAdaptor : CustomToolGenerator.BaseCodeGeneratorWithSite
	{
		protected override byte[] GenerateCode(string path, string contents)
		{
			StringWriter	writer;
			CodeGenerator	codeGen;
			byte[]			output;

			writer = new StringWriter();
			codeGen = new CodeGenerator();
			codeGen.ReaderType = typeof(Neo.MetaModel.Reader.NorqueReader);

			try
			{
				codeGen.GenerateSupportClasses(path, writer);
			}
			catch(Exception e)
			{
				MessageBox.Show(e.Message, "NeoCodeGen");
				writer.WriteLine("/* NeoCodeGen caught an exception while generating the source code: {0}\n\n{1}\n*/", e.Message, e.StackTrace);
			}
			try
			{
				codeGen.GenerateClassFiles(path, null, true, false);
			}
			catch(Exception e)
			{
				MessageBox.Show(e.Message, "NeoCodeGen");
			}

			output = System.Text.Encoding.ASCII.GetBytes(writer.ToString());
			writer.Close();
			
			return output;
		}
	
	}
}
