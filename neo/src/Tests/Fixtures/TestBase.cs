using System;
using System.Data;
using System.Xml;
using System.IO;
using NUnit.Framework;
using Neo.Core;
using Neo.SqlClient;


namespace Neo.Tests
{
	public class TestBase
	{
		static bool	didSetupLog4Net;


		protected void SetupLog4Net()
		{
			if(didSetupLog4Net)
				return;
			System.IO.FileInfo fi = new System.IO.FileInfo("..\\..\\log4net.config");
			if(fi.Exists == false)
				throw new ApplicationException("Cannot find log4net configuration file at " + fi.FullName);
			log4net.Config.DOMConfigurator.ConfigureAndWatch(fi);
			didSetupLog4Net = true;
		}


		protected string GetConfigValue(string name)
		{
			XmlDocument config = new XmlDocument();
			config.Load("..\\..\\test.config");
			return config.DocumentElement.GetElementsByTagName(name)[0].InnerText;
		}

		
		protected DataSet GetTestDataSet()
		{
			DataSet dataset = new DataSet();
			dataset.ReadXml("..\\..\\TestData.xml", XmlReadMode.ReadSchema);
			// Loading a dataset that is not a diffgram will mark all rows as added;
			// which is not what we want. Therefore, we need to call AcceptChanges()
			dataset.AcceptChanges();
			return dataset;
		}


		protected SqlDataStore GetDataStore()
		{
			return new SqlDataStore(GetConfigValue("sqlconnstring"));
		}

	}
}
