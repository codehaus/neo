using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using log4net;
using Neo.Core.Qualifiers;
using Neo.Core.Util;


namespace Neo.Core
{
	public delegate void ColumnChangeHandler(object sender, DataColumnChangeEventArgs e);
	public delegate void RowChangeHandler(object sender, DataRowChangeEventArgs e);
	
	/// <summary>
	/// ObjectContext provides a wrapper around core ADO.Net functionality.
	/// </summary>
	/// <remarks>
	/// <para>
	/// ObjectContext uses a data store to retrieve objects and save changes.
	/// A special data store could act as a façade for other data stores. 
	/// I could select an appropriate store for a request and forward the request.
	/// The context relies on its internal DataSet to track changes (creates, updates and deletes.)
	/// It provides methods to save or reject the changes.
	/// </para>
	/// </remarks>
	[Serializable]
	public class ObjectContext : IDataStore, ISerializable
	{

		//--------------------------------------------------------------------------------------
		//	Static fields and methods
		//--------------------------------------------------------------------------------------

		/// <summary>
		/// Static reference to a logging facility
		/// </summary>
		private static ILog logger;


		//--------------------------------------------------------------------------------------
		//	Fields and constructor
		//--------------------------------------------------------------------------------------

		private ObjectTable			objectTable;
		private DataSet				mainDataSet;
		private IDataStore			dataStore;
		private bool				copiedParent;
		private IEntityMapFactory	emapFactory;
		private PropertyCollection	extendedProperties;
		private DataRow				rowPending;
		private Hashtable			eventHandlers;
		protected ColumnChangeBroker  columnChangeBroker;
		protected RowChangeBroker		rowChangeBroker;

		/// <summary>
		/// Default constructor. Sets up logger and default map factory
		/// </summary>
		public ObjectContext()
		{
			if(logger == null)
				logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

			emapFactory = DefaultEntityMapFactory.SharedInstance;
			
			// If you add code here, you might want to add it to Clear() as well
			objectTable = new ObjectTable();
			mainDataSet = new DataSet();
			mainDataSet.EnforceConstraints = false;
			eventHandlers = new Hashtable();
			rowChangeBroker = new RowChangeBroker();
			columnChangeBroker = new ColumnChangeBroker();
			
			logger.Debug("Initialised new object context.");
		}


		/// <summary>
		/// Constructor. Takes data from the supplied data set into this context
		/// </summary>
		/// <param name="aDataSet">data to be imported</param>
		public ObjectContext(DataSet aDataSet) : this()
		{
			MergeData(aDataSet, false);
		}

		
		/// <summary>
		/// Constructor. Takes data from the supplied data set into this context
		/// and uses a custom entitymapfactory to find maps.
		/// </summary>
		/// <param name="aDataSet">data to be imported</param>
		public ObjectContext(DataSet aDataSet, IEntityMapFactory aFactory) : this()
		{
			emapFactory = aFactory;
			MergeData(aDataSet, false);
		}
	
		/// <summary>
		/// Constructor. Takes data from the supplied data store into this context
		/// </summary>
		/// <remarks>
		/// Data from the store is usually loaded on demand.
		/// </remarks>
		/// <param name="aDataStore">data to be imported</param>
		public ObjectContext(IDataStore aDataStore) : this()
		{
			dataStore = aDataStore;

			ObjectContext parentContext = aDataStore as ObjectContext;
			if(parentContext != null)
			{
				emapFactory = parentContext.emapFactory;
				MergeData(parentContext.DataSet, false);
				copiedParent = true;
			}
		}


		/// <summary>
		/// Constructor. Takes data from the supplied parent context into this context
		/// </summary>
		/// <remarks>
		/// Data from a parent context is normally copied to improve performace. If 
		/// the parent context is huge it might improve overall performance to enable 
		/// on-demand copying.
		/// </remarks>
		/// <param name="parentContext">data to be imported</param>
		/// <param name="copyInCtor">whether to copy the parent intially or on demand</param>
		public ObjectContext(ObjectContext parentContext, bool copyInCtor) : this()
		{
			dataStore = parentContext;

			emapFactory = parentContext.emapFactory;
			if(copyInCtor)
			{
				MergeData(parentContext.DataSet, false);
				copiedParent = true;
			}

		}

		//--------------------------------------------------------------------------------------
		//	Public properties
		//--------------------------------------------------------------------------------------

		/// <summary>
		/// The DataSet containing this ObjectContext&apos;s data
		/// </summary>
		public DataSet DataSet
		{
			get { return mainDataSet; }
		}


		/// <summary>
		/// The DataStore used to persist the DataSet&apos;s data
		/// </summary>
		public IDataStore DataStore
		{
			get { return dataStore; }
		}


		/// <summary>
		/// The EntityMapFactory used to map entity attributes
		/// </summary>
		public IEntityMapFactory EntityMapFactory
		{
			set { emapFactory = value; }
			get { return emapFactory; }
		}


