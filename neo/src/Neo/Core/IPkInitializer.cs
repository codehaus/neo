using System.Data;


namespace Neo.Core
{
	/// <summary>
	/// Contains a single method which must initialise a new database row. 
	/// If PK generation scheme involves PK values provided by the application they 
	/// are passed in.
	/// </summary>
	public interface IPkInitializer
	{
		/// <summary>
		/// Sets up primary key for the supplied row
		/// </summary>
		/// <param name="row">row requiring primary key</param>
		/// <param name="argument">extra information needed to set primary key values</param>
		void InitializeRow(DataRow row, object argument); 
	}

}
