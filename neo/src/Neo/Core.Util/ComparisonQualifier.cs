using System;

namespace Neo.Core.Util
{
	public enum QualifierOperator
	{
		Equal,
		NotEqual,
		GreaterThan,
		LessThan
	}

	public abstract class ComparisonQualifier : Qualifier
	{
		//--------------------------------------------------------------------------------------
		//	Fields and Constructor
		//--------------------------------------------------------------------------------------

		protected QualifierOperator	op;
		protected object			compVal;

		internal ComparisonQualifier()
		{
		}

		
		//--------------------------------------------------------------------------------------
		//	Public properties
		//--------------------------------------------------------------------------------------

		public QualifierOperator Operator
		{
			get { return op; }
		}

		public object Value
		{
			get { return compVal; }
		}


		//--------------------------------------------------------------------------------------
		//	Helper
		//--------------------------------------------------------------------------------------

		protected string OperatorForToString
		{
			get
			{
				switch(Operator)
				{
				case QualifierOperator.Equal:		return "=";
				case QualifierOperator.NotEqual:	return "!=";
				case QualifierOperator.LessThan:	return "<";
				case QualifierOperator.GreaterThan: return ">";
				default:							return Operator.ToString();
				}
			}
		}
		

		protected bool Compare(object objVal, object nullVal)
		{
			if((objVal != nullVal) && (compVal != nullVal) && (compVal.GetType() != objVal.GetType()) && (compVal is IConvertible))
				compVal = Convert.ChangeType(compVal, objVal.GetType());

			switch(op)
			{
			case QualifierOperator.Equal:
				if(compVal == nullVal)
					return (objVal == nullVal);
				return compVal.Equals(objVal);

			case QualifierOperator.NotEqual:
				if(compVal == nullVal)
					return (objVal != nullVal);
				return compVal.Equals(objVal) == false;

			case QualifierOperator.LessThan:
				if((compVal == nullVal) || (objVal == nullVal))
					return false;
				return ((IComparable)compVal).CompareTo(objVal) > 0;
		 
			case QualifierOperator.GreaterThan:
				if((compVal == nullVal) || (objVal == nullVal))
					return false;
				return ((IComparable)compVal).CompareTo(objVal) < 0;

			default:
				throw new InvalidOperationException(String.Format("Invalid operator for qualifier."));
			}
		}
	}
}
