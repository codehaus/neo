using System;
using System.Collections.Specialized;
using System.Xml;
using Neo.Model;


namespace Neo.Model.Impl
{
	public class AttributeImpl : IAttribute
	{
		//--------------------------------------------------------------------------------------
		//	fields and constructor
		//--------------------------------------------------------------------------------------

		private EntityImpl	entity;

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
	

		public AttributeImpl(EntityImpl anEntity)
		{
			entity = anEntity;
		}


		//--------------------------------------------------------------------------------------
		//	INeoProperty impl
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


	}
}
