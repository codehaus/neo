using System.Data;


namespace Neo.Core
{
	/// <summary>
	/// Interface required for objects that are managed by an <c>ObjectContext</c>
	/// </summary>
	/// <remarks>
	/// The object must also implement a constructor taking an <c>ObjectContext</c> and
	/// a <c>DataRow</c>.
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
		/// underlying store. It is perfectly acceptable, while most of the time not
		/// advisable, to modify the row directly; it is guaranteed to be in sync with
		/// the object&apos;s properties.
		/// </remarks>
		DataRow Row { get; }
	}


}
