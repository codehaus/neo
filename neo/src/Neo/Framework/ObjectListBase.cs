using System;
using System.Collections;
using System.Globalization;

using Neo.Core;


namespace Neo.Framework
{
	/// <summary>
	/// Summary description for ObjectListBase.
	/// </summary>
	public abstract class ObjectListBase : ObjectCollectionBase
	{
		//--------------------------------------------------------------------------------------
		//	Fields
		//--------------------------------------------------------------------------------------

		protected bool  isReadOnly;


		//--------------------------------------------------------------------------------------
		// Remaining IList impl 
		//--------------------------------------------------------------------------------------
	
		public override bool IsReadOnly
		{
			get { return isReadOnly; }
		}

		
		//--------------------------------------------------------------------------------------
		// Other public methods that deal with the entire list
		//--------------------------------------------------------------------------------------

		public virtual void MakeReadOnly()
		{
			isReadOnly = true;
		}

		
		public virtual void Sort(IComparer comparer)
		{
			AssertIsMutable();
			((ArrayList)InnerList).Sort(comparer);
		}


		public virtual void Sort(string propName, SortDirection dir)
		{
			AssertIsMutable();
			((ArrayList)InnerList).Sort(new PropertyComparer(propName, dir, CultureInfo.CurrentCulture));
		}


	}
}
