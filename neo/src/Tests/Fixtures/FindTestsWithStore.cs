using Neo.Core;


namespace Neo.Tests.Fixtures
{
	[NUnit.Framework.TestFixture]
	public class FindTestsWithStore : FindTests
	{

		protected override ObjectContext GetContext()
		{
			return new ObjectContext(GetDataStore());
		}


	}
}