		/// <summary>
		/// A property collection instance that can be used to store application specific
		/// information in the context
		/// </summary>
		/// <remarks>
		/// The context completely ignores the contents of the property collection. It is
		/// meant as convenience to avoid having to subclass ObjectContext just because some
		/// extra properties are needed.
		/// </remarks>
		public PropertyCollection ExtendedProperties
		{
			get 
			{
				if(extendedProperties == null)
					extendedProperties = new PropertyCollection();
				return extendedProperties;
			}
		}


		//--------------------------------------------------------------------------------------
		//	Protected and internal properties
		//--------------------------------------------------------------------------------------

		protected internal ObjectTable ObjectTable
		{
			get { return objectTable; }
		}

		
		/// <summary>
		/// Marks row as being processed and change events for it to be ignored
		/// </summary>
		/// <remarks>
		// This is used during object creation because for some objects the PK is also a FK
		// and when the PK is set in the row the context doesn't have the corresponding object 
		// for that row in its tables and thus observers wouldn't be able to look it up. The 
		// context resends the events after its data structures are okay.
		/// </remarks>
		protected internal DataRow RowPending
		{
			get { return rowPending; }
		}


		protected ILog Logger
		{
			get { return logger; }
		}


		protected virtual bool CanLoadFromStore
		{
			get { return (dataStore != null) && ((dataStore is ObjectContext == false) || (copiedParent == false)); }
		}


		//--------------------------------------------------------------------------------------
		//	Dealing with change
		//--------------------------------------------------------------------------------------

		/// <summary>
		/// Clears and resets the internal objects in this Context
		/// </summary>
		/// <remarks>
		/// This makes the context effectively forget all object and their changes.
		/// </remarks>
		public virtual void Clear()
		{
			objectTable = new ObjectTable();
			mainDataSet = new DataSet();
			mainDataSet.EnforceConstraints = false;
			eventHandlers = new Hashtable();
		}


		/// <summary>
		/// Gets all changes made to the main DataSet
		/// </summary>
		/// <returns>all changes made to the main DataSet</returns>
		public virtual DataSet GetChanges()
		{
			return mainDataSet.GetChanges();
		}


		/// <summary>
		/// Determines whether there are any data changes in this context
		/// </summary>
		/// <returns><c>true</c> if changes have been made</returns>
		public virtual bool HasChanges()
		{
			return mainDataSet.HasChanges();
		}
		

		/// <summary>
		/// Marks all changes as having been persisted
		/// </summary>
		/// <remarks>
		/// Use this in conjunction with GetChanges() to implement alternative
		/// persistence operations, in client/server scenarios for example.
		/// </remarks>
		public virtual void AcceptChanges()
		{
			mainDataSet.AcceptChanges();
			objectTable.ForgetAllDeletedObjects();
		}

	
		/// <summary>
		/// Persists data changes in this context to the data store
		/// </summary>
		/// <returns>list of primary keys that were changed during the operation (only for 
		/// native scheme)</returns>
		public virtual ICollection SaveChanges()
		{
			ICollection pkChangeTableList;

			if(dataStore == null)
				throw new InvalidOperationException("Cannot save changes without a data store.");
			
			try
			{
				logger.Debug("SaveChanges. Beginning.");
				pkChangeTableList = dataStore.SaveChangesInObjectContext(this);
				if(pkChangeTableList != null)
					UpdatePrimaryKeys(pkChangeTableList);
				mainDataSet.AcceptChanges();
				objectTable.ForgetAllDeletedObjects();
				logger.Debug("SaveChanges. Completed.");
			}
			catch(Exception e)
			{
				logger.Error("Caught exception while saving changes: " + e.GetType() + "[" + e.Message + "] " + e.StackTrace.Split(new char[] {'\r', '\n'}, 2)[0]);
				if(e is DataStoreSaveException)
					throw e;
				throw new DataStoreSaveException(e);
			}
			return pkChangeTableList;
		}


		//--------------------------------------------------------------------------------------
		//	Helper methods
		//--------------------------------------------------------------------------------------

		/// <summary>
		/// Sets up event handlers for the supplied DataTable
		/// </summary>
		/// <param name="table">DataTable object to which event handlers should be added</param>
		/// <remarks>Adds event handlers for RowDeleting event and RowChanging event</remarks>
		protected virtual void RegisterForTableEvents(DataTable table)
		{
			if(eventHandlers[table.TableName] != null)
				return;

			eventHandlers[table.TableName] = true;
			table.RowDeleting += new DataRowChangeEventHandler(this.OnRowDeleting);
			table.RowChanging += new DataRowChangeEventHandler(this.OnRowChanging);
			table.RowChanged += new DataRowChangeEventHandler(this.OnRowChanged);
			table.ColumnChanging += new DataColumnChangeEventHandler(OnColumnChanging);
		}

		/// <summary>
		/// Raises events for primary key column changes
		/// </summary>
		/// <param name="row">row containing primary key</param>
		protected internal virtual void SendPkColumnChangingEvents(DataRow row)
		{
			foreach(DataColumn c in row.Table.PrimaryKey)
			{
				DataColumnChangeEventArgs eventArg = new DataColumnChangeEventArgs(row, c, row[c]);
				OnColumnChanging(row.Table, eventArg);
			}
		}

