using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Forms;
using EnvDTE;
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
				ProjectItem	modelItem;
				IList		fileList;
				
				fileList = codeGen.GenerateClassFiles(path, null, true, false);
				modelItem = base.GetService(typeof(ProjectItem)) as ProjectItem;
				if(modelItem == null)
					throw new ApplicationException("Generated/updates files but cannot automatically add new files to project.");
				foreach(string file in fileList)
					AddFileToProjectIfNecessary(modelItem.ContainingProject, file);
			}
			catch(Exception e)
			{
				MessageBox.Show(e.Message, "NeoCodeGen");
			}

			output = System.Text.Encoding.ASCII.GetBytes(writer.ToString());
			writer.Close();
			
			return output;
		}


		private void AddFileToProjectIfNecessary(Project project, string file)
		{
			// We should probably check whether the file needs adding BUT this is not 
			// trivial (you may have to traverse folders etc.) and the AddFromFile
			// method in VS.NET 2003 does the right thing as it simply ignores superfluous 
			// adds. However, there is a remark in the docs saying it should fail if the 
			// file is in the project already...
			project.ProjectItems.AddFromFile(file);
		}

	}
}
