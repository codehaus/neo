using System;
using Neo.Core;


namespace Neo.Tests.Fixtures
{
	[NUnit.Framework.TestFixture]
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
