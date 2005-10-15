using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Runtime.Serialization;
using System.Text;
using FirebirdSql.Data.Firebird;
using Foo;
using Neo.Database;


namespace Neo.FirebirdClient
{
	[Serializable]
	public class FirebirdDataStore : DbDataStore
	{
		//--------------------------------------------------------------------------------------
		//	Fields and constructor
		//--------------------------------------------------------------------------------------
		
		private bool keepConnectionOpen = false;

		public FirebirdDataStore() : base(new FirebirdImplFactory())
		{
			NameValueCollection	config = (NameValueCollection)ConfigurationSettings.GetConfig("neo.firebird");
			if(config == null)
				throw new ConfigurationException("Did not find neo.firebird config section.");
			String connectionString = config["connectionstring"];
			if(connectionString == null)
				throw new ConfigurationException("Did not find connectionstring in neo.firebird config section.");
			FinishInitialization(connectionString);
			
			logger.Debug("Created new FirebirdDataStore.");
		}

		public FirebirdDataStore(string connectionString) : base(new FirebirdImplFactory())
		{
			FinishInitialization(connectionString);
			// by default, use delimited identifier for backwards compatibility
			base.usesDelimitedIdentifiers = true;
			
			logger.Debug("Created new FirebirdDataStore.");
		}

        public FirebirdDataStore(string connectionString, bool useDelimitedIdentifiers) : this(connectionString)
        {
            base.usesDelimitedIdentifiers = useDelimitedIdentifiers;
        }
		
		public FirebirdDataStore(IDbConnectionFactory connectionFactory) : base(new FirebirdImplFactory(), connectionFactory)
		{
			FinishInitialization(null);
			// by default, use delimited identifier for backwards compatibility
			base.usesDelimitedIdentifiers = true;
			
			logger.Debug("Created new FirebirdDataStore with an IDbConnectionFactory.");
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FirebirdDataStore"/> class when given 
		/// a FbConnection.
		/// </summary>
		/// <param name="connection">An initialized FbConnection that is ready to open or already open.</param>
		/// <param name="keepConnectionOpen">If set to true the connection won't be closed automatically by the datastore.</param>
		/// <remarks>By default the datastore closes the connection after every access 
		/// to the database. This can be avoided by setting <see cref="keepConnectionOpen"/>
		/// to true. The datatstore then never closes the connection.
		/// <para>On the other hand the datastore always automatically opens the connection
		/// (if it's closed) when it has to access the database.</para>
		/// <para>When using this constructor the resulting store is not serializable. Please use a 
		/// connection factory in this case.</para></remarks> 
		public FirebirdDataStore(FbConnection connection, bool keepConnectionOpen) : base(new FirebirdImplFactory())
		{
			this.keepConnectionOpen = keepConnectionOpen;
			this.connection = connection;
			// by default, use delimited identifier for backwards compatibility
			base.usesDelimitedIdentifiers = true;

			logger.Debug("Created new FirebirdDataStore.");
		}

		public FirebirdDataStore(FbConnection connection, bool keepConnectionOpen, bool useDelimitedIdentifiers) : this(connection, keepConnectionOpen)
		{
			base.usesDelimitedIdentifiers = useDelimitedIdentifiers;
		}

		protected FirebirdDataStore(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			logger.Debug("Deserialized FirebirdDataStore.");
		}

		//--------------------------------------------------------------------------------------
		//	Overrides
		//--------------------------------------------------------------------------------------
		
		protected override object GetFinalPk(DataRow row, IDbCommandBuilder builder)
		{
			object result;
			Type   pkType;

			// A generator called ID_GENERATOR must be in the database for automatic ID generation.
			// For tables that use auto ID a before-trigger must exist that assigns the generator value to the pk column.
			result = ExecuteScalar("SELECT GEN_ID(ID_GENERATOR, 0) AS LATEST_ID FROM RDB$DATABASE", null);

			pkType = row.Table.PrimaryKey[0].DataType;
			if((result.GetType() != pkType) && (result is IConvertible))
				result = Convert.ChangeType(result, pkType);
		
			return result;
		}

		//--------------------------------------------------------------------------------------
		//	Public methods unique to FirebirdStore
		//--------------------------------------------------------------------------------------

		public virtual void ClearTable(string tableName)
		{
			StringBuilder	builder;

			builder = new StringBuilder();
			builder.Append("DELETE FROM ");
			builder.Append(tableName);
			
			ExecuteNonQuery(builder.ToString(), null);
		}

		protected override void Close()
		{
			if (! keepConnectionOpen)
				base.Close ();
		}

	}


}
