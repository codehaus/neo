using System;
using System.Collections;
using System.IO;
using Neo.MetaModel;
using Neo.MetaModel.Reader;
using NUnit.Framework;


namespace Neo.Tools.Tests
{
	[TestFixture]
	public class NorqueReaderTests
	{

		protected IModelReader GetModelReader(string contents)
		{
		    NorqueReader reader;

			reader = new NorqueReader();
			reader.LoadModel(new StringReader(contents));
			return reader;
		}


		protected Entity GetTitleEntity(string contents)
		{
			IModelReader	reader;
			Entity			e;

			reader = GetModelReader(contents);
			while((e = reader.GetNextEntity()) != null)
			{
				if(e.TableName == "titles")
					return e;
			}
			Assert.Fail("Could not find entity for table 'titles.'");
			return null; // keep compiler happy
		}

		
		[Test]
		public void EntityTest() 
		{
			Entity	e = GetTitleEntity(schema_multipleNamespaces);

			Assert.AreEqual("Title", e.ClassName, "Wrong class name.");
			Assert.AreEqual(IdMethod.None, e.IdMethod, "Wrong id method (inherited.)");
			Assert.AreEqual(IdMethod.None, e.IdMethod, "Wrong id method (inherited.)");
		}


		[Test]
		public void UsingListTest()
		{
			Entity	e = GetTitleEntity(schema_multipleNamespaces);
		    ArrayList namespaces = new ArrayList(e.UsedNamespaces);
			
			Assert.AreEqual(1, namespaces.Count, "Wrong number of namespaces for 'using'.");
			Assert.IsTrue(namespaces.Contains("pubs4.Model.X"), "Did not find namespace pubs4.Model.X");
		}

		#region Test data (multipleNamespaces)

		string schema_multipleNamespaces = @"<?xml version='1.0' encoding='ISO-8859-1' standalone='no'?>
			<!DOCTYPE database SYSTEM '../../../MetaModel/Reader/norque.dtd'>

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
			<!DOCTYPE database SYSTEM '../../../MetaModel/Reader/norque.dtd'>

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
			<!DOCTYPE database SYSTEM '../../../MetaModel/Reader/norque.dtd'>

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
			<!DOCTYPE database SYSTEM '../../../MetaModel/Reader/norque.dtd'>

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
