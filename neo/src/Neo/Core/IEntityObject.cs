using System;


namespace Neo.Core
{
	/// <summary>
	/// Minimal interface for a persistable object
	/// </summary>
	/// <remarks>
	/// The object must also implement a constructor taking an <c>ObjectContext</c> and
	/// a <c>DataRow</c> to be usable with <c>ObjectContext</c>
	/// </remarks>
	public interface IEntityObject
	{
		/// <summary>
		/// The ObjectContext object containing this <c>IEntityObject</c>
		/// </summary>
		ObjectContext Context { get; }

		/// <summary>
		/// The Row object from which properties are derived.
		/// </summary>
		/// <remarks>
		/// Modifications to this Row via object properties are persisted back into the 
		/// underlying store. It is perfectly acceptablel, while most of the time not
		/// advisable, to modify the row directly; it is guaranteed to be in sync with
		/// the object's properties.
		/// </remarks>
		System.Data.DataRow Row { get; }
	}


}
