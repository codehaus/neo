using System;
using System.Collections;
using System.Globalization;
using System.Runtime.Serialization;

using Neo.Core;


namespace Neo.Core.Util
{
	public class QualifierParser
	{
		//--------------------------------------------------------------------------------------
		//	Inner types
		//--------------------------------------------------------------------------------------

		public enum TokenType
		{
			String,
			Operator,
			Conjunctor,
			Number,
			QuotedString,
			ParamRef,
			Qualifier,
			PathSep,
			Path
		}


		// public for tests only
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


		//--------------------------------------------------------------------------------------
		//	Fields and constructor
		//--------------------------------------------------------------------------------------

		private object[]	parameters;
		private string		input;
		private int			position;
		private Token[]		stack;
		private	int			sp;
		
		
		public QualifierParser(string qformat, params object[] someParameters)
		{
			parameters = (someParameters != null) ? someParameters : (new object[] { null });
			input = qformat;
		}


		//--------------------------------------------------------------------------------------
		//	Parser Loop
		//--------------------------------------------------------------------------------------

		public Qualifier GetQualifier()
		{
			Token		token;
			Qualifier	qualifier;

			position = -1;
			MoveNextChar();
			stack = new Token[100];
			sp = -1;

			while((token = GetNextToken()) != null)
			{
				Shift(token);
				while((token = Reduce()) != null)
					Shift(token);
			}
			if(sp != 0)
				throw new QualifierParserException("Syntax error.", input, position);
			if(stack[0].Value is Qualifier)
			{
				qualifier = (Qualifier)stack[0].Value;
			}
			else
			{
				if(stack[0].Type != TokenType.String)
					throw new QualifierParserException("Syntax error.", input, position);
				qualifier = new PropertyQualifier((string)stack[0].Value, QualifierOperator.Equal, parameters[0]);
			}

			return qualifier;
		}
		
		protected void Shift(Token t)
		{
			stack[++sp] = t;
		}

		protected Token Reduce()
		{
			Token	token = null;

			// Rule: Qualifier, Conjunctor, Qualifier -> Qualifier
			// If the left qualifier is a clause qualifier and its conjunctor matchtes the
			// conjuctor on the stack, the right qualifier is added to the left.
			if(LhsMatch(TokenType.Qualifier, TokenType.Conjunctor, TokenType.Qualifier))
			{
				Qualifier right = (Qualifier)stack[sp--].Value;
				QualifierConjunctor conj = (QualifierConjunctor)stack[sp--].Value; 
				Qualifier left = (Qualifier)stack[sp--].Value;
				ClauseQualifier q;

				if(((q = left as ClauseQualifier) != null) && (q.Conjunctor == conj))
					q.AddToQualifiers(right);
				else
					q = new ClauseQualifier(conj, left, right);
				token = new Token(TokenType.Qualifier, q);
			}
			// Rule: String, Operator, Literal -> Qualfier
			else if(LhsMatch(TokenType.String, TokenType.Operator, TokenType.QuotedString))
			{
				string val = (string)stack[sp--].Value;
				QualifierOperator op = (QualifierOperator)stack[sp--].Value;
				string attr = (string)stack[sp--].Value;

				PropertyQualifier q = new PropertyQualifier(attr, op, val);
				token = new Token(TokenType.Qualifier, q);
			}
			// Rule: String, Operator, Number -> Qualfier
			else if(LhsMatch(TokenType.String, TokenType.Operator, TokenType.Number))
			{
				int val = (int)stack[sp--].Value;
				QualifierOperator op = (QualifierOperator)stack[sp--].Value;
				string prop = (string)stack[sp--].Value;

				PropertyQualifier q = new PropertyQualifier(prop, op, val);
				token = new Token(TokenType.Qualifier, q);
			}
			// Rule: String, Operator, ParamRef -> Qualfier
			else if(LhsMatch(TokenType.String, TokenType.Operator, TokenType.ParamRef))
			{
				int idx = (int)stack[sp--].Value;
				QualifierOperator op = (QualifierOperator)stack[sp--].Value;
				string attr = (string)stack[sp--].Value;

				PropertyQualifier q = new PropertyQualifier(attr, op, parameters[idx]);
				token = new Token(TokenType.Qualifier, q);
			}
			// Rule: String, Operator, String -> Qualfier
			// This catches 'true' and 'false' and throws an exception otherwise
			else if(LhsMatch(TokenType.String, TokenType.Operator, TokenType.String))
			{
				string str = ((string)stack[sp--].Value).ToLower(CultureInfo.InvariantCulture);
				QualifierOperator op = (QualifierOperator)stack[sp--].Value;
				string attr = (string)stack[sp--].Value;

				if((str != "true") && (str != "false"))
					throw new QualifierParserException("Invalid right hand side for comparison; found " + str, input, position);

				PropertyQualifier q = new PropertyQualifier(attr, op, (str == "true"));
				token = new Token(TokenType.Qualifier, q);
			}
			// Rule: Path, Qualifier -> PathQualifier
			else if(LhsMatch(TokenType.Path, TokenType.Qualifier))
			{
				Qualifier lq = (Qualifier)stack[sp--].Value;
				ArrayList path = (ArrayList)stack[sp--].Value;

				PathQualifier q = new PathQualifier((string[])path.ToArray(typeof(string)), lq);
				token = new Token(TokenType.Qualifier, q);
			}
			// Rule: Path, String, PathSep -> Path
			else if(LhsMatch(TokenType.Path, TokenType.String, TokenType.PathSep))
			{
				sp -= 1;
				string element = (string)stack[sp--].Value;
				ArrayList path = (ArrayList)stack[sp--].Value;

				path.Add(element);
				token = new Token(TokenType.Path, path);
			}
			// Rule: String, PathSep -> Path
			else if(LhsMatch(TokenType.String, TokenType.PathSep))
			{
				sp -= 1;
				string element = (string)stack[sp--].Value;
				
				ArrayList path = new ArrayList();
				path.Add(element);
				token = new Token(TokenType.Path, path);
			}

			
			return token;
		}

