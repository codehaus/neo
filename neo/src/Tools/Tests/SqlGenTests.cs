using System;
using System.Data.SqlClient;
using System.IO;
using Neo.SqlGen;
using NUnit.Framework;


namespace Neo.Tools.Tests 
{
    [TestFixture]
	public class TestTransform : Assertion
	{
		//TODO:Put in config file
		string connectionString = "data source=virtserver;initial catalog=pubs;user id=dev;password=passw0rd";
		string multiTableSchema = "..\\..\\MultiTableSchema.xml";
		string transform = "..\\..\\..\\SqlGen\\Resources";
		string validOutput = "..\\..\\validOutput.sql";

	    SqlGenerator _schemaGenerator;

		[SetUp]
		public void Setup() 
		{
			_schemaGenerator = new SqlGenerator();
			_schemaGenerator.ResourcePath = transform;
		}

		[Test]
		public void CheckSqlIsValid() 
		{
			string sql = "SET PARSEONLY ON " + _schemaGenerator.ProcessFile(multiTableSchema);
		    SqlConnection conn = new SqlConnection(connectionString);
			conn.Open();
			SqlCommand command = new SqlCommand(sql, conn);

			try 
			{
				command.ExecuteNonQuery();
			}
			catch(SqlException sex) 
			{
				Fail("SQL Error: " + sex.Message);
			}
			finally 
			{
				conn.Close();
			}
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void HandleBadPath() 
		{
			_schemaGenerator.ProcessFile("");
		}

		[Test]
		public void CheckConverterOutput() 
		{
		    FileStream stream = File.OpenRead(validOutput);
			string validSql = new StreamReader(stream).ReadToEnd();
			stream.Close();

			string outputSql = _schemaGenerator.ProcessFile(multiTableSchema);
		
			//Output to a file when the test fails - it's easier to read
			//
			//StreamWriter writer = File.CreateText("output.sql");
			//writer.Write(outputSql);
			//writer.Close();

			AssertEquals(validSql, outputSql);
		}

		[Test]
		public void GenerateSqlFile() 
		{
			File.Delete("MultiTableSchema.sql");
			_schemaGenerator.GenerateSqlFile(multiTableSchema);
			Assert(File.Exists("MultiTableSchema.sql"));
		}
	}
}