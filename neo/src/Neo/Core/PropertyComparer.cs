using System;
using System.Reflection;
using System.Collections;
using System.Globalization;
using Neo.Core.Util;


namespace Neo.Core
{
	public enum SortDirection
	{
		Ascending					= 0x00,
		Descending					= 0x01,
		AscendingCaseInsensitive	= 0x02,
		DescendingCaseInsensitive   = 0x03
	}


	public class PropertyComparer : IComparer
	{
		protected const SortDirection InvertDirection = SortDirection.Descending;
		protected const SortDirection IgnoreCase	  = SortDirection.AscendingCaseInsensitive;


		//--------------------------------------------------------------------------------------
		//	Fields and constructor
		//--------------------------------------------------------------------------------------

		private string		   propName;
		private SortDirection  direction;
		private IComparer	   comparer;
		private PropertyInfo   propInfo;
		private Type		   lastType;

		public PropertyComparer(string aPropName, SortDirection aDirection)
		{
			propName = aPropName;
			direction = aDirection;
			if((aDirection & IgnoreCase) == 0)
				comparer = Comparer.Default;
			else
				comparer = CaseInsensitiveComparer.Default;
		}


		public PropertyComparer(string aPropName, SortDirection aDirection, CultureInfo culture)
		{
			propName = aPropName;
			direction = aDirection;
			if((aDirection & IgnoreCase) == 0)
				comparer = new Comparer(culture);
			else
				comparer = new CaseInsensitiveComparer(culture);
		}


		//--------------------------------------------------------------------------------------
		//	Public properties
		//--------------------------------------------------------------------------------------

		public string Property
		{
			get { return propName; }
		}

		
		public SortDirection SortDirection
		{
			get { return direction; }
		}


		//--------------------------------------------------------------------------------------
		//	IComparer impl
		//--------------------------------------------------------------------------------------

		public virtual int Compare(object x, object y)
		{
			object xval = ObjectHelper.GetProperty(x, propName, ref lastType, ref propInfo);
			object yval = ObjectHelper.GetProperty(y, propName, ref lastType, ref propInfo);

			if((direction & InvertDirection) == 0)
				return comparer.Compare(xval, yval);
			else
				return comparer.Compare(yval, xval);
		}

			
		//--------------------------------------------------------------------------------------
		//	Use comparer
		//--------------------------------------------------------------------------------------
	
		public void Sort(ArrayList list)
		{
			list.Sort(this);
		}


	}
}
