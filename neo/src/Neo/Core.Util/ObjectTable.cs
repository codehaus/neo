using System;
using System.Collections;
using Neo.Core;


namespace Neo.Core.Util
{
	/// <summary>
	/// Summary description for ObjectTable.
	/// </summary>
	public class ObjectTable
	{
		private IDictionary		objectTable;
		private IDictionary		objectsByEntityTable;
		private IDictionary			deletedObjectTable;


		public ObjectTable()
		{
			objectTable = new Hashtable();
			objectsByEntityTable = new Hashtable();
			deletedObjectTable = new Hashtable();
		}


		public void AddObject(ObjectId oid, IEntityObject eo)
		{
			ArrayList	entityList;

			objectTable[oid] = eo;

			if((entityList = (ArrayList)objectsByEntityTable[oid.TableName]) == null)
			{
				entityList = new ArrayList();
				objectsByEntityTable[oid.TableName] = entityList;
			}
			entityList.Add(eo);
		}


		public void RemoveObject(ObjectId oid, IEntityObject eo)
		{
			objectTable.Remove(oid);
			((ArrayList)objectsByEntityTable[oid.TableName]).Remove(eo);
		}


		public IList GetAllObjects()
		{
			return new ArrayList(objectTable.Values);	
		}


		public IEntityObject GetObject(ObjectId oid)
		{
			return (IEntityObject)objectTable[oid];
		}


		public IEntityObject GetObject(string tablename, object[] pkvalues)
		{
			return (IEntityObject)objectTable[new ObjectId(tablename, pkvalues)];
		}


		public IList GetObjects(string tablename)
		{
			return (IList)objectsByEntityTable[tablename];
		}


		public void DeleteObject(ObjectId oid)
		{
			IEntityObject eo;

			if((eo = GetObject(oid)) == null)
			{
				if(GetDeletedObject(oid) != null)
					return; // sort-of okay, did see this before
				throw new InvalidOperationException("Nothing known about object " + oid.ToString());
			}
			DeleteObject(oid, eo);
		}


		public void DeleteObject(ObjectId oid, IEntityObject eo)
		{
			RemoveObject(oid, eo);
			deletedObjectTable[oid] = eo;
		}


		public void ForgetAllDeletedObjects()
		{
			deletedObjectTable.Clear();
		}


		public void ForgetDeletedObject(ObjectId oid)
		{
			deletedObjectTable.Remove(oid);
		}


		public IList GetAllDeletedObjectIds()
		{
			return new ArrayList(deletedObjectTable.Keys);
		}


		public IEntityObject GetDeletedObject(ObjectId oid)
		{
			return (IEntityObject)deletedObjectTable[oid];
		}


		public IEntityObject GetDeletedObject(string tablename, object[] pkvalues)
		{
			return (IEntityObject)deletedObjectTable[new ObjectId(tablename, pkvalues)];
		}

	}
}