		/// <summary>
		/// Gets all values for primary key columns of the supplied row
		/// </summary>
		/// <param name="emap">IEntityMap object holding entity definition</param>
		/// <param name="row">row holding primary key values</param>
		/// <param name="ver">DataRowVersion to use for lookup</param>
		/// <returns>Array of primary key values</returns>
		protected internal object[] GetPrimaryKeyValuesForRow(IEntityMap emap, DataRow row, DataRowVersion ver)
		{
			string[] columns;
			object[] values;

			columns = emap.PrimaryKeyColumns;
			values = new object[columns.Length];
			for(int i = 0; i < columns.Length; i++)
				values[i] = row[columns[i], ver];

			return values;
		}


		/// <summary>
		/// Finds row in the existing DataSet and either returns the IEntityObject
		/// representation of it, or creates the representation
		/// </summary>
		/// <param name="aRow">row containing data to be found</param>
		/// <returns>IEntityObject instance representing the supplied row</returns>
		protected virtual IEntityObject GetObjectForRow(DataRow aRow)
		{
			IEntityMap	  emap;
			DataRowVersion lookupVersion;
			object[]	  pkvalues;
			ObjectId	  oid;
			IEntityObject eo;
				
			emap = emapFactory.GetMap(aRow.Table.TableName);
			lookupVersion = (aRow.RowState == DataRowState.Deleted) ? DataRowVersion.Original : DataRowVersion.Current;
			pkvalues = GetPrimaryKeyValuesForRow(emap, aRow, lookupVersion);
			oid = new ObjectId(emap.TableName, pkvalues);

			// If the object is known to be deleted, forget about it.
			if((eo = objectTable.GetDeletedObject(oid)) != null)
				return null;

			// We allow rows from other datasets to be passed in. Find them in the main set.
			if(aRow.Table.DataSet != mainDataSet)
			{
				if((aRow = mainDataSet.Tables[emap.TableName].Rows.Find(pkvalues)) == null)
					throw new InvalidOperationException("Row not found in this context's mainDataSet.");
			}

			if(aRow.RowState != DataRowState.Deleted)
			{
			// If we don't have an object yet, create it.
			if((eo = objectTable.GetObject(oid)) == null)
			{
				eo = CreateEntityObjectInstance(aRow, emap);
				objectTable.AddObject(oid, eo);
			}
			else
			{
				if(eo.Row.Equals(aRow) == false)
					throw new InvalidOperationException("Internal inconsistency; object exists but references different row.");
			}
			}
			else
			{
				eo = CreateEntityObjectInstance(aRow, emap);
				objectTable.AddDeletedObject(oid, eo);
			}
			return eo;
		}


		protected virtual IEntityObject CreateEntityObjectInstance(DataRow row, IEntityMap emap)
		{
			BindingFlags bflags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			object[] args = { row, this };
			return (IEntityObject)Activator.CreateInstance(emap.ConcreteObjectType, bflags, null, args, null);
		}


		//--------------------------------------------------------------------------------------
		//	Handling and distributing change events
		//--------------------------------------------------------------------------------------

		protected virtual void OnRowChanging(object sender, DataRowChangeEventArgs e)
		{
			if(e.Row == rowPending)
				return;

			if(e.Action == DataRowAction.Rollback && e.Row.RowState == DataRowState.Added)
			{
				IEntityMap emap = emapFactory.GetMap(e.Row.Table.TableName);
				object[] pkvalues = GetPrimaryKeyValuesForRow(emap, e.Row, DataRowVersion.Current);
				objectTable.DeleteObject(new ObjectId(e.Row.Table.TableName, pkvalues));
			}
		}

		protected virtual void OnRowChanged(object sender, DataRowChangeEventArgs e)
		{
			if(e.Action == DataRowAction.Delete)
				rowChangeBroker.OnRowDeleting(sender, e);
		}

		protected virtual void OnRowDeleting(object sender, DataRowChangeEventArgs e)
		{
			if(e.Row == rowPending)
				return;

			IEntityMap emap = emapFactory.GetMap(e.Row.Table.TableName);
			object[] pkvalues = GetPrimaryKeyValuesForRow(emap, e.Row, DataRowVersion.Current);
			objectTable.DeleteObject(new ObjectId(e.Row.Table.TableName, pkvalues));

			rowChangeBroker.OnRowDeleting(sender, e);
		}
	
		protected virtual void OnColumnChanging(object sender, DataColumnChangeEventArgs e)
		{
			columnChangeBroker.OnColumnChanging(sender, e);
		}


		public void RegisterForColumnChanges(ColumnChangeHandler handler, string tableName, string columnName) 
		{
			columnChangeBroker.RegisterForColumnChanging(handler, tableName, columnName);
		}

		public void UnRegisterForColumnChanges(ColumnChangeHandler handler, string tableName, string columnName)
		{
			columnChangeBroker.UnRegisterForColumnChanging(handler, tableName, columnName);
		}

		public void RegisterForRowChanges(RowChangeHandler handler, string tableName)
		{
			rowChangeBroker.RegisterForRowChanged(handler, tableName);
		}

