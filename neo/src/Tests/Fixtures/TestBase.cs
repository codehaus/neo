using System;
using System.Data;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using log4net.Config;
using Neo.Core;
using NUnit.Framework;


namespace Neo.Tests.Fixtures
{
	public class TestBase
	{
		static bool	didSetupLog4Net;
		
		string resourcePath;

		[TestFixtureSetUp]
		public void SetUpResourcePath()
		{
			resourcePath = "..\\..";
		}


		protected void SetupLog4Net()
		{
			if(didSetupLog4Net)
				return;

			FileInfo fi = new FileInfo(Path.Combine(resourcePath, "log4net.config"));
			if(fi.Exists == false)
				throw new ApplicationException("Cannot find log4net configuration file at " + fi.FullName);
		    DOMConfigurator.ConfigureAndWatch(fi);
			didSetupLog4Net = true;
		}


		protected string GetConfigValue(string name)
		{
		    XmlDocument config = new XmlDocument();
			config.Load(Path.Combine(resourcePath, "test.config"));
			return config.DocumentElement.GetElementsByTagName(name)[0].InnerText;
		}

		
		protected DataSet GetTestDataSet()
		{
			DataSet dataset = new DataSet();
			dataset.ReadXml(Path.Combine(resourcePath, "TestData.xml"), XmlReadMode.ReadSchema);
			// Loading a dataset that is not a diffgram will mark all rows as added;
			// which is not what we want. Therefore, we need to call AcceptChanges()
			dataset.AcceptChanges();
			return dataset;
		}


		protected IDataStore GetDataStore()
		{
			String storeClassName = GetConfigValue("store");
			String connString = GetConfigValue("sqlconnstring");

			return (IDataStore)Activator.CreateInstance(Type.GetType(storeClassName), new object[] { connString });
		}


		protected object RunThroughSerialization(object original)
		{
		    IFormatter	formatter;
			Stream		stream;
			object		deserialised;
			
			stream = new MemoryStream();
			formatter = new BinaryFormatter();
			formatter.Serialize(stream, original);
			stream.Seek(0, SeekOrigin.Begin);
			formatter = new BinaryFormatter();
			deserialised = formatter.Deserialize(stream);
			stream.Close();

			return deserialised;
		}


		protected object CreateInstance(Type type, params object[] args)
		{
			BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
			return Activator.CreateInstance(type, flags, null, args, null);
		}


		protected DataRow AddRow(DataSet ds, string tablename, params object[] values)
		{
			return AddRow(ds.Tables[tablename], values);
		}

		protected DataRow AddRow(DataTable table, params object[] values)
		{
			DataRow row = table.NewRow();
			for(int i = 0; i < values.Length; i += 2)
				row[(string)values[i]] = values[i+1];
			table.Rows.Add(row);
			return row;
		}



	}
}
