using System;
using System.IO;
using log4net.Config;
using NUnit.Framework;


namespace Movies.Tests
{
	[TestFixture]
	public class TestBase
	{
		static bool	didSetupLog4Net;

		[SetUp]
		public void SetupLog4Net()
		{
			if(didSetupLog4Net)
				return;
			FileInfo fi = new FileInfo("..\\..\\log4net.config");
			if(fi.Exists == false)
				throw new ApplicationException("Cannot find log4net configuration file at " + fi.FullName);
			DOMConfigurator.Configure(fi);
			didSetupLog4Net = true;
		}

	}
}
