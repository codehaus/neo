using System;

namespace Neo.Framework
{

	[AttributeUsage(AttributeTargets.Class)]
	public class NeoEntityAttribute : System.Attribute
	{
		private string tableName;

		public string TableName
		{
			set { tableName = value; }
			get { return tableName; }
		}

	}


	[AttributeUsage(AttributeTargets.Property)]
	public class NeoPropertyAttribute : System.Attribute
	{
		private string column;

		public string Column
		{
			set { column = value; }
			get { return column; }
		}

	}

	
	[AttributeUsage(AttributeTargets.Property)]
	public class NeoPrimaryKey : System.Attribute
	{
		private	string primaryKey;
		private string idMethod;

		public string PrimaryKey
		{
			set { primaryKey = value; }
			get { return primaryKey; }
		}

		public string IdMethod
		{
			set { idMethod = value; }
			get { return idMethod; }
		}

	}


	public class NeoRelationAttribute : System.Attribute
	{
		private string foreignEntity;
		private string localKey;
		private string foreignKey;
		private string deleteRule;

		public string ForeignEntity
		{
			set { foreignEntity = value; }
			get { return foreignEntity; }
		}

		public string LocalKey
		{
			set { localKey = value; }
			get { return localKey; }
		}

		public string ForeignKey
		{
			set { foreignKey = value; }
			get { return foreignKey; }
		}

		public string DeleteRule
		{
			set { deleteRule = value; }
			get { return deleteRule; }
		}
	
	}


	[AttributeUsage(AttributeTargets.Property)]
	public class NeoToOneRelationAttribute : NeoRelationAttribute
	{
	}

	[AttributeUsage(AttributeTargets.Property)]
	public class NeoToManyRelationAttribute : NeoRelationAttribute
	{
	}


}
