using System;
using Neo.Core;
using NUnit.Framework;


namespace Neo.Tests
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
