using System.Collections;
using System.Data;


namespace Neo.Core
{
	/// <summary>
	/// Concrete implementations of this interface can be used to persist objects.
	/// </summary>
	public interface IDataStore
	{
		/// <summary>
		/// Gets rows from the underlying store matching the supplied specification.
		/// </summary>
		/// <param name="fetchSpec">An <c>IFetchSpecification</c> describing which rows to retrieve</param>
		/// <returns>A <c>DataTable</c> containing all matching rows.</returns>
		DataSet FetchRows(IFetchSpecification fetchSpec);

		/// <summary>
		/// Analyses the supplied context and pushes the changed rows back into the 
		/// underlying store.
		/// </summary>
		/// <remarks>
		/// Note that only for the 'native' primary key generation scheme, which leaves
		/// the key generation to the database, anything is returned.
		/// </remarks>
		/// <param name="context">The context to save</param>
		/// <returns>A list of <c>PkChangeTable</c> objects containing new persistent keys</returns>
		ICollection SaveChangesInObjectContext(ObjectContext context);
	}

}
