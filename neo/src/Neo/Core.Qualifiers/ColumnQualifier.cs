using System;
using System.Reflection;
using System.Data;


namespace Neo.Core.Qualifiers
{

	public sealed class ColumnQualifier : Qualifier
	{
		//--------------------------------------------------------------------------------------
		//	Fields and Constructor
		//--------------------------------------------------------------------------------------

		private string		column;
		private IPredicate	predicate;


		public ColumnQualifier(string aColumn, IPredicate aPredicate) : base()
		{
			column = aColumn;
			predicate = aPredicate;
			//predicate = (aValue == null) ? DBNull.Value : aValue;
		}


		//--------------------------------------------------------------------------------------
		//	Public properties
		//--------------------------------------------------------------------------------------

		public string Column
		{
			get { return column; }
		}

		public IPredicate Predicate
		{
			get { return predicate; }
		}


		//--------------------------------------------------------------------------------------
		//	ToString() override
		//--------------------------------------------------------------------------------------

		public override string ToString()
		{
			return String.Format("row[{0}] {1}", Column, predicate.ToString());
		}

		
		//--------------------------------------------------------------------------------------
		//	Evaluation
		//--------------------------------------------------------------------------------------

		public override bool EvaluateWithObject(IEntityObject anObject)
		{
			return predicate.IsTrueForValue(anObject.Row[column], DBNull.Value);
		}

		
		//--------------------------------------------------------------------------------------
		//	Visitor
		//--------------------------------------------------------------------------------------

		public override object  AcceptVisitor(IQualifierVisitor v)
		{
			return v.VisitColumnQualifier(this);
		}
	
	}

}
