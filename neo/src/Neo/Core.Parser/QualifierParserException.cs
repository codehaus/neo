using System;
using System.Runtime.Serialization;


namespace Neo.Core.Parser
{
	[System.Serializable]
	public class QualifierParserException : NeoException
	{
		public QualifierParserException(string message, Tokenizer tokenizer) :
			base(String.Format("{0} position {1} in \"{2}\"", message, tokenizer.Position, tokenizer.Input))
		{
		}

	
		protected QualifierParserException(SerializationInfo si, StreamingContext sc) : base(si, sc)
		{
		}

	}
}
