using System;

namespace Neo.Core.Util
{
	public interface IPredicate
	{
		bool IsTrueForValue(object aValue, object nullVal);
		object Value { get; } // this shouldn't really be on the interface, a not predicate wouldn't have a value
	}


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



	//--------------------------------------------------------------------------------------
	//	Base class for relational predicates (abstract)
	//--------------------------------------------------------------------------------------

	public abstract class RelationalPredicateBase
	{
		protected IComparable predValue;

		public RelationalPredicateBase(IComparable aValue)
		{
			predValue = aValue;
		}

		public object Value
		{
			get { return predValue; }
		}

	}


	//--------------------------------------------------------------------------------------
	//	LessThan
	//--------------------------------------------------------------------------------------

	public class LessThanPredicate : RelationalPredicateBase, IPredicate
	{
		public LessThanPredicate(IComparable aValue) : base(aValue)
		{
		}

		public bool IsTrueForValue(object aValue, object nullVal)
		{
			if((aValue != nullVal) && (predValue != nullVal) && (predValue.GetType() != aValue.GetType()) && (predValue is IConvertible))
				predValue = (IComparable)Convert.ChangeType(predValue, aValue.GetType());

			if((predValue == nullVal) || (aValue == nullVal))
				return false;
			return predValue.CompareTo(aValue) > 0;
		}

		public override string ToString()
		{
			return (predValue != null) ? ("< " + predValue.ToString()) : "null";
		}
	}


	//--------------------------------------------------------------------------------------
	//	GreaterThan
	//--------------------------------------------------------------------------------------

	public class GreaterThanPredicate : RelationalPredicateBase, IPredicate
	{
		public GreaterThanPredicate(IComparable aValue) : base(aValue)
		{
		}

		public bool IsTrueForValue(object aValue, object nullVal)
		{
			if((aValue != nullVal) && (predValue != nullVal) && (predValue.GetType() != aValue.GetType()) && (predValue is IConvertible))
				predValue = (IComparable)Convert.ChangeType(predValue, aValue.GetType());

			if((predValue == nullVal) || (aValue == nullVal))
				return false;
			return predValue.CompareTo(aValue) < 0;
		}

		public override string ToString()
		{
			return (predValue != null) ? ("> " + predValue.ToString()) : "null";
		}
	}


}
