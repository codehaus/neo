namespace Neo.Core.Parser
{
	/// <summary>
	/// Internal class used by <c>Tokenizer</c> and <c>QualifierParser</c>.
	/// </summary>
	public class TokenStack
	{
		//--------------------------------------------------------------------------------------
		//	Fields and constructor
		//--------------------------------------------------------------------------------------

		private Token[]	values;
		private	int		sp;

		public TokenStack()
		{
			values = new Token[100];
			sp = -1;
		}


		//--------------------------------------------------------------------------------------
		//	Accessors
		//--------------------------------------------------------------------------------------

		public int Count
		{
			get { return sp + 1; }
		}

		public void Push(Token t)
		{
			values[++sp] = t;
		}

		public Token Pop()
		{
			return values[sp--];
		}

		public object PopValue()
		{
			return values[sp--].Value;
		}

		public Token Peek()
		{
			return values[sp];
		}

		public bool Match(params TokenType[] types)
		{
			int	tp = types.Length - 1;

			if(tp > sp)
				return false;

			for(int i = 0;i <= tp; i++)
			{
				if(values[sp - i].Type != types[tp - i])
					return false;
			}
			return true;
		}


	}
}
