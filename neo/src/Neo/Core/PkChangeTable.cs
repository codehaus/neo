using System;
using System.Collections;


namespace Neo.Core
{
	/// <summary>
	/// Used to redistribute database generated keys.
	/// </summary>
	/// <remarks>
	/// Neo generates temporary (negative) primary key values. Data stores must provide a mapping from these temporary keys to the actual db values.
	/// <code>
	/// table = new PkChangeTable("jobs");
	/// table.AddPkChange(row["job_id"], actualKeyVal);</code>
	/// Use in distributed environments:
	/// <code>
	/// ds = clientContext.GetChanges();
	/// serverContext.RegisterObjectsForDataSet(ds)
	/// pkctArray = serverContext.SaveChanges();
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
		/// Simple <c>struct</c> holding new and old values
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
		/// Default constructor. Creates a blank change list for this table
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
		/// Allows the <c>foreach</c> keyword to work
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator()
		{
            return changeList.GetEnumerator();
        }
        
		/// <summary>
		/// Adds a new entry to the internal change records
		/// </summary>
		/// <param name="oldValue">original primary key value</param>
		/// <param name="newValue">new primary key value</param>
		public void AddPkChange(object oldValue, object newValue)
		{
			ChangeRecord	changeRecord;

			changeRecord = new ChangeRecord();
			changeRecord.OldValue = oldValue;
			changeRecord.NewValue = newValue;
			changeList.Add(changeRecord);
		}


	}
}
