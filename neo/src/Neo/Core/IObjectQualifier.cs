using System;

namespace Neo.Core
{
	public interface IObjectQualifier
	{
		bool EvaluateWithObject(object anObject);
	}
}
