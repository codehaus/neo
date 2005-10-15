using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Runtime.Serialization;
using Foo;
using Neo.Database;

namespace Neo.MySqlClient
{
    [Serializable]
    public class MySqlDataStore : DbDataStore
    {
        //--------------------------------------------------------------------------------------
        //	Fields and constructor
        //--------------------------------------------------------------------------------------

        public MySqlDataStore() : base(new MySqlImplFactory())
        {
            NameValueCollection config = (NameValueCollection) ConfigurationSettings.GetConfig("neo.mysql");
            if (config == null)
            {
                throw new ConfigurationException("Did not find neo.mysql config section.");
            }
            String connectionString = config["connectionstring"];
            if (connectionString == null)
            {
                throw new ConfigurationException("Did not find connectionstring in neo.oracleclient config section.");
            }

        	FinishInitialization(connectionString);

            logger.Debug("Created new MySqlDataStore.");
        }

        public MySqlDataStore(string connectionString) : base(new MySqlImplFactory())
        {
        	FinishInitialization(connectionString);

            logger.Debug("Created new MySqlDataStore.");
        }

		public MySqlDataStore(string connectionString, bool useDelimitedIdentifiers) : this(connectionString)
        {
            base.usesDelimitedIdentifiers = useDelimitedIdentifiers;
        }

		public MySqlDataStore(IDbConnectionFactory connectionFactory) : base(new MySqlImplFactory(), connectionFactory)
		{
			FinishInitialization(null);
			logger.Debug("Created new MySqlDataStore with an IDbConnectionFactory.");
		}

		protected MySqlDataStore(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            logger.Debug("Deserialized MySqlDataStore.");
        }

        //--------------------------------------------------------------------------------------
        //	Overrides
        //--------------------------------------------------------------------------------------		

        protected override object GetFinalPk(DataRow row, IDbCommandBuilder builder)
        {
            object result;
            Type   pkType;
			
            result = ExecuteScalar("SELECT LAST_INSERT_ID()", null);

            pkType = row.Table.PrimaryKey[0].DataType;
            if((result.GetType() != pkType) && (result is IConvertible))
                result = Convert.ChangeType(result, pkType);
		
            return result;

        }
    }

}