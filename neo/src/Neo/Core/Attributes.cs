using System;

namespace Neo.Core
{
	/// <summary>
	/// Marks a method used during <c>IEntityObject</c> creation
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
