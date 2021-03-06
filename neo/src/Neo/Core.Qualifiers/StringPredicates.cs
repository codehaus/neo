using System;
using System.Globalization;
using System.Text.RegularExpressions;


namespace Neo.Core.Qualifiers
{

	//--------------------------------------------------------------------------------------
	//	Base class for string predicates (abstract)
	//--------------------------------------------------------------------------------------

	/// <summary>
	/// Abstract base class for predicates that work with string values.
	/// </summary>
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

		public void SetPredicateValue(object aValue)
		{
			predValue = (string)aValue;
		}

	}

	//--------------------------------------------------------------------------------------
	//	Like
	//--------------------------------------------------------------------------------------

	/// <summary>
	/// Predicate that returns true if the string matches the pattern.
	/// </summary>
	public class LikePredicate : StringPredicateBase, IMutablePredicate
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

			return Regex.IsMatch(aValueAsString, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
		}

		public override string ToString()
		{
			return ("like " + predValue);
		}
	}

	
	/// <summary>
	/// Predicate that returns true if the string is equal to the string stored in the
	/// predicate not taking case into account.
	/// </summary>
	/// <remarks>
	/// This predicate does not necessarily translate into SQL.
	/// </remarks>
	public class CaseInsensitiveEqualsPredicate : StringPredicateBase, IMutablePredicate
	{
		public CaseInsensitiveEqualsPredicate(string aValue) : base(aValue)
		{
		}

		public bool IsTrueForValue(object aValue, object nullVal)
		{
			if(aValue == nullVal)
				return false;

			CompareInfo compInfo = CultureInfo.CurrentCulture.CompareInfo;
			
			return(compInfo.Compare((string)aValue, predValue, CompareOptions.IgnoreCase) == 0);
		}

		public override string ToString()
		{
			return (predValue != null) ? ("equals (case insensitive) " + predValue) : "null";
		}

	}

	//--------------------------------------------------------------------------------------
	//	StartsWith
	//--------------------------------------------------------------------------------------

	/// <summary>
	/// Predicate that returns true if the string begins with the string stored in the
	/// predicate.
	/// </summary>
	public class StartsWithPredicate : StringPredicateBase, IMutablePredicate
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

	/// <summary>
	/// Predicate that returns true if the string ends with the string stored in the
	/// predicate.
	/// </summary>
	public class EndsWithPredicate : StringPredicateBase, IMutablePredicate
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

	/// <summary>
	/// Predicate that returns true if the string contains the string stored in the
	/// predicate.
	/// </summary>
	public class ContainsPredicate : StringPredicateBase, IMutablePredicate
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


}
