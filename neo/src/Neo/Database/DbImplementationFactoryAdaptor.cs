using System;
using System.Data;
using System.Runtime.Serialization;
using Foo;


namespace Neo.Database
{
	[Serializable]
	public class DbImplementationFactoryAdaptor : IDbConnectionFactory, ISerializable
	{
		//--------------------------------------------------------------------------------------
		//	Fields and constructor
		//--------------------------------------------------------------------------------------

		private String connectionString;
		private IDbImplementationFactory factory;

		public DbImplementationFactoryAdaptor(IDbImplementationFactory factory, string connectionString)
		{
			this.connectionString = connectionString;
			this.factory = factory;
		}

		//--------------------------------------------------------------------------------------
		//	Serialisation support
		//--------------------------------------------------------------------------------------

		protected DbImplementationFactoryAdaptor(SerializationInfo info, StreamingContext context) : base()
		{
			connectionString = info.GetString("connectionString");
			Type fType = (Type)info.GetValue("implFactoryType", typeof(Type));
			factory = (IDbImplementationFactory)Activator.CreateInstance(fType);
		}

		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("connectionString", connectionString);
			info.AddValue("implFactoryType", factory.GetType());
		}

		//--------------------------------------------------------------------------------------
		//	IDbConnectionFactory impl
		//--------------------------------------------------------------------------------------

		public IDbConnection CreateConnection()
		{
			return factory.CreateConnection(connectionString);
		}

	}
}