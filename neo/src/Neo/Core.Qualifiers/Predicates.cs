using System;


namespace Neo.Core.Qualifiers
{

	//--------------------------------------------------------------------------------------
	//	Equals
	//--------------------------------------------------------------------------------------

	/// <summary>
	/// Predicate that returns true if the value is equal to the value stored in the 
	/// predicate.
	/// </summary>
	public class EqualsPredicate : IMutablePredicate
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
			if(predValue == nullVal)
				return (aValue == nullVal);
			if(aValue == nullVal)
				return false;

			if((predValue.GetType() != aValue.GetType()) && (predValue is IConvertible))
				predValue = (IComparable)Convert.ChangeType(predValue, aValue.GetType());
			
			return predValue.Equals(aValue);
		}

		public void SetPredicateValue(object aValue)
		{
			predValue = aValue;
		}

		public override string ToString()
		{
			return (predValue != null) ? ("= " + predValue.ToString()) : "null";
		}

	}

	//--------------------------------------------------------------------------------------
	//	NotEqual
	//--------------------------------------------------------------------------------------

	/// <summary>
	/// Predicate that returns true if the value is not equal to the value stored in the 
	/// predicate.
	/// </summary>
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
