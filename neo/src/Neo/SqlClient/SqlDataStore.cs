using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Runtime.Serialization;
using System.Text;
using Neo.Database;


namespace Neo.SqlClient
{
	[Serializable]
	public class SqlDataStore : DbDataStore
	{

		//--------------------------------------------------------------------------------------
		//	Fields and constructor
		//--------------------------------------------------------------------------------------

		public SqlDataStore() : base()
		{
			NameValueCollection	config = (NameValueCollection)ConfigurationSettings.GetConfig("neo.sqlclient");
			if(config == null)
				throw new ConfigurationException("Did not find neo.sqlclient config section.");
			String connectionString = config["connectionstring"];
			if(connectionString == null)
				throw new ConfigurationException("Did not find connectionstring in neo.sqlclient config section.");
			
			implFactory = new SqlImplFactory();
			connection = implFactory.CreateConnection(connectionString);
	
			logger.Debug("Created new SqlDataStore.");
		}

		public SqlDataStore(string connectionString) : base()
		{
			implFactory = new SqlImplFactory();
			connection = implFactory.CreateConnection(connectionString);

			logger.Debug("Created new SqlDataStore.");
		}

		protected SqlDataStore(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			logger.Debug("Deserialized SqlDataStore.");
		}


		//--------------------------------------------------------------------------------------
		//	Overrides
		//--------------------------------------------------------------------------------------
		
		protected override object GetFinalPk(DataRow row, IDbCommandBuilder builder)
		{
			object result;
			Type   pkType;
			
			// SELECT _SCOPE_IDENTITY AS NEW_ID in SQL8 ...
			result = ExecuteScalar("SELECT @@IDENTITY AS NEW_ID", null);

			pkType = row.Table.PrimaryKey[0].DataType;
			if((result.GetType() != pkType) && (result is IConvertible))
				result = Convert.ChangeType(result, pkType);
		
			return result;
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
