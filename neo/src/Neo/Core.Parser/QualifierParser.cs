using System;
using System.Collections;
using System.Globalization;
using Neo.Core.Qualifiers;


namespace Neo.Core.Parser
{
	/// <summary>
	/// Internal class used to create a tree of qualifier objects from a format string.
	/// </summary>
	public class QualifierParser
	{
		//--------------------------------------------------------------------------------------
		//	Fields and constructor
		//--------------------------------------------------------------------------------------

		private Tokenizer	tokenizer;
		private TokenStack	stack;
		private object[]	parameters;
		
		/// <summary>
		/// Constructor. Creates a new parser.
		/// </summary>
		/// <param name="qformat">The format to parse</param>
		/// <param name="someParameters">The position paramters for the format</param>
		public QualifierParser(string qformat, params object[] someParameters)
		{
			parameters = (someParameters != null) ? someParameters : (new object[] { null });
			tokenizer = new Tokenizer(qformat);
		}


		//--------------------------------------------------------------------------------------
		//	Parser Loop
		//--------------------------------------------------------------------------------------

		/// <summary>
		/// Parses the format and creates the qualifier objects.
		/// </summary>
		/// <returns>The top-level qualifier</returns>
		public Qualifier GetQualifier()
		{
			Token		token;
			Qualifier	qualifier;

			stack = new TokenStack();
			while((token = tokenizer.GetNextToken()) != null)
			{
				stack.Push(token);
				while((token = Reduce()) != null)
					stack.Push(token);
			}

			if(stack.Count != 1)
				throw new QualifierParserException("Syntax error.", tokenizer);
			
			if(stack.Peek().Type == TokenType.String)
			{
				qualifier = new PropertyQualifier((string)stack.PopValue(), new EqualsPredicate(parameters[0]));
			}
			else
			{
				qualifier = stack.PopValue() as Qualifier;
				if(qualifier == null)
					throw new QualifierParserException("Syntax error.", tokenizer);
			}

			return qualifier;
		}


		protected Token Reduce()
		{
			Token	token = null;

			// Rule: OpenBracket, Qualifier, CloseBracket -> Qualifier
			if(stack.Match(TokenType.OpenBracket, TokenType.Qualifier, TokenType.CloseBracket))
			{
				stack.Pop();
				token = stack.Pop();
				stack.Pop();
			}
			// Rule: Qualifier, Conjunctor, Qualifier -> Qualifier
			// If the left qualifier is of the same type as the conjuctor on the stack, 
			// the right qualifier is added to the left; and vice versa.
			else if(stack.Match(TokenType.Qualifier, TokenType.Conjunctor, TokenType.Qualifier))
			{
				Qualifier right = (Qualifier)stack.PopValue();
			    Type qt  = (Type)stack.PopValue(); 
				Qualifier left = (Qualifier)stack.PopValue();
				ClauseQualifier q;

				if(left.GetType() == qt)
				{
					q = left as ClauseQualifier;
					q.AddToQualifiers(right);
				}
				else if(right.GetType() == qt)
				{
					q = right as ClauseQualifier;
					q.AddToQualifiers(left);
				}
				else
				{
					q = (ClauseQualifier)Activator.CreateInstance(qt, new Qualifier[] { left, right });
				}
				token = new Token(TokenType.Qualifier, q);
			}
			// Rule: String, Predicate -> Qualifier
			else if(stack.Match(TokenType.String, TokenType.Predicate))
			{
				IPredicate pred = (IPredicate)stack.PopValue();
				string attr = (string)stack.PopValue();

				PropertyQualifier q = new PropertyQualifier(attr, pred);
				token = new Token(TokenType.Qualifier, q);
			}
			// Rule: Operator, Constant -> Predicate
			else if(stack.Match(TokenType.Operator, TokenType.Constant))
			{
				object val = stack.PopValue();
				Type pt = (Type)stack.PopValue();

				IPredicate pred = (IPredicate)Activator.CreateInstance(pt, new object[] { val } );
				token = new Token(TokenType.Predicate, pred);
			}
			// Rule: Operator, ParamRef -> Predicate
			else if(stack.Match(TokenType.Operator, TokenType.ParamRef))
			{
				int idx = (int)stack.PopValue();
				Type pt = (Type)stack.PopValue();

				IPredicate pred = (IPredicate)Activator.CreateInstance(pt, new object[] { parameters[idx] } );
				token = new Token(TokenType.Predicate, pred);
			}
			// Rule: Operator, String -> Predicate
			// This catches 'true' and 'false' and throws an exception otherwise
			else if(stack.Match(TokenType.Operator, TokenType.String))
			{
				string str = ((string)stack.PopValue()).ToLower(CultureInfo.InvariantCulture);
				Type pt = (Type)stack.PopValue();

				if((str != "true") && (str != "false"))
					throw new QualifierParserException("Invalid right hand side for comparison; found " + str, tokenizer);

				IPredicate pred = (IPredicate)Activator.CreateInstance(pt, new object[] { (str == "true") } );
				token = new Token(TokenType.Predicate, pred);
			}
			// Rule: Path, Qualifier -> PathQualifier
			else if(stack.Match(TokenType.Path, TokenType.Qualifier))
			{
				Qualifier lq = (Qualifier)stack.PopValue();
			    ArrayList path = (ArrayList)stack.PopValue();

				PathQualifier q = new PathQualifier((string[])path.ToArray(typeof(string)), lq);
				token = new Token(TokenType.Qualifier, q);
			}
			// Rule: Path, String, PathSep -> Path
			else if(stack.Match(TokenType.Path, TokenType.String, TokenType.PathSep))
			{
				stack.Pop();
				string element = (string)stack.PopValue();
				ArrayList path = (ArrayList)stack.PopValue();

				path.Add(element);
				token = new Token(TokenType.Path, path);
			}
			// Rule: String, PathSep -> Path
			else if(stack.Match(TokenType.String, TokenType.PathSep))
			{
				stack.Pop();
				string element = (string)stack.PopValue();
				
				ArrayList path = new ArrayList();
				path.Add(element);
				token = new Token(TokenType.Path, path);
			}

			return token;
		}

	}


}
