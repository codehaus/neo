using System;

namespace Neo.Core
{
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class LifecycleCreateAttribute : Attribute
	{
		public LifecycleCreateAttribute() : base()
		{
		}
		
	}
}
