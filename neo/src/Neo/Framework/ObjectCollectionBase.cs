using System;
using System.Collections;
using System.ComponentModel;
using Neo.Core;
using Neo.Core.Util;

namespace Neo.Framework
{
	/// <summary>
	/// Summary description for ObjectCollectionBase.
	/// </summary>
	public abstract class ObjectCollectionBase : ICollection, IList, IBindingList
	{
		//--------------------------------------------------------------------------------------
		//	Fields and Constructor
		//--------------------------------------------------------------------------------------

		protected ObjectCollectionBase()
		{
		}


		//--------------------------------------------------------------------------------------
		//	Protected properties and methods
		//--------------------------------------------------------------------------------------

		protected abstract IList InnerList
		{
			get;
		}

		protected void AssertIsMutable()
		{
			if(IsReadOnly)
				throw new InvalidOperationException("Cannot modify a read-only collection");
		}


		//--------------------------------------------------------------------------------------
		//	IEnumerable Impl
		//--------------------------------------------------------------------------------------

		public virtual IEnumerator GetEnumerator()
		{
			return InnerList.GetEnumerator();
		}

		
		//--------------------------------------------------------------------------------------
		//	ICollection Impl
		//--------------------------------------------------------------------------------------

		public int Count
		{
			get { return InnerList.Count; }
		}

		public bool IsSynchronized
		{
            get { return InnerList.IsSynchronized; }
        }
        
        public object SyncRoot
		{
            get { return InnerList.SyncRoot; }
        }
        
        public void CopyTo(Array arr, int index)
		{
            InnerList.CopyTo(arr, index);
        }

		
		//--------------------------------------------------------------------------------------
		// Explicit IList impl 
		// These methods are explicit so they don't get in the way of our types ones.
		//--------------------------------------------------------------------------------------

		object IList.this[int index]
		{
			get { return InnerList[index]; }
			set { AssertIsMutable(); InnerList[index] = value; }
		}

		int IList.Add(object newObject)
		{
			return Add((IEntityObject)newObject);
		}

		void IList.Remove(object existingObject)
		{
			Remove((IEntityObject)existingObject);
		}

		void IList.Insert(int index, object eo)
		{
			Insert(index, (IEntityObject)eo);
		}

		bool IList.Contains(object eo)
		{
			return Contains((IEntityObject)eo);
		}

		int IList.IndexOf(object eo)
		{
			return IndexOf((IEntityObject)eo);
		}

		//--------------------------------------------------------------------------------------
		// Remaining IList impl 
		// These methods don't declare object types and therefore need not be explicit. 
		//--------------------------------------------------------------------------------------
	
		public virtual bool IsFixedSize
		{
			get { return false; }
		}

		public virtual bool IsReadOnly
		{
			get { return false; }
		}

		public void Clear()
		{
			AssertIsMutable(); 
			while(Count > 0)
				Remove((IEntityObject)((IList)this)[Count - 1]);
 			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}

		public void RemoveAt(int index)
		{
			AssertIsMutable(); 
			Remove((IEntityObject)((IList)this)[index]);
		}

		
		//--------------------------------------------------------------------------------------
		// Protected typed variants of IList
		// These are called, after an appropriate cast, from the explicit IList impl. These
		// methods can (a) be overridden by subclasses to change behaviour and (b) be used 
		// to implement the public strongly typed methods.
		//--------------------------------------------------------------------------------------

		protected virtual int Add(IEntityObject newObject)
		{
 			AssertIsMutable();
 			int index = InnerList.Add(newObject);
 			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
 			return index; 		
		}

