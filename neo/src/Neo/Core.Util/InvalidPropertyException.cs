using System;
using System.Runtime.Serialization;


namespace Neo.Core.Util
{
	[System.Serializable]
	public class InvalidPropertyException : ApplicationException
	{
		public InvalidPropertyException(string message, Exception nestedEx) : base(message ,nestedEx)
		{
		}

		protected InvalidPropertyException(SerializationInfo si, StreamingContext sc) : base(si, sc)
		{
		}

	}

}
