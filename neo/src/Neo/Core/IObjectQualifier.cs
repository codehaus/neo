namespace Neo.Core
{
	/// <summary>
	/// Some qualifier classes can evaluate against regular objects, not only 
	/// against <c>IEntityObjects</c>. These qualifiers implement <c>IObjectQualifier</c>.
	/// </summary>
	public interface IObjectQualifier
	{
		/// <summary>
		/// Performs this qualifier&apos;s action agains the supplied object
		/// </summary>
		/// <param name="anObject">object to be tested</param>
		/// <returns><c>true</c> if the object meets this qualifier&apos;s requirements</returns>
		bool EvaluateWithObject(object anObject);
	}
}
