using System;
using System.Data;


namespace Neo.Core
{
	/// <summary>
	/// Contains a single method which must initialise a new database row. 
	/// If scheme involves pk values provided by the application they are passed in.
	/// <code>
	/// public interface IPkInitializer {
	///	    void InitializeRow(DataRow row, object arg);
	///	}
	///	</code>
	/// </summary>
	public interface IPkInitializer
	{
		/// <summary>
		/// Sets up primary key(s) for the supplied row
		/// </summary>
		/// <param name="row">row requiring primary key(s)</param>
		/// <param name="argument">extra information needed to set primary key values</param>
		void InitializeRow(DataRow row, object argument); 
	}

}
