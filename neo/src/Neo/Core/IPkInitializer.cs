using System;
using System.Data;


namespace Neo.Core
{
	public interface IPkInitializer
	{
		void InitializeRow(DataRow row, object argument); 
	}

}
