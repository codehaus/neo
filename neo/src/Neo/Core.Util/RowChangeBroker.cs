using System.Collections;
using System.Data;


namespace Neo.Core.Util
{
	public class RowChangeBroker
	{
		private Hashtable tableChangeListeners;

		public RowChangeBroker()
		{
			tableChangeListeners = new Hashtable();
		}

		public void RegisterForRowChanged(RowChangeHandler handler, string tableName)
		{
			IList listeners = GetTableListeners(tableName);

			listeners.Add(handler);
		}

		public void UnRegisterForRowChanged(RowChangeHandler handler, string tableName)
		{
			IList listeners = TableChangeListeners(tableName);

			if(listeners != null)
			{
				listeners.Remove(handler);
			}
		}

		private IList GetTableListeners(string tableName)
		{
			IList listeners = TableChangeListeners(tableName);

			if (listeners == null)
			{
				listeners = new ArrayList();
				tableChangeListeners[tableName] = listeners;
			}
			return listeners;
		}

		private IList TableChangeListeners(string tableName)
		{
			return tableChangeListeners[tableName] as IList;
		}

		public void OnRowDeleting(object sender, DataRowChangeEventArgs e)
		{
			string tableName = e.Row.Table.TableName;

			IList listeners = TableChangeListeners(tableName);

			if (listeners != null)
			{
				for(int i = 0; i < listeners.Count; i++)
				{
					((RowChangeHandler)listeners[i])(sender, e);
				}
			}
		}
	}
}