		protected bool LhsMatch(params TokenType[] types)
		{
			int	tp = types.Length - 1;

			if(tp > sp)
				return false;

			for(int i = 0;i <= tp; i++)
			{
				if(stack[sp - i].Type != types[tp - i])
					return false;
			}
			return true;
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

			if(Char.IsLetter(c))
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
				throw new QualifierParserException(String.Format("Invalid start character for token; found {0}", c),
					input, position);
			}
			return token;
		}

		protected Token ReadStringToken(char c)
		{
			int start = position;
			while(Char.IsLetterOrDigit(c) && MoveNextChar())
				c = GetCurrentChar();

			string val = input.Substring(start, position - start);
			string lowerVal = val.ToLower(CultureInfo.InvariantCulture);
			if(lowerVal == "and")
				return new Token(TokenType.Conjunctor, QualifierConjunctor.And);
			else if(lowerVal == "or")
				return new Token(TokenType.Conjunctor, QualifierConjunctor.Or);
	
			return new Token(TokenType.String, val);
		}

		protected Token ReadNumToken(char c)
		{
			int start = position;
			while(Char.IsDigit(c) && MoveNextChar())
				c = GetCurrentChar();
			string val = input.Substring(start, position - start);
			return new Token(TokenType.Number, Convert.ToInt32(val));
		}

		protected Token ReadOpToken(char c)
		{
			QualifierOperator op;
			MoveNextChar();
			if(c == '=')
				op = QualifierOperator.Equal;
			else if(c == '<')
				op = QualifierOperator.LessThan;
			else if(c == '>')
				op = QualifierOperator.GreaterThan;
			else
				throw new QualifierParserException("Unknown operator.", input, position);
			return new Token(TokenType.Operator, op);
		}

		protected Token ReadQuotedStringToken(char c)
		{
			int start = position;
			MoveNextChar();
			while(GetCurrentChar() != '\'')
				MoveNextChar();
			MoveNextChar();
			string val = input.Substring(start + 1, position - start - 2);
			return new Token(TokenType.QuotedString, val);
		}

		protected Token ReadRefToken(char c)
		{
			int start = position;
			MoveNextChar();
			while(Char.IsDigit(GetCurrentChar()))
				MoveNextChar();
			if(GetCurrentChar() != '}')
				throw new QualifierParserException("Invalid argument reference.", input, position);
			MoveNextChar();
			string val = input.Substring(start + 1, position - start - 2);
			return new Token(TokenType.ParamRef, Convert.ToInt32(val));
		}

		protected Token ReadPathSepToken(char c)
		{
			MoveNextChar();
			return new Token(TokenType.PathSep, null);
		}


		//--------------------------------------------------------------------------------------
		//	input reader (shares state with tokenizer)
		//--------------------------------------------------------------------------------------

		protected bool IsAtEnd()
		{
			return (position == input.Length);
		}

		protected char GetCurrentChar()
		{
			if(position == input.Length)
				throw new QualifierParserException("Unexpected end of input.", input, position);
			return input[position];
		}

		protected bool MoveNextChar()
		{
			position += 1;
			return (position < input.Length);
		}

	}


	//--------------------------------------------------------------------------------------
	//	Exceptions
	//--------------------------------------------------------------------------------------

	[Serializable]
	public class QualifierParserException : NeoException
	{
		public QualifierParserException(string message, string input, int position) :
			base(String.Format("position {0}: {1}", position, message))
		{
		}

	
		protected QualifierParserException(SerializationInfo si, StreamingContext sc) : base(si, sc)
		{
		}


	}

}
