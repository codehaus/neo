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
		/// <summary>
		/// The type of the token.
		/// </summary>
		public readonly TokenType	Type;
		/// <summary>
		/// The value of the token.
		/// </summary>
		public readonly object		Value;

		/// <summary>
		/// Initializes a new instance of the <c>Token</c> class.
		/// </summary>
		/// <param name="aType">Type of the token</param>
		/// <param name="aValue">Value of the token</param>
		public Token(TokenType aType, object aValue)
		{
			Type = aType;
			Value = aValue;
		}
	}

}
