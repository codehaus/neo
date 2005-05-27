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
			
			DateTime start = DateTime.Now;
			int count1 = 100000;
			for(int i = 0; i < count1; i++)
				factory.CreateObject(i.ToString());
			DateTime end = DateTime.Now;

			Console.WriteLine("speed = {0} ObjectCreations/s", GetSpeed(start, end, count1));

			Qualifier q = Qualifier.Format("TitleId = 99");
			int count = 100;
			start = DateTime.Now;
			for(int i = 0; i < count; i++)
			{
				factory.Find(q);
			}
			end = DateTime.Now;
			
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


		[Test]
		public void SpeedOfAssignments()
		{
			for(int k = 5; k < 36; k+= 10)
			{
				int publisherCount = 1000;
				int titlesPerPublisher = k;

				ObjectContext context = new ObjectContext();

				TitleFactory titleFactory = new TitleFactory(context);
				for(int i = 0; i < publisherCount * titlesPerPublisher; i++)
					titleFactory.CreateObject(i.ToString());

				PublisherFactory pubFactory = new PublisherFactory(context);
				for(int i = 0; i < publisherCount; i++)
					pubFactory.CreateObject(i.ToString());

				TitleList titles = titleFactory.FindAllObjects();
				PublisherList publishers = pubFactory.FindAllObjects();
				Random r = new Random();
			
				Console.WriteLine("Starting tests for {0} titles per publisher.", titlesPerPublisher);

				int count = 100000;
				DateTime start = DateTime.Now;
				for(int i = 0; i < count; i++)
				{
					int index = r.Next(0, titles.Count);
					titles[index].TheTitle = "FOOBAR";
				}
				DateTime end = DateTime.Now;
				Console.WriteLine("{0}", GetSpeed(start, end, count));

				start = DateTime.Now;
				for(int i = 0; i < publisherCount; i++)
					publishers[i].Titles.Touch();
				end = DateTime.Now;
				Console.WriteLine("{0}", GetSpeed(start, end, publisherCount));

				start = DateTime.Now;
				for(int i = 0; i < publisherCount; i++)
					for(int j = 0; j < titlesPerPublisher; j++)
						titles[i*titlesPerPublisher+j].Publisher = publishers[i];
				end = DateTime.Now;
				Console.WriteLine("{0}", GetSpeed(start, end, publisherCount * titlesPerPublisher));
			
				count = 10000;
				start = DateTime.Now;
				for(int i = 0; i < count; i++)
				{
					int index = r.Next(0, titles.Count);
					titles[index].TheTitle = "FOOBAR";
				}
				end = DateTime.Now;
				Console.WriteLine("{0}", GetSpeed(start, end, count));
			}
		}


		private static double GetSpeed(DateTime start, DateTime end, int count)
		{
			return ((double)count)/(end - start).TotalMilliseconds * 1000;
		}

		
		[STAThread]
		static void Main() 
		{
			new SpeedTests().SpeedOfAssignments();
			Console.ReadLine();
		}


	}
}
