using System;
using System.Reflection;
using System.Globalization;
using System.Collections;
using System.Data;
using System.Text;
using System.Threading;
using Neo.Core.Util;
using log4net;


namespace Neo.Core
{
	/// <summary>
	/// Summary description for ObjectContext.
	/// </summary>
	public class ObjectContext : IDataStore
	{

		//--------------------------------------------------------------------------------------
		//	Static fields and methods
		//--------------------------------------------------------------------------------------

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


		public ObjectContext()
		{
			if(logger == null)
				logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString());

			emapFactory = DefaultEntityMapFactory.SharedInstance;
			
			// If you add code here, you might want to add it to Clear() as well
			objectTable = new ObjectTable();
			mainDataSet = new DataSet();
			mainDataSet.EnforceConstraints = false;
			eventHandlers = new Hashtable();
			
			logger.Debug("Initialised new object context.");
		}


		public ObjectContext(DataSet aDataSet) : this()
		{
			MergeData(aDataSet, false);
		}

		
		public ObjectContext(IDataStore aDataStore) : this()
		{
			dataStore = aDataStore;

			if(aDataStore is ObjectContext)
			{
				MergeData(((ObjectContext)aDataStore).DataSet, false);
				copiedParent = true;
			}
		}


		//--------------------------------------------------------------------------------------
		//	Public properties
		//--------------------------------------------------------------------------------------

		public DataSet DataSet
		{
			get { return mainDataSet; }
		}


		public IDataStore DataStore
		{
			get { return dataStore; }
		}


		public IEntityMapFactory EntityMapFactory
		{
			set { emapFactory = value; }
			get { return emapFactory; }
		}


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

		
		protected internal DataRow RowPending
		{
			get { return rowPending; }
		}


		protected ILog Logger
		{
			get { return logger; }
		}


		protected bool CanLoadFromStore
		{
			get { return (dataStore != null) && ((dataStore is ObjectContext == false) || (copiedParent == false)); }
		}


		//--------------------------------------------------------------------------------------
		//	Dealing with change
		//--------------------------------------------------------------------------------------

		public virtual void Clear()
		{
			objectTable = new ObjectTable();
			mainDataSet = new DataSet();
			mainDataSet.EnforceConstraints = false;
			eventHandlers = new Hashtable();
		}


		public virtual DataSet GetChanges()
		{
			return mainDataSet.GetChanges();
		}


		public virtual bool HasChanges()
		{
			return mainDataSet.HasChanges();
		}
		

		public virtual void AcceptChanges()
		{
			mainDataSet.AcceptChanges();
			objectTable.ForgetAllDeletedObjects();
		}

	
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

		protected virtual void RegisterForTableEvents(DataTable table)
		{
			if(eventHandlers[table.TableName] != null)
				return;

			eventHandlers[table.TableName] = true;
			table.RowDeleting += new DataRowChangeEventHandler(this.RowEvent);
			table.RowChanging += new DataRowChangeEventHandler(this.RowEvent);
		}

		
		protected internal virtual void SendColumnChangingEvents(DataTable table, DataColumnChangeEventArgs eventArg)
		{
			object[] args;
			
			args = new object[] { eventArg };
			BindingFlags bflags = BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance;
			typeof(DataTable).InvokeMember("OnColumnChanging", bflags, null, table, args, CultureInfo.CurrentCulture);
		}


		protected internal virtual void SendPkColumnChangingEvents(DataRow row)
		{
			foreach(DataColumn c in row.Table.PrimaryKey)
			{
				DataColumnChangeEventArgs eventArg = new DataColumnChangeEventArgs(row, c, row[c]);
				SendColumnChangingEvents(row.Table, eventArg);
			}
		}


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


		protected virtual IEntityObject GetObjectForRow(DataRow aRow)
		{
			IEntityMap	  emap;
			object[]	  pkvalues;
			ObjectId	  oid;
			IEntityObject eo;
				
			emap = emapFactory.GetMap(aRow.Table.TableName);
			pkvalues = GetPrimaryKeyValuesForRow(emap, aRow, DataRowVersion.Current);
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

			// If we don't have an object yet, create it.
			if((eo = objectTable.GetObject(oid)) == null)
			{
				BindingFlags bflags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
				object[] args = { aRow, this };
				eo = (IEntityObject)Activator.CreateInstance(emap.ObjectType, bflags, null, args, null);
				objectTable.AddObject(oid, eo);
			}
			else
			{
				if(eo.Row.Equals(aRow) == false)
					throw new InvalidOperationException("Internal inconsistency; object exists but references different row.");
			}
			return eo;
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

		public IList MergeData(DataSet aDataSet)
		{
			return MergeData(aDataSet, true);
		}

		protected virtual IList MergeData(DataSet aDataSet, bool sendColChangeEvents)
		{
			ArrayList	objectList;
			DataTable	table;
			IEntityObject	eo;

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
					if((r.RowState != DataRowState.Deleted) && ((eo = GetObjectForRow(r)) != null))
					{
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
						SendColumnChangingEvents(mainDataSet.Tables[row.Table.TableName], eventArg);
					}
				}
				logger.Debug("Finished sending post merge events.");
			}

