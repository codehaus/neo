using System;
using Neo.Core;
using NUnit.Framework;
using Pubs4.Model;


namespace Neo.Tests
{
	[TestFixture]
	public class ScenarioTestsWithStore : ScenarioTests
	{

		protected override ObjectContext GetContext()
		{
			return new ObjectContext(GetDataStore());
		}

		
		protected override DateTime GetDate(int y, int m, int d)
		{
			return new DateTime(y, m, d);
		}


	}

}
