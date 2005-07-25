using System;
using System.Data;
using System.Text;


namespace Neo.Core.Util
{
	/// <summary>
	/// A unique identifier for an object comprising its table name and pk values. You should not
	/// need to access instances of ObjectId directly.
	/// </summary>
	[Serializable]
	public class ObjectId
	{
		//--------------------------------------------------------------------------------------
		// Static methods
		//--------------------------------------------------------------------------------------

		public static ObjectId GetObjectIdForObject(IEntityObject eo)
		{
			IEntityMap emap = eo.Context.EntityMapFactory.GetMap(eo.Row.Table.TableName);
			object[] pkvalues = eo.Context.GetPrimaryKeyValuesForRow(emap, eo.Row, DataRowVersion.Current);
			return new ObjectId(emap.TableName, pkvalues);
		}

		public static ObjectId GetObjectIdForRow(DataRow row)
		{
			DataColumn[] columns = row.Table.PrimaryKey;
			object[] values = new object[columns.Length];
			for(int i = 0; i < columns.Length; i++)
				values[i] = row[columns[i], (row.RowState == DataRowState.Deleted) ? DataRowVersion.Original : DataRowVersion.Current];
			return new ObjectId(row.Table.TableName, values);
		}


		//--------------------------------------------------------------------------------------
		// Fields and constructor
		//--------------------------------------------------------------------------------------

		public readonly string		TableName;
		public readonly object[]	PkValues;


		public ObjectId(string tableName, object[] pkvalues)
		{
			if(tableName == null)
				throw new ArgumentException("ObjectId: TableName cannot be null");
			if(pkvalues == null)
				throw new ArgumentException("ObjectId: PkValues cannot be null");
			TableName = tableName;
			PkValues = pkvalues;
		}


		public ObjectId(string tableName, object pkvalue) : this(tableName, new object[] { pkvalue })
		{
		}


		//--------------------------------------------------------------------------------------
		// Custom equals/hashcode
		//--------------------------------------------------------------------------------------

		public override int GetHashCode()
		{
			int	hashcode;

			switch(PkValues.Length)
			{
			case 0:
				hashcode = 0;
				break;
			case 1:
				hashcode = TableName.GetHashCode() ^ PkValues[0].GetHashCode();
				break;
			default:
				hashcode = TableName.GetHashCode() ^ PkValues[0].GetHashCode() ^ PkValues[1].GetHashCode();
				break;
			}
			return hashcode;
		}


		public override bool Equals(object anObject)
		{
			ObjectId other;

			if((other = anObject as ObjectId) == null)
				return false;
			if(TableName.Equals(other.TableName) == false)
				return false;
			if(PkValues.Length != other.PkValues.Length)
				return false;
			for(int i = 0; i < PkValues.Length; i++)
			{
				if(PkValues[i].Equals(other.PkValues[i]) == false)
					return false;
			}
			return true;
		}


		//--------------------------------------------------------------------------------------
		//	Look nice
		//--------------------------------------------------------------------------------------

		public override string ToString()
		{	
		    StringBuilder	sb;
			bool			isFirstValue;

			sb = new StringBuilder("{");
			sb.Append(TableName);
			sb.Append(": ");
			isFirstValue = true;
			foreach(object o in PkValues)
			{
				if(isFirstValue == false)
					sb.Append(", ");
				isFirstValue = false;
				sb.Append(o.ToString());
			}
			sb.Append("}");

			return sb.ToString();
		}
	}
}
