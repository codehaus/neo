using System;
using System.Collections;


namespace Neo.Core
{

	public class PkChangeTable
	{
		//--------------------------------------------------------------------------------------
		//	Nested data type
		//--------------------------------------------------------------------------------------

		public struct ChangeRecord
		{
			public object OldValue;
			public object NewValue;
		}


		//--------------------------------------------------------------------------------------
		//	Fields and Constructor
		//--------------------------------------------------------------------------------------

		private string tableName;
		private ArrayList changeList;

		public PkChangeTable(string aTableName)
		{
			tableName = aTableName;
			changeList = new ArrayList();
		}


		//--------------------------------------------------------------------------------------
		//	Accessor methods
		//--------------------------------------------------------------------------------------

		public string TableName
		{
			get { return tableName; }
		}

		
		public int Count
		{
			get { return changeList.Count; }
		}
		
		public ChangeRecord this[int index]
		{
			get { return (ChangeRecord)changeList[index]; }
		}


		public IEnumerator GetEnumerator()
		{
            return changeList.GetEnumerator();
        }
        
  
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