		public void UnRegisterForRowChanges(RowChangeHandler handler, string tableName)
		{
			rowChangeBroker.UnRegisterForRowChanged(handler, tableName);
		}
	

		//--------------------------------------------------------------------------------------
		//	Adding data from ADO.NET to this context
		//--------------------------------------------------------------------------------------

		public virtual IEntityObject ImportRow(DataRow aRow)
		{
			DataTable	table;
			DataRow		newRow;
			IEntityMap	emap;
			
			emap = emapFactory.GetMap(aRow.Table.TableName);
			emap.UpdateSchemaInDataSet(mainDataSet, SchemaUpdate.Full);

			table = mainDataSet.Tables[aRow.Table.TableName];
			if(table.Rows.Find(GetPrimaryKeyValuesForRow(emap, aRow, DataRowVersion.Current)) != null)
				throw new NeoException("Failed to import row; already exists in this context.");

			newRow = table.NewRow();
			table.Rows.Add(newRow);
			foreach(DataColumn c in table.Columns)
			{
				if(aRow.Table.Columns[c.ColumnName] != null)
					newRow[c] = aRow[c.ColumnName];
			}
			
			return GetObjectForRow(newRow);
		}


		/// <summary>
		/// Adds all rows for a table to the underlying DataSet
		/// </summary>
		/// <param name="newTable">DataTable to be added</param>
		protected virtual void ImportRowsFromTable(DataTable newTable)
		{
			IEntityMap	emap;
			string		tableName;
			DataTable	thisTable;
			ObjectId	oid;
			
			tableName = newTable.TableName;
			emap = emapFactory.GetMap(tableName);
			emap.UpdateSchemaInDataSet(mainDataSet, SchemaUpdate.Full);
			thisTable = mainDataSet.Tables[tableName];
			RegisterForTableEvents(thisTable);

			foreach(DataRow rowInNewTable in newTable.Rows)
			{
				oid = new ObjectId(tableName, GetPrimaryKeyValuesForRow(emap, rowInNewTable, DataRowVersion.Current));
				if((objectTable.GetObject(oid) == null) && (objectTable.GetDeletedObject(oid) == null))
				{
					thisTable.ImportRow(rowInNewTable);
					GetObjectForRow(rowInNewTable);
				}
			}
		}


		/// <summary>
		/// Merges the supplied DataSet with the underlying DataSet
		/// </summary>
		/// <param name="aDataSet">DataSet object to be merged</param>
		/// <returns>List of added <c>IEntityObject</c>s</returns>
		public virtual IList MergeData(DataSet aDataSet)
		{
			return MergeData(aDataSet, true);
		}

		protected virtual IList MergeData(DataSet aDataSet, bool sendColChangeEvents)
		{
			ArrayList	objectList;
			DataTable	table;
			IEntityObject	eo = null;

			if(aDataSet == null)
				throw new ArgumentException("MergeData called with null DataSet");

			foreach(DataTable t in aDataSet.Tables)
			{
				IEntityMap emap = emapFactory.GetMap(t.TableName);
				emap.UpdateSchemaInDataSet(mainDataSet, SchemaUpdate.Full);
			}

			mainDataSet.Merge(aDataSet, false, MissingSchemaAction.Ignore);

			objectList = new ArrayList();

			foreach(DataTable t in aDataSet.Tables)
			{
				table = mainDataSet.Tables[t.TableName];
				RegisterForTableEvents(table);

				foreach(DataRow r in table.Rows)
				{
					if((eo = GetObjectForRow(r)) != null)
					{
						if(r.RowState != DataRowState.Deleted)
						objectList.Add(eo);
					}
				}
			}
			if(sendColChangeEvents)
			{
				logger.Debug("Beginning to send post merge events.");

				// For some reason the dataset suspends sending DataColumnChanging events. (Even more
				// curious as it does send DataRowDeleting events.) In any case, our Relations rely
				// on these events, so we have to send them ourselves.
				foreach(DataRelation relation in aDataSet.Relations)
				{
					if(relation.ChildColumns.Length != 1)
						throw new InvalidOperationException("Neo does not support compound foreign keys.");
					foreach(DataRow row in relation.ChildTable.Rows)
					{
						DataColumn				  fkColumn;
						object					  prevValue;
						DataColumnChangeEventArgs eventArg;

						if((row.RowState == DataRowState.Deleted) || (row.RowState == DataRowState.Detached))
							continue;
						fkColumn = relation.ChildColumns[0];
						prevValue = row[fkColumn];
						eventArg = new DataColumnChangeEventArgs(row, row.Table.Columns[fkColumn.ColumnName], prevValue);
						OnColumnChanging(mainDataSet.Tables[row.Table.TableName], eventArg);
					}
				}
				logger.Debug("Finished sending post merge events.");
			}

			return objectList;
		}


		//--------------------------------------------------------------------------------------
		//	Updating Primary Keys
		//--------------------------------------------------------------------------------------

