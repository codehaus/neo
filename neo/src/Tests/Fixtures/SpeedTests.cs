using System;
using Neo.Core;
using Neo.Core.Util;
using Pubs4.Model;


namespace Neo.Tests.Fixtures
{
	[NUnit.Framework.Ignore("[MANUAL] Speed tests only.")]
	public class SpeedTests
	{

		[NUnit.Framework.Test]
		public void SpeedOfGetProperty()
		{
		    ObjectContext context;
		    Title		  title;
		    DateTime	  start, end;

			context = new ObjectContext();
			title = new TitleFactory(context).CreateObject("XX9999");
		
			start = DateTime.Now;
			for(int i = 0; i < 500000; i++)
			    ObjectHelper.GetProperty(title, "TheTitle");
			end = DateTime.Now;
			
			Console.WriteLine("time spend = {0}", (end - start).TotalMilliseconds);
		}


		[NUnit.Framework.Test]
		public void SpeedOfPropertyQualifier()
		{
			ObjectContext context;
			TitleFactory  factory;
			DateTime	  start, end;

			context = new ObjectContext();
			factory = new TitleFactory(context);
			
			for(int i = 0; i < 50000; i++)
				factory.CreateObject(i.ToString());

			start = DateTime.Now;
			for(int i = 0; i < 10; i++)
			{
				factory.Find("TitleId = 99");
			}
			end = DateTime.Now;
			
			Console.WriteLine("time spend = {0}", (end - start).TotalMilliseconds);
		}
	

		[NUnit.Framework.Test]
		public void SpeedOfGetObjects()
		{
			ObjectContext	 context;
			TitleFactory	 titleFactory;
			PublisherFactory pubFactory;
			DateTime		 start, end;

			context = new ObjectContext();

			titleFactory = new TitleFactory(context);
			for(int i = 0; i < 1000; i++)
				titleFactory.CreateObject(i.ToString());

			pubFactory = new PublisherFactory(context);
			for(int i = 0; i < 99000; i++)
				pubFactory.CreateObject(i.ToString());

			start = DateTime.Now;
			Qualifier q = Qualifier.Format("TitleId = 99");
			for(int i = 0; i < 1000; i++)
			{
				titleFactory.Find(q);
			}
			end = DateTime.Now;
			
			Console.WriteLine("time spend = {0}", (end - start).TotalMilliseconds);
		}


	}
}
