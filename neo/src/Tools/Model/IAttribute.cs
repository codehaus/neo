using System;
using System.Collections;
using System.Data;


namespace Neo.Model
{

	public interface IAttribute
	{
		string ColumnName { get; }
		
		string PropertyName { get; }
		Type PropertyType { get; }
		string PropertyAttributes { get; }
		
		bool IsPkColumn { get; }
		string Size { get; }
		bool AllowsNull { get; }
		bool IsHidden { get; }
		bool IsRequired { get; }

	}

}