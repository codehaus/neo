using System;

namespace Neo.Core
{
	/// <summary>
	/// Some qualifier classes can evaluate against regular obejcts as well as 
	/// against <c>IEntityObjects</c>.
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
