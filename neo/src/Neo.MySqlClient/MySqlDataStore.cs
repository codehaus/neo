using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Runtime.Serialization;

using Neo.Database;

namespace Neo.MySqlClient
{
    [Serializable]
    public class MySqlDataStore : DbDataStore
    {
        //--------------------------------------------------------------------------------------
        //	Fields and constructor
        //--------------------------------------------------------------------------------------

        public MySqlDataStore() : base()
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

            implFactory = new MySqlImplFactory();
            connection = implFactory.CreateConnection(connectionString);

            logger.Debug("Created new MySqlDataStore.");
        }

        public MySqlDataStore(string connectionString) : base()
        {
            implFactory = new MySqlImplFactory();
            connection = implFactory.CreateConnection(connectionString);

            logger.Debug("Created new MySqlDataStore.");
        }

        public MySqlDataStore(string connectionString, bool useDelimitedIdentifiers) : this(connectionString)
        {
            base.usesDelimitedIdentifiers = useDelimitedIdentifiers;
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