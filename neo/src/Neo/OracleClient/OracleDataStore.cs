using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.OracleClient;
using System.Reflection;
using log4net;
using Neo.Core;
using Neo.Database;


namespace Neo.OracleClient
{
	public class OracleDataStore : DbDataStore
	{

		//--------------------------------------------------------------------------------------
		//	Fields and constructor
		//--------------------------------------------------------------------------------------

		public OracleDataStore() : this(null)
		{
		}

		public OracleDataStore(string connectionString)
		{
			if(logger == null)
				logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

			logger.Debug("Created new SqlDataStore.");

			if(connectionString == null)
			{
			    NameValueCollection	config = (NameValueCollection)ConfigurationSettings.GetConfig("neo.oracleclient");
				if(config != null)
					connectionString = config["connectionstring"];
			}
			implFactory = new OracleImplFactory();
			connection = implFactory.CreateConnection(connectionString);
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
