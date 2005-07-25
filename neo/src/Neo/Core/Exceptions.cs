using System;
using System.Runtime.Serialization;
using Neo.Core.Util;


namespace Neo.Core
{
	/// <summary>
	/// General Neo exception and base class for other Neo exceptions.
	/// </summary>
	/// <remarks>
	/// All exception that are thrown as a part of the normal operation of Neo are subclasses
	/// of this type. In case of internal errors, other exceptions might be thrown.
	/// </remarks>
	[Serializable]
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
	/// General DataStore exception, thrown when saves fail.
	/// </summary>
	[Serializable]
	public class DataStoreSaveException : NeoException
	{
		[Serializable]
		public class ErrorInfo
		{
			private ObjectId oid;
			private string message;
			[NonSerialized] private IEntityObject entityObject;

			public ErrorInfo(ObjectId objectId, string message)
			{
				this.oid = objectId;
				this.message = message;
				this.entityObject = null;
			}

			public ObjectId ObjectId
			{
				get { return oid; }
			}

			public IEntityObject EntityObject
			{
				get { return entityObject; }
			}

			public string Message
			{
				get { return message; }
			}

			// Unfortunately, C# 1.x does not support different access permissions for get/set
			internal void SetEntityObject(IEntityObject anObject)
			{
				entityObject = anObject;
			}
		}


		private ErrorInfo[] errors;

		internal DataStoreSaveException(string reason) : base("Errors while saving: " + reason)
		{
		}

		internal DataStoreSaveException(string reason, ErrorInfo[] errors) : this(reason)
		{
			this.errors = errors;
		}

		internal DataStoreSaveException(Exception e) : base("Errors while saving: " + e.Message, e)
		{
		}

		internal DataStoreSaveException(Exception e, ObjectId oid) : this(e)
		{
			errors = new ErrorInfo[] { new ErrorInfo(oid, e.Message) };
		}

		/// <summary>
		/// Initializes a new instance of the DataStoreSaveException class with serialized data.
		/// </summary>
		/// <param name="si">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
		/// <param name="sc">The StreamingContext that contains contextual information about the source or destination. </param>
		protected DataStoreSaveException(SerializationInfo si, StreamingContext sc) : base(si, sc)
		{
			errors = (ErrorInfo[])si.GetValue("errors", typeof(ErrorInfo[]));
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("errors", errors, typeof(ErrorInfo[]));
		}

		/// <summary>
		/// An array of <c>ErrorInfo</c> for rows that caused problems and the corresponding error message.
		/// </summary>
		public ErrorInfo[] Errors
		{
			get { return errors; }
		}

		/// <summary>
		/// The <c>ObjectId</c> for the row that cause the problem, if there was exactly one error.
		/// </summary>
		public ObjectId ObjectId
		{
			get 
			{
				if((errors == null) || (errors.Length != 1))
					return null;
				return errors[0].ObjectId; 
			}
		}

	}


	/// <summary>
	/// Thrown when a unique value is expected but none or more than one was found.
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
	/// Thrown when a database <c>null</c> is not valid for a given attribute or column.
	/// </summary>
	/// <remarks>
	/// You can implement HandleNullValue on your entity objects to deal with <c>null</c> 
	/// values arriving from the database, e.g. convert <c>null</c> into zero for an 
	/// <c>int</c>.
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
	/// Thrown when an object was not found when looking for it via a foreign key.
	/// </summary>
	/// <remarks>
	/// An exception is thrown because this condition indicates a data integrity problem.
	/// </remarks>
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


	/// <summary>
	/// Thrown when a query is invalid.
	/// </summary>
	[Serializable]
	public class InvalidQueryException : NeoException
	{
		internal InvalidQueryException(string message) : base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the InvalidQueryException class with serialized data.
		/// </summary>
		/// <param name="si">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
		/// <param name="sc">The StreamingContext that contains contextual information about the source or destination. </param>
		protected InvalidQueryException(SerializationInfo si, StreamingContext sc) : base(si, sc)
		{
		}

	}


}
