using Neo.Core;
using NUnit.Framework;


namespace Neo.Tests.Fixtures
{
	[TestFixture]
	public class FindTestsWithStore : FindTests
	{

		protected override ObjectContext GetContext()
		{
			return new ObjectContext(GetDataStore());
		}


	}
}
