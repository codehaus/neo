using System;


namespace Neo.Core
{
	public interface IEntityObject
	{
		ObjectContext Context { get; }
		System.Data.DataRow Row { get; }
	}


}
