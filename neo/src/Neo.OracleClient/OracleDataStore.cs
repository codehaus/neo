using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Runtime.Serialization;
using Foo;
using Neo.Core;
using Neo.Database;


namespace Neo.OracleClient
{
	[Serializable]
	public class OracleDataStore : DbDataStore
	{

		//--------------------------------------------------------------------------------------
		//	Fields and constructor
		//--------------------------------------------------------------------------------------

		public OracleDataStore() : base(new OracleImplFactory())
		{
		    NameValueCollection	config = (NameValueCollection)ConfigurationSettings.GetConfig("neo.oracleclient");
			if(config == null)
				throw new ConfigurationException("Did not find neo.oracleclient config section.");
			String connectionString = config["connectionstring"];
			if(connectionString == null)
				throw new ConfigurationException("Did not find connectionstring in neo.oracleclient config section.");
				
			FinishInitialization(connectionString);
		
			logger.Debug("Created new OracleDataStore.");
		}

		public OracleDataStore(string connectionString) : base(new OracleImplFactory())
		{
			FinishInitialization(connectionString);

			logger.Debug("Created new OracleDataStore.");
		}

		public OracleDataStore(string connectionString, bool useDelimitedIdentifiers) : this(connectionString)
        {
            base.usesDelimitedIdentifiers = useDelimitedIdentifiers;
        }

		public OracleDataStore(IDbConnectionFactory connectionFactory) : base(new OracleImplFactory(), connectionFactory)
		{
			FinishInitialization(null);
			logger.Debug("Created new OracleDataStore with an IDbConnectionFactory.");
		}
	
		protected OracleDataStore(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			logger.Debug("Deserialized OracleDataStore.");
		}


		//--------------------------------------------------------------------------------------
		//	Overrides
		//--------------------------------------------------------------------------------------

		protected override void InsertRow(DataRow row, PkChangeTable pkChangeTable)	
		{
			if((row.Table.PrimaryKey.Length == 1) && (row.Table.PrimaryKey[0].AutoIncrement))
				throw new NotSupportedException("OracleDataStore does not support identity columns.");
			base.InsertRow(row, pkChangeTable);
		}
		
	
		protected override object GetFinalPk(DataRow row, IDbCommandBuilder builder)
		{
			throw new InvalidOperationException("GetFinalPk should never be invoked on OracleDataStore.");
		}


	}


}
