using System;
using System.Globalization;
using Neo.Core.Qualifiers;


namespace Neo.Core.Parser
{
	/// <summary>
	/// Internal class used by the <c>QualifierParser</c>.
	/// </summary>
	public class Tokenizer
	{
		//--------------------------------------------------------------------------------------
		//	Fields and constructor
		//--------------------------------------------------------------------------------------

		private string		input;
		private int			position;

		public Tokenizer(string inputString)
		{
			input = inputString;
			position = 0;
		}


		//--------------------------------------------------------------------------------------
		//	Accessors
		//--------------------------------------------------------------------------------------

		public string Input
		{
			get { return input; }
		}

		public int Position
		{
			get { return position; }
		}

		
		//--------------------------------------------------------------------------------------
		//	Input reader
		//--------------------------------------------------------------------------------------

		protected bool IsAtEnd()
		{
			return (position == input.Length);
		}

		protected char GetCurrentChar()
		{
			if(position == input.Length)
				throw new QualifierParserException("Unexpected end of input.", this);
			return input[position];
		}

		protected bool MoveNextChar()
		{
			position += 1;
			return (position < input.Length);
		}


		//--------------------------------------------------------------------------------------
		//	Tokenizer
		//--------------------------------------------------------------------------------------

		public Token GetNextToken()
		{
			Token	token;
			char	c;

			if(IsAtEnd())
				return null;

			c = GetCurrentChar();
			while(Char.IsWhiteSpace(c) && MoveNextChar())
				c = GetCurrentChar();
			if(Char.IsWhiteSpace(c))
				return null;

			if(Char.IsLetter(c) || c == '_')
			{
				token = ReadStringToken(c);
			}
			else if(Char.IsDigit(c))
			{
				token = ReadNumToken(c);
			}
			else if((c == '=') || (c == '<') || (c == '>') || (c == '!'))
			{
				token = ReadOpToken(c);
			}
			else if(c == '\'')
			{
				token = ReadQuotedStringToken(c);
			}
			else if(c == '(')
			{
				token = new Token(TokenType.OpenBracket, null);
				MoveNextChar();
			}
			else if(c ==')')
			{
				token = new Token(TokenType.CloseBracket, null);
				MoveNextChar();
			}
			else if(c == '[')
			{
				token = new Token(TokenType.OpenSqBracket, null);
				MoveNextChar();
			}
			else if(c ==']')
			{
				token = new Token(TokenType.CloseSqBracket, null);
				MoveNextChar();
			}
			else if(c == '{')
			{
				token = ReadRefToken(c);
			}
			else if(c == '.')
			{
				token = ReadPathSepToken(c);
			}
			else
			{
				throw new QualifierParserException(String.Format("Invalid start character for token; found {0}", c), this);
			}
			return token;
		}

		protected Token ReadStringToken(char c)
		{
			int start = position;
			while((Char.IsLetterOrDigit(c) || (c == '_')) && MoveNextChar())
				c = GetCurrentChar();

			string val = input.Substring(start, position - start);
			string lowerVal = val.ToLower(CultureInfo.InvariantCulture);
			if(lowerVal == "and")
				return new Token(TokenType.Conjunctor, typeof(AndQualifier));
			else if(lowerVal == "or")
				return new Token(TokenType.Conjunctor, typeof(OrQualifier));
			else if(lowerVal == "like")
				return new Token(TokenType.Operator, typeof(LikePredicate));
			else if(lowerVal == "null")
				return new Token(TokenType.Constant, null);
	
			return new Token(TokenType.String, val);
		}

		protected Token ReadNumToken(char c)
		{
			int start = position;
			while(Char.IsDigit(c) && MoveNextChar())
				c = GetCurrentChar();
			string val = input.Substring(start, position - start);
			return new Token(TokenType.Constant, Convert.ToInt32(val));
		}

		protected Token ReadOpToken(char c)
		{
			Type pt;
			MoveNextChar();
			if(c == '=')
			{
				pt = typeof(EqualsPredicate);
			}
			else if(c == '<')
			{
				if((GetCurrentChar() == '=') && MoveNextChar())
					pt = typeof(LessOrEqualPredicate);
				else
					pt = typeof(LessThanPredicate);
			}
			else if(c == '>')
			{
				if((GetCurrentChar() == '=') && MoveNextChar())
					pt = typeof(GreaterOrEqualPredicate);
				else
					pt = typeof(GreaterThanPredicate);
			}
			else if((c == '!') && (GetCurrentChar() == '=') && MoveNextChar())
			{
				pt = typeof(NotEqualPredicate);
			}
			else
			{
				throw new QualifierParserException("Unknown operator; found: " + c, this);
			}
			return new Token(TokenType.Operator, pt);
		}

		protected Token ReadQuotedStringToken(char c)
		{
			int start = position;
			MoveNextChar();
			while(GetCurrentChar() != '\'')
				MoveNextChar();
			MoveNextChar();
			string val = input.Substring(start + 1, position - start - 2);
			return new Token(TokenType.Constant, val);
		}

		protected Token ReadRefToken(char c)
		{
			int start = position;
			MoveNextChar();
			while(Char.IsDigit(GetCurrentChar()))
				MoveNextChar();
			if(GetCurrentChar() != '}')
				throw new QualifierParserException("Invalid argument reference.", this);
			MoveNextChar();
			string val = input.Substring(start + 1, position - start - 2);
			return new Token(TokenType.ParamRef, Convert.ToInt32(val));
		}

		protected Token ReadPathSepToken(char c)
		{
			MoveNextChar();
			return new Token(TokenType.PathSep, null);
		}


	}
}
