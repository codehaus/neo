using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using log4net;
using Neo.Database;


namespace Neo.SqlClient
{
	public class SqlDataStore : DbDataStore
	{

		//--------------------------------------------------------------------------------------
		//	Fields and constructor
		//--------------------------------------------------------------------------------------

		public SqlDataStore() : this(null)
		{
		}

		public SqlDataStore(string connectionString)
		{
			if(logger == null)
				logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

			logger.Debug("Created new SqlDataStore.");

			if(connectionString == null)
			{
				NameValueCollection	config = (NameValueCollection)ConfigurationSettings.GetConfig("neo.sqlclient");
				if(config != null)
					connectionString = config["connectionstring"];
			}
			implFactory = new SqlImplFactory();
			connection = implFactory.CreateConnection(connectionString);
		}


		//--------------------------------------------------------------------------------------
		//	Overrides
		//--------------------------------------------------------------------------------------
		
		protected override object GetFinalPk(DataRow row, IDbCommandBuilder builder)
		{
			// SELECT _SCOPE_IDENTITY AS NEW_ID in SQL8 ...
			return ExecuteScalar("SELECT @@IDENTITY AS NEW_ID", null);

		}

		//--------------------------------------------------------------------------------------
		//	Public methods unique to SqlStore
		//--------------------------------------------------------------------------------------

		public virtual void ClearTable(string tableName)
		{
			StringBuilder	builder;

			builder = new StringBuilder();
			builder.Append("DELETE FROM ");
			builder.Append(tableName);
			
			ExecuteNonQuery(builder.ToString(), null);
		}



	}


}
