using System;
using System.Runtime.Serialization ;


namespace Neo.Core
{
	[Serializable]
	public class NeoException : ApplicationException
	{
		internal NeoException(string reason) : base("[NEO] " + reason)
		{
		}

		internal NeoException(string reason, Exception nestedException) : base("[NEO] " + reason, nestedException)
		{
		}

		protected NeoException(SerializationInfo si, StreamingContext sc) : base(si, sc)
		{
		}

	}


	[Serializable]
	public class DataStoreSaveException : NeoException
	{
		internal DataStoreSaveException(string reason) : base(reason)
		{
		}

		internal DataStoreSaveException(Exception e) : base("Errors while saving: " + e.Message, e)
		{
		}

		protected DataStoreSaveException(SerializationInfo si, StreamingContext sc) : base(si, sc)
		{
		}

	}


	[Serializable]
	public class NotUniqueException : NeoException
	{
		internal NotUniqueException(bool foundMultiple, string qualifierFormat, params object[] parameters) :
			base(BuildMessage(foundMultiple, qualifierFormat, parameters))
		{
		}

		protected NotUniqueException(SerializationInfo si, StreamingContext sc) : base(si, sc)
		{
		}

		private static string BuildMessage(bool foundMultiple, string qualifierFormat, params object[] parameters)
		{
			string parameterString = "";
			for (int i = 0; i < parameters.Length; i++)
			{
				string parameterValue = parameters[i] == null ? "<NULL>" : parameters[i].ToString();
				parameterString += "'" + parameterValue + "'";
				if (i < parameters.Length - 1)
					parameterString += ", ";
			}

			string message = "The query '{0}' for parameters {1} {2}.";
			string reason;

			if (foundMultiple)
				reason = "was not unique";
			else
				reason = "returned zero matches"; 

			return String.Format(message, qualifierFormat, parameterString, reason);
		}
	}


	[Serializable]
	public class InvalidDbNullException : NeoException
	{
		internal InvalidDbNullException(string message) : base(message)
		{
		}

		protected InvalidDbNullException(SerializationInfo si, StreamingContext sc) : base(si, sc)
		{
		}

	}


	[Serializable]
	public class ObjectNotFoundException : NeoException
	{
		internal ObjectNotFoundException(string message) : base(message)
		{
		}

		protected ObjectNotFoundException(SerializationInfo si, StreamingContext sc) : base(si, sc)
		{
		}

	}


}
