using System;

namespace Neo.Core.Parser
{
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
