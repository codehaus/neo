namespace Neo.Core.Parser
{
	/// <summary>
	/// Specifies the type of a <c>Token</c>.
	/// </summary>
	public enum TokenType
	{
		String,
		Operator,
		Conjunctor,
		Constant,
		ParamRef,
		Predicate,
		Qualifier,
		PathSep,
		Path,
		OpenBracket,
		CloseBracket
	}


	/// <summary>
	/// Internal class used by the <c>QualifierParser</c>.
	/// </summary>
	public class Token
	{
		public readonly TokenType	Type;
		public readonly object		Value;

		public Token(TokenType aType, object aValue)
		{
			Type = aType;
			Value = aValue;
		}
	}

}