		protected virtual void Remove(IEntityObject existingObject)
		{
			AssertIsMutable(); 
			int index = IndexOf(existingObject);
			InnerList.Remove(existingObject);
			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));		
		}

		protected virtual void Insert(int index, IEntityObject eo)
		{
			AssertIsMutable(); 
			InnerList.Insert(index, eo);
 			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));		
		}

		protected virtual bool Contains(IEntityObject eo)
		{
			return InnerList.Contains(eo);
		}

		protected virtual int IndexOf(IEntityObject eo)
		{
			return InnerList.IndexOf(eo);
		}


		//--------------------------------------------------------------------------------------
		// Explicit IBindingList impl 
		// Most members are explicit as they are probably only needed for controls that support 
		// IBindingList.
		//--------------------------------------------------------------------------------------
		 
		bool IBindingList.AllowEdit 
									
		{ 
			get { return true ; }
		}

 		bool IBindingList.AllowNew 
 		{ 
 			get { return false ; }
 		}
 
 		bool IBindingList.AllowRemove 
 		{ 
 			get { return false ; }
 		}
 
 		bool IBindingList.SupportsChangeNotification 
 		{ 
 			get { return true ; }
 		}
     
 		bool IBindingList.SupportsSearching 
 		{ 
 			get { return false ; }
 		}
 
 		public virtual bool SupportsSorting 
 		{ 
 			get { return false ; }
 		}
 
 		// Methods.
 		object IBindingList.AddNew() 
 		{
 			throw new NotSupportedException();
 		}
 
 
 		// Unsupported properties.
 		bool IBindingList.IsSorted 
 		{ 
 			get { throw new NotSupportedException(); }
 		}
 
 		ListSortDirection IBindingList.SortDirection 
 		{ 
 			get { throw new NotSupportedException(); }
 		}
 
 
 		PropertyDescriptor IBindingList.SortProperty 
 		{ 
 			get { throw new NotSupportedException(); }
 		}
 
 
 		// Unsupported Methods.
 		void IBindingList.AddIndex(PropertyDescriptor property) 
 		{
 			throw new NotSupportedException(); 
 		}
 
 		void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction) 
 		{
 			throw new NotSupportedException(); 
 		}
 
 		int IBindingList.Find(PropertyDescriptor property, object key) 
 		{
 			throw new NotSupportedException(); 
 		}
 
 		void IBindingList.RemoveIndex(PropertyDescriptor property) 
 		{
 			throw new NotSupportedException(); 
 		}
 
 		void IBindingList.RemoveSort() 
 		{
 			throw new NotSupportedException(); 
 		}
 
 		private ListChangedEventHandler onListChanged;
 
 		public event ListChangedEventHandler ListChanged 
 		{
 			add 
 			{
 				onListChanged += value;
 			}
 			remove 
 			{
 				onListChanged -= value;
 			}
 		}
 
 		protected virtual void OnListChanged(ListChangedEventArgs ev) 
 		{
 			if (onListChanged != null) 
 			{
 				onListChanged(this, ev);
 			}
 		}
 
 		// Called by objects in the collection when they change.
 		protected void EntityObjectChanged(object eo) 
 		{
 			int index = InnerList.IndexOf(eo);
 			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, index));
 		}

		
		//--------------------------------------------------------------------------------------
		//	Finder methods
		//  Should be used by subclasses to implement strongly typed finders.
		//--------------------------------------------------------------------------------------

		protected virtual IEntityObject FindUnique(string qualifierFormat, params object[] parameters)
		{
			Qualifier qualifier = Qualifier.Format(qualifierFormat, parameters);
			IEntityObject match = null;
			foreach(IEntityObject eo in InnerList)
			{
				if(qualifier.EvaluateWithObject(eo))
				{
					if(match != null)
						throw new NotUniqueException(true, qualifierFormat, parameters);
					match = eo;
				}
			}
			if(match == null)
				throw new NotUniqueException(false, qualifierFormat, parameters);
			return match;
		}


		protected virtual IEntityObject FindFirst(string qualifierFormat, params object[] parameters)
		{
			Qualifier qualifier = Qualifier.Format(qualifierFormat, parameters);
			foreach(IEntityObject eo in InnerList)
			{
				if(qualifier.EvaluateWithObject(eo))
					return eo;
			}
			return null;
		}


		protected virtual void Find(IList resultSet, string qualifierFormat, params object[] parameters)
		{
			Qualifier qualifier = Qualifier.Format(qualifierFormat, parameters);
			foreach(IEntityObject eo in InnerList)
			{
				if(qualifier.EvaluateWithObject(eo))
					resultSet.Add(eo);
			}
		}


		//--------------------------------------------------------------------------------------
		//	Comparison methods
		//--------------------------------------------------------------------------------------
		
		private const string InThisListMarker = "i";
		private const string InOtherListMarker = "o";

		public bool ContainsSameObjects(ObjectCollectionBase other)
		{
			Hashtable oidTable;

			if(this.Count != other.Count)
				return false;

			oidTable = new Hashtable(InnerList.Count);
			foreach(IEntityObject eo in InnerList)
			{
				ObjectId oid = ObjectId.GetObjectIdForObject(eo);
				oidTable[oid] = InThisListMarker;
			}
			foreach(IEntityObject eo in other)
			{
				ObjectId oid = ObjectId.GetObjectIdForObject(eo);
				if(Object.ReferenceEquals(oidTable[oid], InThisListMarker) == false)
					return false;
				oidTable[oid] = InOtherListMarker;
			}
			return true;
		}


		//--------------------------------------------------------------------------------------
		//	Mapping methods
		//--------------------------------------------------------------------------------------

		public IList GetProperty(string propName)
		{
			ArrayList result = new ArrayList(InnerList.Count);
			foreach(IEntityObject eo in InnerList)
				result.Add(ObjectHelper.GetProperty(eo, propName));
			return result;
		}


	}
}
