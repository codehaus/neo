using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Runtime.Serialization;
using Foo;
using Neo.Database;


namespace Neo.PostgresqlClient
{
	[Serializable]
	public class PostgresqlDataStore : DbDataStore
	{

		//--------------------------------------------------------------------------------------
		//	Fields and constructor
		//--------------------------------------------------------------------------------------

		public PostgresqlDataStore() : base(new PostgresqlImplFactory())
		{
		    NameValueCollection	config = (NameValueCollection)ConfigurationSettings.GetConfig("neo.postgresqlclient");
			if(config == null)
				throw new ConfigurationException("Did not find neo.postgresqlclient config section.");
			String connectionString = config["connectionstring"];
			if(connectionString == null)
				throw new ConfigurationException("Did not find connectionstring in neo.postgresqlclient config section.");
				
			FinishInitialization(connectionString);
		
			logger.Debug("Created new PostgresqlDataStore.");
		}

		public PostgresqlDataStore(string connectionString) : base(new PostgresqlImplFactory())
		{
			FinishInitialization(connectionString);

			logger.Debug("Created new PostgresqlDataStore.");
		}

		public PostgresqlDataStore(string connectionString, bool useDelimitedIdentifiers) : this(connectionString)
        {
            base.usesDelimitedIdentifiers = useDelimitedIdentifiers;
        }

		public PostgresqlDataStore(IDbConnectionFactory connectionFactory) : base(new PostgresqlImplFactory(), connectionFactory)
		{
			FinishInitialization(null);
			logger.Debug("Created new PostgresqlDataStore with an IDbConnectionFactory.");
		}
	
		protected PostgresqlDataStore(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			logger.Debug("Deserialized PostgresqlDataStore.");
		}


		//--------------------------------------------------------------------------------------
		//	Overrides
		//--------------------------------------------------------------------------------------

		
	
		protected override object GetFinalPk(DataRow row, IDbCommandBuilder builder)
		{
			object result;
			Type   pkType;
			
			result = ExecuteScalar(String.Format("SELECT currval('{0}_{1}_seq')",
				row.Table.TableName, row.Table.PrimaryKey[0].ColumnName), null);

			pkType = row.Table.PrimaryKey[0].DataType;
			if((result.GetType() != pkType) && (result is IConvertible))
				result = Convert.ChangeType(result, pkType);
		
			return result;
		}


	}


}
