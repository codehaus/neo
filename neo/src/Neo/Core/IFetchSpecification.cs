using System;


namespace Neo.Core
{
	public interface IFetchSpecification
	{
		IEntityMap EntityMap { get; }
		Qualifier Qualifier { get; }
		Int32 FetchLimit { get; }
	}

}
