using System;
using System.Collections;
using System.Data;


namespace Neo.Core
{
	public interface IDataStore
	{
		DataTable FetchRows(IFetchSpecification fetchSpec);
		ICollection SaveChangesInObjectContext(ObjectContext context);
	}

}
