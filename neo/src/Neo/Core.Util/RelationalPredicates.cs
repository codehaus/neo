using System;

namespace Neo.Core.Util
{
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


	//--------------------------------------------------------------------------------------
	//	GreaterOrEqual
	//--------------------------------------------------------------------------------------

	public class GreaterOrEqualPredicate : RelationalPredicateBase, IPredicate
	{
		public GreaterOrEqualPredicate(IComparable aValue) : base(aValue)
		{
		}

		public bool IsTrueForValue(object aValue, object nullVal)
		{
			if((aValue != nullVal) && (predValue != nullVal) && (predValue.GetType() != aValue.GetType()) && (predValue is IConvertible))
				predValue = (IComparable)Convert.ChangeType(predValue, aValue.GetType());

			if((predValue == nullVal) || (aValue == nullVal))
				return false;
			return predValue.CompareTo(aValue) <= 0;
		}

		public override string ToString()
		{
			return (predValue != null) ? (">= " + predValue.ToString()) : "null";
		}
	}


	//--------------------------------------------------------------------------------------
	//	LessOrEqual
	//--------------------------------------------------------------------------------------

	public class LessOrEqualPredicate : RelationalPredicateBase, IPredicate
	{
		public LessOrEqualPredicate(IComparable aValue) : base(aValue)
		{
		}

		public bool IsTrueForValue(object aValue, object nullVal)
		{
			if((aValue != nullVal) && (predValue != nullVal) && (predValue.GetType() != aValue.GetType()) && (predValue is IConvertible))
				predValue = (IComparable)Convert.ChangeType(predValue, aValue.GetType());

			if((predValue == nullVal) || (aValue == nullVal))
				return false;
			return predValue.CompareTo(aValue) >= 0;
		}

		public override string ToString()
		{
			return (predValue != null) ? ("<= " + predValue.ToString()) : "null";
		}
	}


}
