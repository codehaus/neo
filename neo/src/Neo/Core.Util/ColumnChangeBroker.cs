using System.Collections;
using System.Data;

namespace Neo.Core.Util
{
	public class ColumnChangeBroker
	{
		private Hashtable tableChangeListeners;

		public ColumnChangeBroker()
		{
			tableChangeListeners = new Hashtable();
		}

		public void RegisterForColumnChanging(ColumnChangeHandler handler, string tableName, string columnName)
		{
			Hashtable listeners = GetTableListeners(tableName);

			AddListener(listeners, handler, columnName);
		}

		private Hashtable GetTableListeners(string tableName)
		{
			Hashtable listeners = TableChangeListeners(tableName);

			if (listeners == null)
			{
				listeners = new Hashtable();
				tableChangeListeners[tableName] = listeners;
			}
			return listeners;
		}

		private Hashtable TableChangeListeners(string tableName)
		{
			return tableChangeListeners[tableName] as Hashtable;
		}

		private IList GetListeners(string tableName, string columnName)
		{
			Hashtable tableListeners = TableChangeListeners(tableName);
			if (tableListeners == null)
				return null;

			IList columnListeners = ColumnListeners(tableListeners, columnName);
			if (columnListeners == null)
				return null;

			return columnListeners;
		}

		public void UnRegisterForColumnChanging(ColumnChangeHandler handler, string tableName, string columnName)
		{
			Hashtable listeners = GetTableListeners(tableName);
			RemoveListener(listeners, handler, columnName);
		}

		private void AddListener(Hashtable listeners, ColumnChangeHandler handler, string columnName)
		{
			IList columnListeners = GetColumnListeners(listeners, columnName);
			columnListeners.Add(handler);
		}

		private void RemoveListener(Hashtable listeners, ColumnChangeHandler handler, string columnName)
		{
			IList columnListeners = GetColumnListeners(listeners, columnName);
			columnListeners.Remove(handler);
		}

		private IList GetColumnListeners(Hashtable listeners, string columnName)
		{
			IList columnListeners = ColumnListeners(listeners, columnName);
			if (columnListeners == null)
			{
				columnListeners = new ArrayList();
				listeners[columnName] = columnListeners;
			}
			return columnListeners;
		}

		private IList ColumnListeners(Hashtable listeners, string columnName)
		{
			return listeners[columnName] as IList;
		}

		public void OnColumnChanging(object sender, DataColumnChangeEventArgs e)
		{
			string tableName = e.Column.Table.TableName;
			string columnName = e.Column.ColumnName;

			IList listeners = GetListeners(tableName, columnName);

			if (listeners != null)
			{
				for(int i = 0; i < listeners.Count; i++)
					((ColumnChangeHandler)listeners[i])(sender, e);
			}
		}
	}
}