using System;
using System.Runtime.Serialization;


namespace Neo.Core.Parser
{
	[Serializable]
	public class QualifierParserException : NeoException
	{
		public QualifierParserException(string message, int position) :
			base(String.Format("position {0}: {1}", position, message))
		{
		}

	
		protected QualifierParserException(SerializationInfo si, StreamingContext sc) : base(si, sc)
		{
		}

	}
}
