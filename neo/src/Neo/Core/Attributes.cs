using System;


namespace Neo.Core
{
	/// <summary>
	/// Marks a method to be called after the creation of an <c>IEntityObject</c>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class LifecycleCreateAttribute : Attribute
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public LifecycleCreateAttribute() : base()
		{
		}
		
	}
}
