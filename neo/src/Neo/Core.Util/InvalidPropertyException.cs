using System;
using System.Runtime.Serialization;


namespace Neo.Core.Util
{
	/// <summary>
	/// Thrown when a property that was accessed using reflection does not exist on a type.
	/// </summary>
	[System.Serializable]
	public class InvalidPropertyException : ApplicationException
	{
		internal InvalidPropertyException(string message, Exception nestedEx) : base(message ,nestedEx)
		{
		}

		/// <summary>
		/// Initializes a new instance of the InvalidPropertyException class with serialized data.
		/// </summary>
		/// <param name="si">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
		/// <param name="sc">The StreamingContext that contains contextual information about the source or destination. </param>
		protected InvalidPropertyException(SerializationInfo si, StreamingContext sc) : base(si, sc)
		{
		}

	}

}
