using System;


namespace Neo.Core.Qualifiers
{
	//--------------------------------------------------------------------------------------
	//	Base class for relational predicates (abstract)
	//--------------------------------------------------------------------------------------

	/// <summary>
	/// Abstract base class for predicates that compare values, i.e. determine the ordering
	/// of two values.
	/// </summary>
	public abstract class RelationalPredicateBase : IMutablePredicate
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

		
		public bool IsTrueForValue(object aValue, object nullVal)
		{
			if((predValue == nullVal) || (aValue == nullVal))
				return false;

			if((predValue.GetType() != aValue.GetType()) && (predValue is IConvertible))
				predValue = (IComparable)Convert.ChangeType(predValue, aValue.GetType());

			return IsTrueForCompareResult(predValue.CompareTo(aValue));
		}

		protected abstract bool IsTrueForCompareResult(int result);

		public void SetPredicateValue(object aValue)
		{
			predValue = (IComparable)aValue;
		}
		
		public override string ToString()
		{
			return (predValue != null) ? (OperatorString + " " + predValue.ToString()) : "null";
		}
		
		protected abstract string OperatorString { get; }

	}


	//--------------------------------------------------------------------------------------
	//	LessThan
	//--------------------------------------------------------------------------------------

	/// <summary>
	/// Predicate that returns true if the value is less than the value stored in the 
	/// predicate.
	/// </summary>
	public class LessThanPredicate : RelationalPredicateBase, IPredicate
	{
		public LessThanPredicate(IComparable aValue) : base(aValue)
		{
		}
		
		protected override bool IsTrueForCompareResult(int result)
		{
			return result > 0;
		}

		protected override string OperatorString
		{
			get { return "<"; }
		}

	}


	//--------------------------------------------------------------------------------------
	//	GreaterThan
	//--------------------------------------------------------------------------------------

	/// <summary>
	/// Predicate that returns true if the value is greater than the value stored in the 
	/// predicate.
	/// </summary>
	public class GreaterThanPredicate : RelationalPredicateBase, IPredicate
	{
		public GreaterThanPredicate(IComparable aValue) : base(aValue)
		{
		}

		protected override bool IsTrueForCompareResult(int result)
		{
			return result < 0;
		}

		protected override string OperatorString
		{
			get { return ">"; }
		}

	}


	//--------------------------------------------------------------------------------------
	//	GreaterOrEqual
	//--------------------------------------------------------------------------------------

	/// <summary>
	/// Predicate that returns true if the value is greater or equal to the value stored in 
	/// the predicate.
	/// </summary>
	public class GreaterOrEqualPredicate : RelationalPredicateBase, IPredicate
	{
		public GreaterOrEqualPredicate(IComparable aValue) : base(aValue)
		{
		}

		protected override bool IsTrueForCompareResult(int result)
		{
			return result <= 0;
		}


		protected override string OperatorString
		{
			get { return ">="; }
		}

	}


	//--------------------------------------------------------------------------------------
	//	LessOrEqual
	//--------------------------------------------------------------------------------------

	/// <summary>
	/// Predicate that returns true if the value is less or equal to the value stored in 
	/// the predicate.
	/// </summary>
	public class LessOrEqualPredicate : RelationalPredicateBase, IPredicate
	{
		public LessOrEqualPredicate(IComparable aValue) : base(aValue)
		{
		}

		protected override bool IsTrueForCompareResult(int result)
		{
			return result >= 0;
		}

		protected override string OperatorString
		{
			get { return "<="; }
		}

	}


}
