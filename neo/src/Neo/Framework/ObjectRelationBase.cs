using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using Neo.Core;


namespace Neo.Framework
{
	/// <summary>
	/// This is to be subclassed to provide strongly typed representations of the set
	/// of objects which are part of a relation.
	/// </summary>
	public abstract class ObjectRelationBase : ObjectCollectionBase
	{
		//--------------------------------------------------------------------------------------
		//	Fields and constructor
		//--------------------------------------------------------------------------------------

		internal readonly IEntityObject Owner;
		internal readonly DataRelation Relation;
		protected		  IList innerList;

		protected ObjectRelationBase(IEntityObject eo, DataRelation aRelation)
		{
			if(aRelation.ParentColumns.Length > 1)
				throw new ArgumentException("DataRelation cannot have compound keys.");
			Owner = eo;
			Relation = aRelation;
		}


		//--------------------------------------------------------------------------------------
		//	Protected properties
		//--------------------------------------------------------------------------------------

		protected override IList InnerList
		{
			get
			{
				if(innerList == null)
					Load();
				return innerList;
			}
		}

		//--------------------------------------------------------------------------------------
		//	Some properties to make the code below more readable
		//--------------------------------------------------------------------------------------

		private string foreignTableName
		{
			get { return Relation.ChildTable.TableName; }
		}

		private string localColumnName
		{
			get { return Relation.ParentColumns[0].ColumnName; }
		}

		private string foreignColumnName
		{
			get { return Relation.ChildColumns[0].ColumnName; }
		}


		//--------------------------------------------------------------------------------------
		//	Collection management
		//--------------------------------------------------------------------------------------

		protected virtual void Load()
		{
			innerList = Owner.Context.GetObjectsFromTable(foreignTableName, foreignColumnName, Owner.Row[localColumnName]);

			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
			
			Owner.Context.RegisterForColumnChanges(new ColumnChangeHandler(OnColumnChanging), Relation.ChildTable.TableName, foreignColumnName);
			Owner.Context.RegisterForRowChanges(new RowChangeHandler(OnRowDeleting), Relation.ChildTable.TableName);
		}

		private object ObjectForForeignRow(DataRow row, bool tryDeleted)
		{
			IEntityMap emap = Owner.Context.EntityMapFactory.GetMap(row.Table.TableName);
			object[] pkvalues = Owner.Context.GetPrimaryKeyValuesForRow(emap, row, DataRowVersion.Current);
			object eo = Owner.Context.ObjectTable.GetObject(foreignTableName, pkvalues);
			if((eo == null) && tryDeleted)
				eo = Owner.Context.ObjectTable.GetDeletedObject(foreignTableName, pkvalues);
			return eo;
		}

		public void OnRowDeleting(object sender, DataRowChangeEventArgs e)
		{
			// Check whether our owner is deleted. In this case we don't need to do
			// anything. Actually, we can't...
			if((Owner.Row.RowState == DataRowState.Deleted) || (Owner.Row.RowState == DataRowState.Detached))
				return;

			// We assume this is a row we are related to, because this is checked by the event broker
			innerList.Remove(ObjectForForeignRow(e.Row, true));
		}

		public void OnColumnChanging(object sender, DataColumnChangeEventArgs e)
		{
			// Check whether our owner is deleted. In this case we don't need to do
			// anything. Actually, we can't...
			if((Owner.Row.RowState == DataRowState.Deleted) || (Owner.Row.RowState == DataRowState.Detached))
				return;

			// The ObjectContext tells us to ignore this row for the moment being.
			if(e.Row == Owner.Context.RowPending) 
				return; 

			object newFkValue = e.ProposedValue;
			object oldFkValue = e.Row[foreignColumnName];
			object ownerPkValue = Owner.Row[localColumnName];

			if((newFkValue != null) && (newFkValue.Equals(ownerPkValue)))
			{	
				object eo = ObjectForForeignRow(e.Row, false);
				// We need these sanity checks. During PK change propagation we see a
				// change of the child rows but we don't need to add the object for
				// the new matching row because we did so when both parent and child
				// still had the old value.
				if((eo != null) && (innerList.Contains(eo) == false))
					AddToInnerList(eo);
			}

			if((oldFkValue != null) && (oldFkValue.Equals(ownerPkValue)) && (oldFkValue.Equals(newFkValue) == false))
			{
				object eo = ObjectForForeignRow(e.Row, false);
				RemoveFromInnerList(eo);
			}

			// If a new (added) row was modified by a child context we receive a change
			// event when the child's changes are applied to our context. However, the
			// change event doesn't contain the original value which means that if the
			// object was part of the relation before we won't know other than by
			// testing whether the object is in the inner list.
			if((e.Row.RowState == DataRowState.Added) && (newFkValue != null) && (newFkValue.Equals(ownerPkValue) == false))
			{
				object eo = ObjectForForeignRow(e.Row, false);
				if(innerList.Contains(eo))
					RemoveFromInnerList(eo);

			}
		}

		protected virtual void AddToInnerList(object eo)
		{
			int index = innerList.Add(eo);
			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
		}

		protected virtual void RemoveFromInnerList(object eo)
		{
			int index = innerList.IndexOf(eo);
			innerList.Remove(eo);
			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
		}
   
		
		//--------------------------------------------------------------------------------------
		//	Untyped implementations to be called by subclasses
		//--------------------------------------------------------------------------------------

		protected override void Insert(int index, IEntityObject eo)
		{
			throw new InvalidOperationException("Cannot insert into an ObjectRelation. Must use Add instead.");
		}

		protected override int Add(IEntityObject newObject)
		{
			newObject.Row[foreignColumnName] = Owner.Row[localColumnName];
			return IndexOf(newObject);
		}

		protected override void Remove(IEntityObject existingObject)
		{
			existingObject.Row[foreignColumnName] = DBNull.Value;
		}

		protected virtual void CopyToListAndMakeReadOnly(ObjectListBase list)
		{
			foreach(object o in this)
				((IList)list).Add(o);
			list.MakeReadOnly();
		}

		protected virtual void CopyToListAndSort(ObjectListBase list, string propName, SortDirection dir)
		{
			foreach(object o in this)
				((IList)list).Add(o);
			list.Sort(propName, dir);
			list.MakeReadOnly();
		}

		//--------------------------------------------------------------------------------------
		//	Public properties and methods
		//--------------------------------------------------------------------------------------

		public virtual void Touch()
		{
			if(innerList == null)
				Load();
		}

		public virtual void InvalidateCache()
		{
			innerList = null;
			Owner.Context.UnRegisterForColumnChanges(new ColumnChangeHandler(OnColumnChanging), Relation.ChildTable.TableName, foreignColumnName);
			Owner.Context.UnRegisterForRowChanges(new RowChangeHandler(OnRowDeleting), Relation.ChildTable.TableName);
			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}

	}
}