		/// <summary>
		/// 
		/// </summary>
		/// <param name="changeTables"></param>
		public virtual void UpdatePrimaryKeys(ICollection changeTables)
		{
			foreach(PkChangeTable t in changeTables)
				UpdatePrimaryKeys(t); 
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="changeTable">changeTable holding keys to be updated</param>
		public virtual void UpdatePrimaryKeys(PkChangeTable changeTable)
		{
			IEntityMap	  emap;
			ObjectId	  oldOid, newOid;
			IEntityObject eo;

			// What happens if we have sub contexts? How will they find out about these changes?
			// They won't and saving there changes will duplicate newly inserted objects...

			emap = emapFactory.GetMap(changeTable.TableName);
			if(emap.PrimaryKeyColumns.Length > 1)
				throw new ArgumentException("PkChangeTable refers to a table that has a compound primary key.");

			foreach(PkChangeTable.ChangeRecord r in changeTable)
			{
				oldOid = new ObjectId(changeTable.TableName, r.OldValue);
				newOid = new ObjectId(changeTable.TableName, r.NewValue);
				eo = objectTable.GetObject(oldOid);
				if(eo == null)
					throw new ArgumentException("PkChangeTable references unknown object " + oldOid.ToString());
				eo.Row[emap.PrimaryKeyColumns[0]] = r.NewValue;
				objectTable.RemoveObject(oldOid, eo);
				objectTable.AddObject(newOid, eo);
			}

		}
		 

		//--------------------------------------------------------------------------------------
		//	Creating objects
		//--------------------------------------------------------------------------------------

		/// <summary>
		/// Creates a new <c>IEntityObject</c> with all of the required primary key values
		/// </summary>
		/// <param name="objectType"><c>IEntityObject</c> type to be created</param>
		/// <param name="pkvalues">array holding exactly the correct number of primary key values
		/// or null if the pk scheme generates its own keys, e.g. GUID and native schemes.</param>
		/// <returns></returns>
		/// <remarks></remarks>
		public virtual IEntityObject CreateObject(Type objectType, object[] pkvalues)
		{
			IEntityMap		emap;
			IPkInitializer	pkInit;
			ObjectId		oid;
			DataRow			newRow, existingRow;
			IEntityObject	newObject, deletedObject;

			if(logger.IsDebugEnabled) logger.Debug("Creating object of type " + objectType.ToString());

			emap = emapFactory.GetMap(objectType);
			emap.UpdateSchemaInDataSet(mainDataSet, SchemaUpdate.Full);
			pkInit = emap.GetPkInitializer();

			existingRow = null;
			// If we get a pk argument, which means they are defined externally, there is a 
			// chance that a primary key which belonged to an object that was deleted is reused. 
			// In this case we need to recycle the row.
			if(pkvalues != null)
			{
				oid = new ObjectId(emap.TableName, pkvalues);
				if((deletedObject = objectTable.GetDeletedObject(oid)) != null)
				{
					objectTable.ForgetDeletedObject(oid);
					existingRow = deletedObject.Row;
				}
			}
			
			if(existingRow == null)
			{
				newRow = mainDataSet.Tables[emap.TableName].NewRow();
				rowPending = newRow;
				mainDataSet.Tables[emap.TableName].Rows.Add(newRow);
				pkInit.InitializeRow(newRow, pkvalues);
				rowPending = null;
			}
			else if(existingRow.RowState == DataRowState.Detached)
			{
				RecycleRow(existingRow);
				newRow = existingRow;
				rowPending = newRow;
				mainDataSet.Tables[emap.TableName].Rows.Add(newRow);
				pkInit.InitializeRow(newRow, pkvalues);
				rowPending = null;
			}
			else // row state is Deleted
			{
				RecycleRow(existingRow);
				newRow = existingRow;
			}
			newObject = GetObjectForRow(newRow);

			RegisterForTableEvents(newRow.Table);

			// Our ObjectRelation objects have ignored the column changing events
			// so we need to resend them now.
			SendPkColumnChangingEvents(newRow);
				
			// Call Lifecycle method
			BindingFlags bflags = BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance;
			foreach(MethodInfo m in objectType.GetMethods(bflags))
			{
				if(m.IsDefined(typeof(LifecycleCreateAttribute), true))
					m.Invoke(newObject, null);
			}

			return newObject;
		}

		
		/// <summary>
		/// Takes a row and <c>null</c>s all non-PK values
		/// </summary>
		/// <param name="row">DataRow to be recycled</param>
		protected virtual void RecycleRow(DataRow row)
		{
			ArrayList	pkcolumns;

			rowPending = row;
			
			if(row.RowState == DataRowState.Deleted)
				row.RejectChanges();

			pkcolumns = new ArrayList(row.Table.PrimaryKey);
			foreach(DataColumn c in row.Table.Columns)
			{
				if(pkcolumns.Contains(c) == false)
					row[c] = DBNull.Value;
			}
			
			rowPending = null;
		}


		//--------------------------------------------------------------------------------------
		//	Deleting objects
		//--------------------------------------------------------------------------------------

		/// <summary>
		/// Marks object as deleted. This will deletes row from persistent storage when the 
		/// changes are saved.
		/// </summary>
		/// <param name="eo"><c>IEntityObject</c> to be removed</param>
		public virtual void DeleteObject(IEntityObject eo)
		{
			if(eo.Context != this)
				throw new ArgumentException("Object is not in this context.");
			logger.Debug("Deleting object of type " + eo.GetType().ToString());
			eo.Row.Delete();
		}

		/// <summary>
		/// Determines whether the change event is for a deletion, or rollback for an added row
		/// </summary>
		/// <param name="e">DataRowChangeEventArgs giving row properties</param>
		/// <returns><c>true</c> if the change event is for a deletion, or rollback for an added row</returns>
		protected virtual bool IsObjectDeleteEvent(DataRowChangeEventArgs e)
		{
			if(e.Action == DataRowAction.Delete)
				return true;
			if(e.Action == DataRowAction.Rollback)
				return (e.Row.RowState == DataRowState.Added);
			return false;
		}


		//--------------------------------------------------------------------------------------
		//	Getting objects (In-memory lookup)
		//--------------------------------------------------------------------------------------

		/// <summary>
		/// Gets all rows registered in the internal dataset
		/// </summary>
		/// <returns>all rows registered in the internal dataset</returns>
		public ICollection GetAllRegisteredObjects()
		{
			return objectTable.GetAllObjects();
		}


		/// <summary>
		/// Looks up the supplied <c>IEntityObject</c> in the internal object table
		/// </summary>
		/// <param name="eo"><c>IEntityObject</c> to find</param>
		/// <returns>internal representation of supplied <c>IEntityObject</c>, if found</returns>
		/// <remarks>
		/// Use this when you have a set of objects in a parent context and need references
		/// to them in a child context; or vice versa.
		/// </remarks>
		public IEntityObject GetLocalObject(IEntityObject eo)
		{
			IEntityMap emap = emapFactory.GetMap(eo.GetType());
			DataRowVersion lookupVersion = DataRowVersion.Current;
			if((eo.Row.RowState == DataRowState.Deleted) || (eo.Row.RowState == DataRowState.Detached))
				lookupVersion = DataRowVersion.Original;
			object[] pkvalues = GetPrimaryKeyValuesForRow(emap, eo.Row, lookupVersion);
			if((eo = GetObjectFromTable(emap.TableName, pkvalues)) == null)
				throw new ObjectNotFoundException("Object not available in this context.");
			return eo;
		}

		
		/// <summary>
		/// Creates a new list of <c>EntityObject</c>s based upon the supplied list,
		/// but using its internal representation of those objects
		/// </summary>
		/// <param name="eoList"></param>
		/// <returns></returns>
		public IList GetLocalObjects(IList eoList)
		{
			BindingFlags bflags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			IList localList = (IList)Activator.CreateInstance(eoList.GetType(), bflags, null, null, null);
			foreach(IEntityObject eo in eoList)
				localList.Add(GetLocalObject(eo));
			return localList;
		}
 
		public IList GetDeletedObjects()
		{
			return ObjectTable.GetDeletedObjects();
		}
 		
		//--------------------------------------------------------------------------------------
		//	Getting objects (Will fetch from store if neccessary)
		//--------------------------------------------------------------------------------------

		/// <summary>
		/// Gets object from storage
		/// </summary>
		/// <param name="tableName">name of table holding object</param>
		/// <param name="pkvalues">primary keys identifying object</param>
		/// <returns>object matching primary keys, if found</returns>
		/// <remarks>Any results are merged with this context. Tries to use cached version, if that is available</remarks>
		public virtual IEntityObject GetObjectFromTable(string tableName, object[] pkvalues)
		{
			IEntityObject	obj;
			ObjectId		oid;
			
			oid = new ObjectId(tableName, pkvalues);
			
			// First, try in memory
			if((obj = objectTable.GetObject(oid)) != null)
				return obj;

			// Now check whether it's deleted.
			if(objectTable.GetDeletedObject(oid) != null)
				return null;

			// Neither, so try the store
			if(CanLoadFromStore)
			{
				FetchObjectFromStore(emapFactory.GetMap(tableName), pkvalues);
				return objectTable.GetObject(oid);
			}
			
			return null;
		}


		/// <summary>
		/// Finds all rows containing the supplied value in the named column
		/// </summary>
		/// <param name="tableName">table name</param>
		/// <param name="columnName">column containing values to test</param>
		/// <param name="queryValue">value required for match</param>
		/// <returns>all matching rows</returns>
		/// <remarks>
		/// return rows are merged with this context
		/// </remarks>
		internal protected virtual IList GetObjectsFromTable(string tableName, string columnName, object queryValue)
		{
			IEntityMap	emap;
			IList		objects, result;

			emap = emapFactory.GetMap(tableName);
			if(CanLoadFromStore)
			{
				Qualifier q = new ColumnQualifier(columnName, new EqualsPredicate(queryValue));
				FetchSpecification fetchSpec = new FetchSpecification(emap, q);
				FetchObjectsFromStore(fetchSpec);
			}

			if((objects = objectTable.GetObjects(tableName)) == null)
				return new ArrayList();

			result = new ArrayList();
			foreach(IEntityObject eo in objects)
			{
				if(eo.Row[columnName].Equals(queryValue))
					result.Add(eo);
			}

			return result;
		}

		
		/// <summary>
		/// Returns all rows matching the supplied specification
		/// </summary>
		/// <param name="fetchSpec"><c>IFetchSpecification</c> object detailing objects to find</param>
		/// <returns>matching objects from table</returns>
		public virtual IList GetObjects(IFetchSpecification fetchSpec)
		{
			IList	objects, result;
			int		limit;

			// Always consult the data store if we have one because we can't be sure we 
			// have all objects in memory.
			if(CanLoadFromStore)
				FetchObjectsFromStore(fetchSpec);

			if(logger.IsDebugEnabled) logger.Debug("In-memory search for " + fetchSpec.ToString());
			
			if((objects = objectTable.GetObjects(fetchSpec.EntityMap.TableName)) == null)
				return new ArrayList();

			limit = fetchSpec.FetchLimit;
			if(fetchSpec.Qualifier == null)
			{
				if((limit == -1) || (limit > objects.Count) || (fetchSpec.SortOrderings != null))
				{
					result = new ArrayList(objects);
				}
				else
				{
					result = new object[limit];
					for(int i = 0; i < limit; i++)
						result[i] = objects[i];
				}
			}
			else
			{
				Qualifier q = fetchSpec.Qualifier;
				result = new ArrayList();
				foreach(IEntityObject eo in objects)
				{
					if(q.EvaluateWithObject(eo))
					{
						result.Add(eo);
						if((fetchSpec.SortOrderings == null) && (result.Count == limit))
							break;
					}
				}
			}
			if(fetchSpec.SortOrderings != null)
			{
				ArrayList objectsSorted = new ArrayList(result);
				fetchSpec.SortOrderings[0].Sort(objectsSorted);
				if((limit >= 0) && (limit < objectsSorted.Count))
					result = objectsSorted.GetRange(0, Math.Min(limit, objectsSorted.Count));
				else
					result = objectsSorted;
			}

			return result;
		}


		/// <summary>
		/// Gets all objects in other tables that have references to this object
		/// </summary>
		/// <param name="theObject"></param>
		/// <param name="excludeCascades"><t>true</t> to ignore reference from tables into
		/// which the objects table cascades deletes.</param>
		/// <returns>related objects</returns>
		/// <remarks>
		/// This method is useful to find out whether a given object can be deleted in
		/// the data store.
		/// </remarks>
		public IList GetReferencingObjects(IEntityObject theObject, bool excludeCascades)
		{
			ArrayList	referencingObjects;

			referencingObjects = new ArrayList();
			foreach(DataRelation r in theObject.Row.Table.ChildRelations)
			{
				if((excludeCascades) && (r.ChildKeyConstraint.DeleteRule == Rule.Cascade))
					continue;

				referencingObjects.AddRange(GetObjectsFromTable(r.ChildTable.TableName, r.ChildColumns[0].ColumnName, theObject.Row[r.ParentColumns[0]]));
			}
			return referencingObjects;
		}


		//--------------------------------------------------------------------------------------
		//	Fetching objects from the store
		//--------------------------------------------------------------------------------------

		protected virtual void FetchObjectFromStore(IEntityMap emap, object[] pkvalues)
		{
			FetchSpecification	fetchSpec;
			Qualifier			mainQualifier;
			DataTable			table;

			string[] pkcolumns = emap.PrimaryKeyColumns;				
			if(pkcolumns.Length == 1)
			{
				mainQualifier = new ColumnQualifier(pkcolumns[0], new EqualsPredicate(pkvalues[0]));
			}
			else
			{
				ArrayList qualifiers = new ArrayList(pkcolumns.Length);
				for(int i = 0; i < pkcolumns.Length; i++)
					qualifiers.Add(new ColumnQualifier(pkcolumns[i], new EqualsPredicate(pkvalues[i])));
				mainQualifier = new AndQualifier(qualifiers);
			}

			fetchSpec = new FetchSpecification(emap, mainQualifier);
			if(logger.IsDebugEnabled) logger.Debug("Querying store for " + fetchSpec.ToString() + " (PK)");
			table = dataStore.FetchRows(fetchSpec);

			if(table.Rows.Count == 0)
			{
				if(logger.IsInfoEnabled) logger.Info("No rows returned for " + fetchSpec.ToString() + " (PK)");
				throw new ObjectNotFoundException("No rows returned for " + fetchSpec.ToString() + " (PK)");
			}

			ImportRowsFromTable(table);
		}


		protected virtual void FetchObjectsFromStore(IFetchSpecification fetchSpec)
		{
			if(logger.IsDebugEnabled) logger.Debug("Querying store for " + fetchSpec.ToString());
			DataTable table = dataStore.FetchRows(fetchSpec);
			ImportRowsFromTable(table);
		}


		//--------------------------------------------------------------------------------------
		//	IDataStore Impl
		//--------------------------------------------------------------------------------------

		/// <summary>
		/// Gets rows from the data store according to the spefication
		/// </summary>
		/// <param name="fetchSpec">IFetchSpecification object stating what rows to fetch</param>
		/// <returns>Nothing</returns>
		/// <remarks>
		/// This method will also return newly created objects that have not been persisted yet.
		/// </remarks>
		public virtual DataTable FetchRows(IFetchSpecification fetchSpec)
		{
			Qualifier	q;
			IList		objects;
			DataTable	myTable, resultTable;

			myTable = mainDataSet.Tables[fetchSpec.EntityMap.TableName];
			if(myTable == null)
			{
				resultTable = new DataTable(fetchSpec.EntityMap.TableName);
				fetchSpec.EntityMap.UpdateSchema(resultTable, SchemaUpdate.Basic);
				return resultTable;
			}

			resultTable = myTable.Clone();

			if(CanLoadFromStore)
				FetchObjectsFromStore( fetchSpec );
			
			if((objects = objectTable.GetObjects(fetchSpec.EntityMap.TableName)) == null)
				return resultTable;

			if(fetchSpec.Qualifier == null)
			{
				foreach(IEntityObject eo in objects)
					resultTable.ImportRow(eo.Row);
			}
			else
			{
				q = new QualifierConverter(fetchSpec.EntityMap).ConvertPropertyQualifiers(fetchSpec.Qualifier);	
				foreach(IEntityObject eo in objects)
				{
					if(q.EvaluateWithObject(eo))
						resultTable.ImportRow(eo.Row);
				}
			}
			return resultTable;
		}


		/// <summary>
		/// Saves changes in the child context in this context.
		/// </summary>
		/// <remarks>
		/// This method should rarely ever be used in client code; use SaveChanges() in
		/// the child context instead.
		/// </remarks>
		/// <param name="childContext">child context that is to be saved</param>
		/// <returns>Always returns <c>null</c></returns>
		public virtual ICollection SaveChangesInObjectContext(ObjectContext childContext)
		{
			IEntityObject eo;
			DataSet		  changeSet;

			// Detached rows don't count as changes. But if the child deleted something
			// that we also have as added we need to delete it here, too.
			foreach(ObjectId oid in childContext.ObjectTable.GetAllDeletedObjectIds())
			{
				if(((eo = objectTable.GetObject(oid)) != null) && (eo.Row.RowState == DataRowState.Added))
					DeleteObject(eo);
			}

			if((changeSet = childContext.GetChanges()) == null)
				return null;
			MergeData(changeSet, false);
			// For some reason the dataset suspends sending DataColumnChanging events. (Even more
			// curious as it does send DataRowDeleting events.) In any case, our Relations rely
			// on these events, so we have to send them ourselves.
			foreach(DataRelation relation in changeSet.Relations)
			{
				if(relation.ChildColumns.Length != 1)
					throw new InvalidOperationException("Neo does not support compound foreign keys.");
				foreach(DataRow row in relation.ChildTable.Rows)
				{
					DataColumn				  fkColumn;
					object					  newValue;
					DataColumnChangeEventArgs eventArg;

					if((row.RowState == DataRowState.Deleted) || (row.RowState == DataRowState.Detached))
						continue;
					fkColumn = relation.ChildColumns[0];
					newValue = row[fkColumn];
					// the rows have the values in original/current but the values should be in
					// current/proposed, so undo the last accept...
					if(row.RowState == DataRowState.Modified)
						row.RejectChanges();
					eventArg = new DataColumnChangeEventArgs(row, row.Table.Columns[fkColumn.ColumnName], newValue);
					OnColumnChanging(mainDataSet.Tables[row.Table.TableName], eventArg);
				}
			}

			return null;
		}

	
		//--------------------------------------------------------------------------------------
		//	ISerializable Impl
		//--------------------------------------------------------------------------------------

		protected ObjectContext(SerializationInfo info, StreamingContext context) : this()
		{
			StringReader reader = new StringReader(info.GetString("xmlData"));
			DataSet ds = new DataSet();
			ds.ReadXml(reader, System.Data.XmlReadMode.ReadSchema);
			MergeData(ds, false);

			dataStore = (IDataStore)info.GetValue("dataStore", typeof(IDataStore));
			copiedParent = info.GetBoolean("copiedParent");
			
			Type emapFactoryType = (Type)info.GetValue("emapFactory", typeof(Type));
			if(emapFactoryType != null)
				emapFactory = (IEntityMapFactory)Activator.CreateInstance(emapFactoryType);
			
			extendedProperties = (PropertyCollection)info.GetValue("extendedProperties", typeof(PropertyCollection));
			
			logger.Debug("Deserialized object context.");
		}
		

		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if(rowPending != null)
				throw new InvalidOperationException("Cannot serialise a context while a row is changing.");

			StringWriter writer = new StringWriter();
			mainDataSet.WriteXml(writer, XmlWriteMode.WriteSchema);
			info.AddValue("xmlData", writer.ToString());
	
			info.AddValue("dataStore", dataStore);
			info.AddValue("copiedParent", copiedParent);

			if(emapFactory.Equals(DefaultEntityMapFactory.SharedInstance))
				info.AddValue("emapFactory", null);
			else
				info.AddValue("emapFactory", emapFactory.GetType());
			
			info.AddValue("extendedProperties", extendedProperties);
		}

	}
}