using System;
using System.Globalization;
using System.Text.RegularExpressions;


namespace Neo.Core.Qualifiers
{

	//--------------------------------------------------------------------------------------
	//	Base class for string predicates (abstract)
	//--------------------------------------------------------------------------------------

	public abstract class StringPredicateBase
	{
		protected string predValue;

		public StringPredicateBase(string aValue)
		{
			predValue = aValue;
		}

		public object Value
		{
			get { return predValue; }
		}

	}

	//--------------------------------------------------------------------------------------
	//	Like
	//--------------------------------------------------------------------------------------

	public class LikePredicate : StringPredicateBase, IPredicate
	{
		protected string pattern;

		public LikePredicate(string aValue) : base(aValue)
		{
			if(aValue == null)
				throw new ArgumentException("The like predicate requires a non-null value.");

			aValue = Regex.Escape(aValue);
			pattern = "^" + aValue.Replace("%", ".*") + "$";
		}

		public bool IsTrueForValue(object aValue, object nullVal)
		{
			string aValueAsString;

			if(aValue == nullVal)
				return false;
		
			if((aValueAsString = aValue as String) == null)
				throw new ArgumentException("The like predicate can only be used with strings.");

			return Regex.IsMatch(aValueAsString, pattern, RegexOptions.IgnoreCase);
		}

		public override string ToString()
		{
			return ("like " + predValue);
		}
	}

#if WANTS_STRING_PREDICATES
	//--------------------------------------------------------------------------------------
	//	StartsWith
	//--------------------------------------------------------------------------------------

	public class StartsWithPredicate : StringPredicateBase, IPredicate
	{
		public StartsWithPredicate(string aValue) : base(aValue)
		{
		}

		public bool IsTrueForValue(object aValue, object nullVal)
		{
			if(aValue == nullVal)
				return false;  // null starts with nothing, not even null...?

			CompareInfo compInfo = CultureInfo.CurrentCulture.CompareInfo;
			return compInfo.IsPrefix((string)aValue, predValue, CompareOptions.IgnoreCase);
		}

		public override string ToString()
		{
			return (predValue != null) ? ("startswith " + predValue) : "null";
		}
	}


	//--------------------------------------------------------------------------------------
	//	EndsWith
	//--------------------------------------------------------------------------------------

	public class EndsWithPredicate : StringPredicateBase, IPredicate
	{
		public EndsWithPredicate(string aValue) : base(aValue)
		{
		}

		public bool IsTrueForValue(object aValue, object nullVal)
		{
			if(aValue == nullVal)
				return false;  // null ends with nothing, not even null...?

			CompareInfo compInfo = CultureInfo.CurrentCulture.CompareInfo;
			return compInfo.IsSuffix((string)aValue, predValue, CompareOptions.IgnoreCase);
		}

		public override string ToString()
		{
			return (predValue != null) ? ("endswith " + predValue) : "null";
		}
	}


	//--------------------------------------------------------------------------------------
	//	Contains
	//--------------------------------------------------------------------------------------

	public class ContainsPredicate : StringPredicateBase, IPredicate
	{
		public ContainsPredicate(string aValue) : base(aValue)
		{
		}

		public bool IsTrueForValue(object aValue, object nullVal)
		{
			if(aValue == nullVal)
				return false;  // null contains nothing, not even null...?

			CompareInfo compInfo = CultureInfo.CurrentCulture.CompareInfo;
			return (compInfo.IndexOf((string)aValue, predValue, CompareOptions.IgnoreCase) != -1);
		}

		public override string ToString()
		{
			return (predValue != null) ? ("contains " + predValue) : "null";
		}
	}
#endif

}