			return objectList;
		}


		//--------------------------------------------------------------------------------------
		//	Updating Primary Keys
		//--------------------------------------------------------------------------------------

		public virtual void UpdatePrimaryKeys(ICollection changeTables)
		{
			foreach(PkChangeTable t in changeTables)
				UpdatePrimaryKeys(t); 
		}

		public virtual void UpdatePrimaryKeys(PkChangeTable changeTable)
		{
			IEntityMap	  emap;
			ObjectId	  oid;
			IEntityObject eo;

			// What happens if we have sub contexts? How will they find out about these changes?
			// They won't and saving there changes will duplicate newly inserted objects...

			emap = emapFactory.GetMap(changeTable.TableName);
			if(emap.PrimaryKeyColumns.Length > 1)
				throw new ArgumentException("PkChangeTable refers to a table that has a compound primary key.");
			// WARNING: This is a bit naive. What happens when you create a Title as well as a
			// TitleAuthor? If the TitleAuthor is updated first, the TitleAuthorRelation in 
			// Title will drop it because it doesn't match its owner's PK...
			foreach(PkChangeTable.ChangeRecord r in changeTable)
			{
				oid = new ObjectId(changeTable.TableName, new object[] { r.OldValue });
				if((eo = objectTable.GetObject(oid)) == null)
					throw new ArgumentException("PkChangeTable references unknown object " + oid.ToString());
				eo.Row[emap.PrimaryKeyColumns[0]] = r.NewValue;
			}

		}
		 

		//--------------------------------------------------------------------------------------
		//	Creating objects
		//--------------------------------------------------------------------------------------

		public virtual IEntityObject CreateObject(Type objectType, object[] pkvalues)
		{
			IEntityMap		emap;
			IPkInitializer	pkInit;
			ObjectId		oid;
			DataRow			newRow, existingRow;
			IEntityObject	newObject, deletedObject;

			logger.Debug("Creating object of type " + objectType.ToString());

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

			RegisterForTableEvents(newRow.Table);

			return newObject;
		}

		
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

		public virtual void DeleteObject(IEntityObject eo)
		{
			if(eo.Context != this)
				throw new ArgumentException("Object is not in this context.");
			logger.Debug("Deleting object of type " + eo.GetType().ToString());
			eo.Row.Delete();
		}


		protected virtual void RowEvent(object sender, DataRowChangeEventArgs e)
		{
			if(e.Row == rowPending)
				return;

			if(IsObjectDeleteEvent(e) == false)
				return;

			// Normally, to read the PK we have to access the original version but
			// if the child row which is being deleted was new (i.e. just added), we
			// must not access the original version (which it doesn't have because it 
			// is new.) but the current version...
			DataRowVersion lookupVersion = DataRowVersion.Original;
			if(e.Row.RowState == DataRowState.Added)
				lookupVersion = DataRowVersion.Current;

			IEntityMap emap = emapFactory.GetMap(e.Row.Table.TableName);
			object[] pkvalues = GetPrimaryKeyValuesForRow(emap, e.Row, lookupVersion);
			objectTable.DeleteObject(new ObjectId(e.Row.Table.TableName, pkvalues));
		}

	
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

		public ICollection GetAllRegisteredObjects()
		{
			return objectTable.GetAllObjects();
		}


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

		
		public IList GetLocalObjects(IList eoList)
		{
			BindingFlags bflags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			IList localList = (IList)Activator.CreateInstance(eoList.GetType(), bflags, null, null, null);
			foreach(IEntityObject eo in eoList)
				localList.Add(GetLocalObject(eo));
			return localList;
		}
 
 		
		//--------------------------------------------------------------------------------------
		//	Getting objects (Will fetch from store if neccessary)
		//--------------------------------------------------------------------------------------

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


		internal protected virtual IList GetObjectsFromTable(string tableName, string columnName, object queryValue)
		{
			IEntityMap	emap;
			IList		objects, result;

			emap = emapFactory.GetMap(tableName);
			if(CanLoadFromStore)
			{
				Qualifier q = new ColumnQualifier(columnName, QualifierOperator.Equal, queryValue);
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
				if(limit == -1)
				{
					result = objects;
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
						if(--limit == 0)
							break;
					}
				}
			}

			return result;
		}


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
				mainQualifier = new ColumnQualifier(pkcolumns[0], QualifierOperator.Equal, pkvalues[0]);
			}
			else
			{
				ArrayList qualifiers = new ArrayList(pkcolumns.Length);
				for(int i = 0; i < pkcolumns.Length; i++)
					qualifiers.Add(new ColumnQualifier(pkcolumns[i], QualifierOperator.Equal, pkvalues[i]));
				mainQualifier = new ClauseQualifier(QualifierConjunctor.And, qualifiers);
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

			if((objects = objectTable.GetObjects(fetchSpec.EntityMap.TableName)) == null)
				return resultTable;

			q = fetchSpec.Qualifier;
			if(q is ClauseQualifier)
				q = ((ClauseQualifier)q).GetWithColumnQualifiers(fetchSpec.EntityMap);
			else if(q is PropertyQualifier)
				q = new ColumnQualifier((PropertyQualifier)q, fetchSpec.EntityMap);

			foreach(IEntityObject eo in objects)
			{
				if((q == null) || (q.EvaluateWithObject(eo)))
					resultTable.ImportRow(eo.Row);
			}
			
			return resultTable;
		}


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
					object					  prevValue;
					DataColumnChangeEventArgs eventArg;

					if((row.RowState == DataRowState.Deleted) || (row.RowState == DataRowState.Detached))
						continue;
					fkColumn = relation.ChildColumns[0];
					prevValue = row[fkColumn];
					// the rows have the values in original/current but the values should be in
					// current/proposed, so undo the last accept...
					if(row.RowState == DataRowState.Modified)
						row.RejectChanges();
					eventArg = new DataColumnChangeEventArgs(row, row.Table.Columns[fkColumn.ColumnName], prevValue);
					SendColumnChangingEvents(mainDataSet.Tables[row.Table.TableName], eventArg);
				}
			}

			return null;
		}

	
	}


}
