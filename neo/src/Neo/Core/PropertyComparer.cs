using System;
using System.Reflection;
using System.Collections;
using System.Globalization;
using Neo.Core.Util;


namespace Neo.Core
{
	/// <summary>
	/// Determines the direction of sorting, and case sensitivity
	/// </summary>
	public enum SortDirection
	{
		/// <summary> Ascending, case-sensitive alphabetic sort</summary>
		Ascending					= 0x00,
		/// <summary> Decending, case-sensitive alphabetic sort</summary>
		Descending					= 0x01,
		/// <summary> Ascending, non case-sensitive alphabetic sort</summary>
		AscendingCaseInsensitive	= 0x02,
		/// <summary> Decending, non case-sensitive alphabetic sort</summary>
		DescendingCaseInsensitive   = 0x03
	}


	/// <summary>
	/// Concrete <c>IComparer</c> used to compare properties between EntityObject instances
	/// </summary>
	/// <remarks>
	/// This comparer will only compare objects by a single property.
	/// Multiple property comparisons are not supported
	/// </remarks>
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

		/// <summary>
		/// Constructor. Sets property name and sort direction to be used in subsequent sorts
		/// </summary>
		/// <param name="aPropName">Name of property value to compare</param>
		/// <param name="aDirection">Sort direction and case matching</param>
		public PropertyComparer(string aPropName, SortDirection aDirection)
		{
			propName = aPropName;
			direction = aDirection;
			if((aDirection & IgnoreCase) == 0)
				comparer = Comparer.Default;
			else
				comparer = CaseInsensitiveComparer.Default;
		}


		/// <summary>
		/// Constructor. Sets property name and sort direction to be used in subsequent sorts
		/// </summary>
		/// <param name="aPropName">Name of property value to compare</param>
		/// <param name="aDirection">Sort direction and case matching</param>
		/// <param name="culture">Culture used when performing string comparisons</param>
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

		/// <summary>
		/// Name of property to compare when sorting
		/// </summary>
		public string Property
		{
			get { return propName; }
		}

		
		/// <summary>
		/// Sort direction (Ascending, Descending)
		/// </summary>
		public SortDirection SortDirection
		{
			get { return direction; }
		}


		//--------------------------------------------------------------------------------------
		//	IComparer impl
		//--------------------------------------------------------------------------------------

		/// <summary>
		/// Compares one <c>EntityObject</c> instance with another using the value of <c>Property</c>
		/// </summary>
		/// <param name="x">First object to compare</param>
		/// <param name="y">Second object to compare</param>
		/// <returns>
		/// <c>0</c>, if the objects are the same,
		/// <c>1</c>, if x &gt; y,
		/// <c>-1</c> if x &gt; y
		/// </returns>
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
	
		/// <summary>
		/// Uses <c>ArrayList</c>&apos; QuickSort implementation to sort the supplied list
		/// </summary>
		/// <param name="list">list to be sorted</param>
		public void Sort(ArrayList list)
		{
			list.Sort(this);
		}


	}
}
