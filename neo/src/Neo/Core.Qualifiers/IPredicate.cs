namespace Neo.Core.Qualifiers
{
	/// <summary>
	/// Describes a predicate that can be applied to a value.
	/// </summary>
	public interface IPredicate
	{
		bool IsTrueForValue(object aValue, object nullVal);
		object Value { get; } // this shouldn't really be on the interface a not predicate wouldn't have a value
	}


	internal interface IMutablePredicate : IPredicate
	{
		void SetPredicateValue(object aValue);
	}

}
