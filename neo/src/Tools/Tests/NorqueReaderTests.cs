using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using Neo.Model;


namespace Neo.Tools.Tests
{
	[TestFixture]
	public class NorqueReaderTests
	{

		protected IModelReader GetModelReader(string contents)
		{
			Neo.Model.Reader.NorqueReader reader;

			reader = new Neo.Model.Reader.NorqueReader();
			reader.LoadModel(new StringReader(contents));
			return reader;
		}


		protected IEntity GetTitleEntity(string contents)
		{
			IModelReader	reader;
			IEntity			e;

			reader = GetModelReader(contents);
			while((e = reader.GetNextEntity()) != null)
			{
				if(e.TableName == "titles")
					return e;
			}
			Assertion.Fail("Could not find entity for table 'titles.'");
			return null; // keep compiler happy
		}

		
		[Test]
		public void EntityTest() 
		{
			IEntity	e = GetTitleEntity(schema_multipleNamespaces);

			Assertion.AssertEquals("Wrong class name.", "Title", e.ClassName);
			Assertion.AssertEquals("Wrong id method (inherited.)", IdMethod.None, e.IdMethod);
			Assertion.AssertEquals("Wrong id method (inherited.)", IdMethod.None, e.IdMethod);
		}


		[Test]
		public void UsingListTest()
		{
			IEntity	e = GetTitleEntity(schema_multipleNamespaces);
			ArrayList namespaces = new ArrayList(e.UsedNamespaces);
			
			Assertion.AssertEquals("Wrong number of namespaces for 'using'.", 1, namespaces.Count);
			Assertion.Assert("Did not find namespace pubs4.Model.X", namespaces.Contains("pubs4.Model.X"));
		}

		#region Test data (multipleNamespaces)

		string schema_multipleNamespaces = @"<?xml version='1.0' encoding='ISO-8859-1' standalone='no'?>
			<!DOCTYPE database SYSTEM '../../../CodeGen/Resources/norque.dtd'>

			<database name='pubs' package='pubs4.Model' defaultIdMethod='none'>
				<table name='titles' javaName='Title' description='Title (Book) Table'>
					<column name='title_id' required='true' primaryKey='true' type='CHAR' size='6'/>
					<column name='pub_id' hidden='true' type='CHAR' size='4'/>
					<foreign-key foreignTable='publishers' name='Publisher'> 
						<reference local='pub_id' foreign='pub_id'/>
					</foreign-key>
				</table>
				<table name='publishers' javaName='Publisher' subPackage='X' description='Publisher Table'>
					<column name='pub_id' required='true' primaryKey='true' hidden='true' type='CHAR' size='4' />
				</table>
			</database>";

		#endregion


		[Test]
		public void ForeignKeyComments()
		{
			GetModelReader(schema_foreignKeyWithComments);
		}

		#region Test data (foreignKeyWithComments)

		string schema_foreignKeyWithComments = @"<?xml version='1.0' encoding='ISO-8859-1' standalone='no'?>
			<!DOCTYPE database SYSTEM '../../../SqlGen/Resources/norque.dtd'>

			<database
			name='pubs'
			package='pubs4.Model'
			defaultIdMethod='none'>

			<table name='titles' 
				javaName='Title'
				description='Title (Book) Table'>
				<column
				name='title_id'
				required='true'
				primaryKey='true'
				type='CHAR'
				size='6'
				description='Book id'/>
				<column
				name='pub_id'
				hidden='true'
				type='CHAR'
				size='4'
				description='FK Publisher'/>
				<foreign-key foreignTable='publishers' name='Publisher'> 
				<!-- comment in foreign key -->
				<reference
					local='pub_id'
					foreign='pub_id'/>
				</foreign-key>
			</table>
			</database>";

		#endregion
	

		[Test]
		[ExpectedException(typeof(ApplicationException))]
		public void CompoundForeignKeys()
		{
			GetModelReader(schema_compoundForeignKey);
		}

		#region Test data (compoundForeignKey)

		string schema_compoundForeignKey = @"<?xml version='1.0' encoding='ISO-8859-1' standalone='no'?>
			<!DOCTYPE database SYSTEM '../../../SqlGen/Resources/norque.dtd'>

			<database
			name='pubs'
			package='pubs4.Model'
			defaultIdMethod='none'>

			<table name='titles' 
				javaName='Title'
				description='Title (Book) Table'>
				<column
				name='title_id'
				required='true'
				primaryKey='true'
				type='CHAR'
				size='6'
				description='Book id'/>
				<column
				name='pub_id'
				hidden='true'
				type='CHAR'
				size='4'
				description='FK Publisher'/>
				<foreign-key foreignTable='publishers' name='Publisher'> 
				<!-- this should break -->
				<reference
					local='pub_id'
					foreign='pub_id'/>
				<reference
					local='pub_id'
					foreign='pub_id'/>
				</foreign-key>
			</table>
			</database>";

		#endregion

		[Test]
		public void TableHasUniqueElement()
		{
			GetModelReader(schema_tableWithUniqueElement);
		}

		#region Test data (uniqueConstraint)

		string schema_tableWithUniqueElement = @"<?xml version='1.0' encoding='ISO-8859-1' standalone='no'?>
			<!DOCTYPE database SYSTEM '../../../SqlGen/Resources/norque.dtd'>

			<database
			name='pubs'
			package='pubs4.Model'
			defaultIdMethod='none'>

			<table name='titles' 
				javaName='Title'
				description='Title (Book) Table'>
				<column
				name='title_id'
				required='true'
				primaryKey='true'
				type='CHAR'
				size='6'
				description='Book id'/>
				<column
				name='pub_id'
				hidden='true'
				type='CHAR'
				size='4'
				description='FK Publisher'/>
				<unique name='myUniqueCols'>
					<unique-column name='description'/>
					<unique-column name='name'/>
				</unique>
			</table>
			</database>";

		#endregion
	
	}
}