using System;


namespace Neo.MetaModel
{
	public class EntityAttribute
	{
		//--------------------------------------------------------------------------------------
		//	fields and constructor
		//--------------------------------------------------------------------------------------

		private String		propertyName;
		private Type		propertyType;
		private String		propertyAttributes; /* custom attributes in the .NET sense */

		private String		columnName;
		private String		columnType;
		private String		size;

		private Boolean		isPkColumn;
		private Boolean		allowsNull;
		private Boolean		isRequired;
		private Boolean		isHidden;
	
		private String		defaultValue; 
	

		public EntityAttribute(Entity anEntity)
		{
		}


		//--------------------------------------------------------------------------------------
		//	public attributes
		//--------------------------------------------------------------------------------------

		public string PropertyName
		{
			set { propertyName = value; }
			get { return propertyName; }
		}

		public Type PropertyType
		{
			set { propertyType = value; }
			get { return propertyType; }
		}

		public String PropertyAttributes
		{
			set { propertyAttributes = value; }
			get { return propertyAttributes; }
		}

		public string ColumnName
		{
			set { columnName = value; }
			get { return columnName; }
		}

		public string ColumnType
		{
			set { columnType = value; }
			get { return columnType; }
		}

		public string Size
		{
			set { size = value; }
			get { return size; }
		}

		public bool AllowsNull
		{
			set { allowsNull = value; }
			get { return allowsNull; }
		}

		public bool IsPkColumn
		{
			set { isPkColumn = value; }
			get { return isPkColumn; }
		}

		public bool IsHidden
		{
			set { isHidden = value; }
			get { return isHidden; }
		}

		public bool IsRequired
		{
			set { isRequired = value; }
			get { return isRequired; }
		}


		public string DefaultValue 
		{
			get { return defaultValue; }
			set { defaultValue = value; }
		}


		//--------------------------------------------------------------------------------------
		//	compatibility
		//--------------------------------------------------------------------------------------

		public string DotNetName
		{
			get { return PropertyName; }
		}


		public string DotNetType
		{
			get { return PropertyType.FullName; }
		}


		//--------------------------------------------------------------------------------------
		//  helper methods
		//--------------------------------------------------------------------------------------

		public void SetDefaultColumnType(Type dataType)
		{
			if(dataType.Equals(typeof(Boolean)))
				columnType = "BIT";
			else if(dataType.Equals(typeof(Int16)))
				columnType =  "SMALLINT";
			else if(dataType.Equals(typeof(Int32)))
				columnType =  "INTEGER";
			else if(dataType.Equals(typeof(Int64)))
				columnType =  "BIGINT";
			else if(dataType.Equals(typeof(Double)))
				columnType =  "DOUBLE";
			else if(dataType.Equals(typeof(Decimal)))
				columnType =  "DECIMAL";
			else if(dataType.Equals(typeof(String)))
				columnType =  "VARCHAR";
			else if(dataType.Equals(typeof(DateTime)))
				columnType =  "DATETIME";
			else if(dataType.Equals(typeof(byte[])))
				columnType =  "BINARY";
			else if(dataType.Equals(typeof(Guid)))
				columnType =  "UNIQUEIDENTIFIER";
			else
				throw new ArgumentException("Data type " + dataType + " not supported.");
		}

	}
}
