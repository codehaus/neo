using System;

namespace Neo.Core.Util
{

	//--------------------------------------------------------------------------------------
	//	Equals
	//--------------------------------------------------------------------------------------

	public class EqualsPredicate : IPredicate
	{
		protected object predValue;

		public EqualsPredicate(object aValue)
		{
			predValue = aValue;
		}

		public object Value
		{
			get { return predValue; }
		}

		public virtual bool IsTrueForValue(object aValue, object nullVal)
		{
			if((aValue != nullVal) && (predValue != nullVal) && (predValue.GetType() != aValue.GetType()) && (predValue is IConvertible))
				predValue = (IComparable)Convert.ChangeType(predValue, aValue.GetType());

			if(predValue == nullVal)
				return (aValue == nullVal);
			return predValue.Equals(aValue);
		}

		public override string ToString()
		{
			return (predValue != null) ? ("= " + predValue.ToString()) : "null";
		}
	}

	//--------------------------------------------------------------------------------------
	//	NotEqual
	//--------------------------------------------------------------------------------------

	public class NotEqualPredicate : EqualsPredicate
	{
		public NotEqualPredicate(object aValue) : base(aValue)
		{
		}

		public override bool IsTrueForValue(object aValue, object nullVal)
		{
			return !base.IsTrueForValue(aValue, nullVal);
		}

		public override string ToString()
		{
			return (predValue != null) ? ("!= " + predValue.ToString()) : "null";
		}
	}

}
