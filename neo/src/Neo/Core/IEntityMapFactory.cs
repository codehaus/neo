using System;
using System.Collections;


namespace Neo.Core
{
	public interface IEntityMapFactory
	{
		ICollection GetAllMaps();
		IEntityMap GetMap(Type type);
		IEntityMap GetMap(string tablename);
	}

}
