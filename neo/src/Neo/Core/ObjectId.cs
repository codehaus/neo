using System;
using System.Text;
using System.Data;


namespace Neo.Core
{
	/// <summary>
	/// A unique identifier for an object comprising its table name and pk values. You should not
	/// need to access instances of ObjectId directly.
	/// </summary>
	public class ObjectId
	{
		//--------------------------------------------------------------------------------------
		// Fields and constructor
		//--------------------------------------------------------------------------------------

		public readonly string		TableName;
		public readonly object[]	PkValues;


		public ObjectId(string tableName, object[] pkvalues)
		{
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
			switch(PkValues.Length)
			{
			case 0:
				return 0;
			case 1:
				return TableName.GetHashCode() ^ PkValues[0].GetHashCode();
			default:
				return TableName.GetHashCode() ^ PkValues[0].GetHashCode() ^ PkValues[1].GetHashCode();
			}
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
				if(PkValues[i].Equals(other.PkValues[i]) == false)
					return false;
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
