using System;
using System.Collections;
using System.Data;


namespace Neo.Model
{
	public interface IModel
	{
		string Namespace { get; }

		ICollection Entities { get; }
	}

}
			