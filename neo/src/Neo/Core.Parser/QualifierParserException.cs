using System;
using System.Runtime.Serialization;


namespace Neo.Core.Parser
{
	/// <summary>
	/// Exception thrown by the <c>QualifierParser</c> if the format contains errors.
	/// </summary>
	[System.Serializable]
	public class QualifierParserException : NeoException
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Reason for the error</param>
		/// <param name="tokenizer"><c>Tokenizer</c> positioned at the error</param>
		public QualifierParserException(string message, Tokenizer tokenizer) :
			base(String.Format("{0} (position {1} in \"{2}\")", message, tokenizer.Position, tokenizer.Input))
		{
		}

	
		/// <summary>
		/// Initializes a new instance of the QualifierParserException class with serialized data.
		/// </summary>
		/// <param name="si">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
		/// <param name="sc">The StreamingContext that contains contextual information about the source or destination. </param>
		protected QualifierParserException(SerializationInfo si, StreamingContext sc) : base(si, sc)
		{
		}

	}
}
