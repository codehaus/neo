using System;


namespace Neo.Core.Qualifiers
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

		
		public bool IsTrueForValue(object aValue, object nullVal)
		{
			if((predValue == nullVal) || (aValue == nullVal))
				return false;

			if((predValue.GetType() != aValue.GetType()) && (predValue is IConvertible))
				predValue = (IComparable)Convert.ChangeType(predValue, aValue.GetType());

			return IsTrueForCompareResult(predValue.CompareTo(aValue));
		}

		protected abstract bool IsTrueForCompareResult(int result);

		
		public override string ToString()
		{
			return (predValue != null) ? (OperatorString + " " + predValue.ToString()) : "null";
		}
		
		protected abstract string OperatorString { get; }

	}


	//--------------------------------------------------------------------------------------
	//	LessThan
	//--------------------------------------------------------------------------------------

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
