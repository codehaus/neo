using System;
using Neo.Core;
using Neo.Core.Util;
using NUnit.Framework;
using Pubs4.Model;


namespace Neo.Tests.Fixtures
{
	[Ignore("[MANUAL] Speed tests only.")]
	public class SpeedTests
	{

		[Test]
		public void SpeedOfGetProperty()
		{
			ObjectContext context;
			Title		  title;

			context = new ObjectContext();
			title = new TitleFactory(context).CreateObject("XX9999");
	
			int count = 100000;
			DateTime start = DateTime.Now;
			for(int i = 0; i < count; i++)
				ObjectHelper.GetProperty(title, "TheTitle");
			DateTime end = DateTime.Now;
			
			Console.WriteLine("speed = {0} GetProperty/s", GetSpeed(start, end, count));
		}


		[Test]
		public void SpeedOfPropertyQualifier()
		{
			ObjectContext context;
			TitleFactory  factory;

			context = new ObjectContext();
			factory = new TitleFactory(context);
			
			for(int i = 0; i < 100000; i++)
				factory.CreateObject(i.ToString());

			Qualifier q = Qualifier.Format("TitleId = 99");
			int count = 100;
			DateTime start = DateTime.Now;
			for(int i = 0; i < count; i++)
			{
				factory.Find(q);
			}
			DateTime end = DateTime.Now;
			
			Console.WriteLine("speed = {0} PropQualifier/s", GetSpeed(start, end, count));
		}
	

		[Test]
		public void SpeedOfPropertyQualifierWithOtherObjects()
		{
			ObjectContext	 context;
			TitleFactory	 titleFactory;
			PublisherFactory pubFactory;

			context = new ObjectContext();

			titleFactory = new TitleFactory(context);
			for(int i = 0; i < 1000; i++)
				titleFactory.CreateObject(i.ToString());

			pubFactory = new PublisherFactory(context);
			for(int i = 0; i < 99000; i++)
				pubFactory.CreateObject(i.ToString());

			Console.WriteLine("Starting test.");

			Qualifier q = Qualifier.Format("TitleId = 99");
			int count = 100;
			DateTime start = DateTime.Now;
			for(int i = 0; i < count; i++)
			{
				titleFactory.Find(q);
			}
			DateTime end = DateTime.Now;
			
			Console.WriteLine("speed = {0} PropQualifier2/s", GetSpeed(start, end, count));
		}


		private static double GetSpeed(DateTime start, DateTime end, int count)
		{
			return ((double)count)/(end - start).TotalMilliseconds * 1000;
		}

	}
}
