using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using CustomToolGenerator;
using EnvDTE;
using Neo.Generator.CodeGen;
using Neo.MetaModel.Reader;


namespace Neo.VsTool
{
	[Guid("3A95AC49-6FE5-4841-A12A-16C4FBAFBB8D")]
	public class CodeGenAdaptor : BaseCodeGeneratorWithSite
	{
		protected override byte[] GenerateCode(string path, string contents)
		{
			StringWriter writer;
			CodeGenerator codeGen;
			byte[] output;

			writer = new StringWriter();
			codeGen = new CodeGenerator();
			codeGen.ReaderType = typeof(NorqueReader);

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
				ProjectItem modelItem;
				IList fileList;

				fileList = codeGen.GenerateClassFiles(path, null, true, false);
				modelItem = base.GetService(typeof(ProjectItem)) as ProjectItem;
				if(modelItem == null)
					throw new ApplicationException("Generated/updates files but cannot automatically add new files to project.");
				foreach (string file in fileList)
					AddFileToProjectIfNecessary(modelItem.ContainingProject, file);
			}
			catch(Exception e)
			{
				MessageBox.Show(e.Message, "NeoCodeGen");
			}

			output = Encoding.ASCII.GetBytes(writer.ToString());
			writer.Close();

			return output;
		}


		private void AddFileToProjectIfNecessary(Project project, string filename)
		{
			if(ItemsContainFile(project.ProjectItems, filename) == false)
				project.ProjectItems.AddFromFile(filename);
		}


		private bool ItemsContainFile(ProjectItems items, string filename)
		{
			foreach(ProjectItem projectItem in items)
			{
				if(IsFolder(projectItem))
					return ItemsContainFile(projectItem.ProjectItems, filename);
				else
					return ProjectItemRepresentsFile(projectItem, filename);
			}
			return false;
		}


		// if match check document full name in case it's the right filename wrong folder
		// NOTE: accessing the document can cause a COM exception
		private bool ProjectItemRepresentsFile(ProjectItem projectItem, string filename)
		{
			if(filename.ToLower() == projectItem.Name.ToLower())
			{
				try
				{
					Document doc = projectItem.Document;
					if(doc != null && doc.FullName != null)
					{
						if(doc.FullName.ToLower() == doc.FullName.ToLower())
							return true;
					}
				}
				catch(COMException)
				{
					// ignore it
				}
			}
			return false;
		}

		public bool IsFolder(ProjectItem item)
		{
			string[] folderGUIDs =
				{
					"{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}", // GUID for VB, C#, and J# folders
					"{6BB5F8F0-4483-11D3-8BCF-00C04F8EC28C}"  // GUID for VC folders
				};
			foreach(string guid in folderGUIDs)
			{
				if(item.Kind.ToUpper() == guid)
					return true;
			}
			return false;
		}
	}

}
