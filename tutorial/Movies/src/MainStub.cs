using System;
using System.IO;
using System.Windows.Forms;
using log4net.Config;
using Movies.Forms;
using Neo.Core;
using Neo.SqlClient;


namespace Movies
{
	public class MainStub
	{
		[STAThread]
		static void Main() 
		{
			FileInfo fi = new FileInfo("..\\..\\log4net.config");
			if(fi.Exists == false)
				throw new ApplicationException("Cannot find log4net configuration file at " + fi.FullName);
			DOMConfigurator.Configure(fi);

			IDataStore store = new SqlDataStore("data source=VIRTSERVER;initial catalog=Movies;user id=dev;password=passw0rd");
			ObjectContext mainContext = new ObjectContext(store);
			MainWindowForm mainForm = new MainWindowForm(mainContext);

			Application.Run(mainForm);
		}
	}
}
