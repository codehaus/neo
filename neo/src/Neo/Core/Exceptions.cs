using System;
using System.Runtime.Serialization;


namespace Neo.Core
{
	/// <summary>
	/// General Neo exception
	/// </summary>
	/// <remarks>
	/// All exception that are thrown as a part of the normal operation of Neo are subclasses
	/// of this type. In case of internal errors, other exceptions might be thrown.
	/// </remarks>
	[System.Serializable]
	public class NeoException : ApplicationException
	{
		internal NeoException(string reason) : base("[NEO] " + reason)
		{
		}

		internal NeoException(string reason, Exception nestedException) : base("[NEO] " + reason, nestedException)
		{
		}

		/// <summary>
		/// Initializes a new instance of the NeoException class with serialized data.
		/// </summary>
		/// <param name="si">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
		/// <param name="sc">The StreamingContext that contains contextual information about the source or destination. </param>
		protected NeoException(SerializationInfo si, StreamingContext sc) : base(si, sc)
		{
		}

	}


	/// <summary>
	/// General DataStore exception, thrown when saves fail
	/// </summary>
	[Serializable]
	public class DataStoreSaveException : NeoException
	{
		internal DataStoreSaveException(string reason) : base(reason)
		{
		}

		internal DataStoreSaveException(Exception e) : base("Errors while saving: " + e.Message, e)
		{
		}

		/// <summary>
		/// Initializes a new instance of the DataStoreSaveException class with serialized data.
		/// </summary>
		/// <param name="si">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
		/// <param name="sc">The StreamingContext that contains contextual information about the source or destination. </param>
		protected DataStoreSaveException(SerializationInfo si, StreamingContext sc) : base(si, sc)
		{
		}

	}


	/// <summary>
	/// Thrown when a unique value is expected but none or more than one is found
	/// </summary>
	[Serializable]
	public class NotUniqueException : NeoException
	{
		internal NotUniqueException(bool foundMultiple, string qualifierFormat, params object[] parameters) :
			base(BuildMessage(foundMultiple, qualifierFormat, parameters))
		{
		}

		/// <summary>
		/// Initializes a new instance of the NotUniqueException class with serialized data.
		/// </summary>
		/// <param name="si">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
		/// <param name="sc">The StreamingContext that contains contextual information about the source or destination. </param>
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


	/// <summary>
	/// Thrown when a database null is not valid for a given attribute or column
	/// </summary>
	/// <remarks>
	/// You can implement HandleNullValue on your entity objects to deal with null values 
	/// arriving from the database, e.g. convert null into zero for an int.
	/// </remarks>
	[Serializable]
	public class InvalidDbNullException : NeoException
	{
		internal InvalidDbNullException(string message) : base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the InvalidDbNullException class with serialized data.
		/// </summary>
		/// <param name="si">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
		/// <param name="sc">The StreamingContext that contains contextual information about the source or destination. </param>
		protected InvalidDbNullException(SerializationInfo si, StreamingContext sc) : base(si, sc)
		{
		}

	}


	/// <summary>
	/// Thrown when an object is not found
	/// </summary>
	[Serializable]
	public class ObjectNotFoundException : NeoException
	{
		internal ObjectNotFoundException(string message) : base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the ObjectNotFoundException class with serialized data.
		/// </summary>
		/// <param name="si">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
		/// <param name="sc">The StreamingContext that contains contextual information about the source or destination. </param>
		protected ObjectNotFoundException(SerializationInfo si, StreamingContext sc) : base(si, sc)
		{
		}

	}


}
