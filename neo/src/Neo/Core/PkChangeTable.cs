using System.Collections;


namespace Neo.Core
{
	/// <summary>
	/// Used to redistribute database generated keys.
	/// </summary>
	/// <remarks>
	/// When working with database generated (native) primary keys, Neo uses temporary (negative) 
	/// primary key values until an object is saved to the store. The data stores must provide a 
	/// mapping from these temporary keys to the actual db values by returning a 
	/// <c>PkChangeTable</c> from the SaveChangesInObjectContext method.
	/// <code>
	/// table = new PkChangeTable("jobs");
	/// table.AddPkChange(row["job_id"], actualKeyVal);</code>
	/// When an <c>ObjectContext</c> is connected directly to an <c>IDataStore</c> it handles the 
	/// key mapping internally but in distributed environments it is possible to pass the change 
	/// table between different processes as follows.
	/// <code>
	/// ds = clientContext.GetChanges();
	/// // now this ds is passed to the server, which could use another context
	/// serverContext.ImportData(ds)
	/// pkctArray = serverContext.SaveChanges();
	/// // the pkctArray is passed back to the client process which needs to apply them
	/// clientContext.UpdatePrimaryKeys(pkctArray);
	/// clientContext.AcceptChanges();
	/// </code>
	/// </remarks>
	public class PkChangeTable
	{
		//--------------------------------------------------------------------------------------
		//	Nested data type
		//--------------------------------------------------------------------------------------

		/// <summary>
		/// A simple <c>struct</c> holding the old and new values for a change.
		/// </summary>
		public struct ChangeRecord
		{
			/// <summary>
			/// Original primary key value
			/// </summary>
			public object OldValue;

			/// <summary>
			/// New primary key value
			/// </summary>
			public object NewValue;
		}


		//--------------------------------------------------------------------------------------
		//	Fields and Constructor
		//--------------------------------------------------------------------------------------

		private string tableName;
		private ArrayList changeList;

		/// <summary>
		/// Default constructor. Creates a blank change table for a database table
		/// </summary>
		/// <param name="aTableName">name of table whose primary keys are changing</param>
		public PkChangeTable(string aTableName)
		{
			tableName = aTableName;
			changeList = new ArrayList();
		}


		//--------------------------------------------------------------------------------------
		//	Accessor methods
		//--------------------------------------------------------------------------------------

		/// <summary>
		/// Name of table for this <c>PkChangeTable</c> object
		/// </summary>
		public string TableName
		{
			get { return tableName; }
		}

		/// <summary>
		/// Count of primary key changes for this table
		/// </summary>
		public int Count
		{
			get { return changeList.Count; }
		}
		
		/// <summary>
		/// Returns <c>ChangeRecord</c> struct at this index
		/// </summary>
		public ChangeRecord this[int index]
		{
			get { return (ChangeRecord)changeList[index]; }
		}

		/// <summary>
		/// Returns an enumarator for the changes and thus allows the use of <c>foreach</c> 
		/// with the table.
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator()
		{
            return changeList.GetEnumerator();
        }
        
		/// <summary>
		/// Adds a new entry to the table.
		/// </summary>
		/// <param name="oldValue">original primary key value</param>
		/// <param name="newValue">new primary key value</param>
		public void AddPkChange(object oldValue, object newValue)
		{
			ChangeRecord changeRecord;

			changeRecord = new ChangeRecord();
			changeRecord.OldValue = oldValue;
			changeRecord.NewValue = newValue;
			changeList.Add(changeRecord);
		}


	}
}
