using System;
using System.IO;
using Neo.MetaModel;
using Neo.MetaModel.Reader;
using NUnit.Framework;


namespace Neo.Tools.Tests
{
	[TestFixture]
	public class XsdReaderTests
	{
		protected IModelReader GetModelReader(string contents)
		{
			XsdReader reader;

			reader = new XsdReader();
			reader.LoadModel(new StringReader(contents));
			return reader;
		}


		private Entity GetEntity(string contents, string entityName)
		{
			IModelReader reader;
			Entity e;

			reader = GetModelReader(contents);
			while((e = reader.GetNextEntity()) != null)
			{
				if(e.TableName == entityName)
					return e;
			}
			Assert.Fail("Could not find entity for table '" + entityName + ".'");
			return null; // keep compiler happy
		}

		#region Test data (singleEntity)

		const string dataSet_singleEntity = @"<?xml version='1.0' encoding='utf-8' ?>
		<xs:schema id='Pubs' targetNamespace='http://tempuri.org/Pubs.xsd' elementFormDefault='qualified'
		attributeFormDefault='qualified' xmlns='http://tempuri.org/Pubs.xsd' xmlns:mstns='http://tempuri.org/Pubs.xsd'
		xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:msdata='urn:schemas-microsoft-com:xml-msdata' xmlns:codegen='urn:schemas-microsoft-com:xml-msprop'>
		<xs:element name='Pubs' msdata:IsDataSet='true'>
		<xs:complexType>
		<xs:choice maxOccurs='unbounded'>
		<xs:element name='titles' codegen:typedName='Title'>
		<xs:complexType>
		<xs:sequence>
		<xs:element name='title_id' codegen:typedName='TitleID'>
		<xs:simpleType>
		<xs:restriction base='xs:string'>
		<xs:maxLength value='6' />
		</xs:restriction>
		</xs:simpleType>
		</xs:element>
		<xs:element name='title'>
		<xs:simpleType>
		<xs:restriction base='xs:string'>
		<xs:maxLength value='80' />
		</xs:restriction>
		</xs:simpleType>
		</xs:element>
		<xs:element name='type' >
		<xs:simpleType>
		<xs:restriction base='xs:string'>
		<xs:maxLength value='4' />
		</xs:restriction>
		</xs:simpleType>
		</xs:element>
		<xs:element name='pub_id' minOccurs='0' >
		<xs:simpleType>
		<xs:restriction base='xs:string'>
		<xs:maxLength value='4' />
		</xs:restriction>
		</xs:simpleType>
		</xs:element>
		<xs:element name='price' type='xs:decimal' minOccurs='0' />
		<xs:element name='advance' type='xs:decimal' minOccurs='0' />
		<xs:element name='royalty' type='xs:int' minOccurs='0' />
		<xs:element name='ytd_sales' type='xs:int' minOccurs='0' />
		<xs:element name='notes' minOccurs='0'>
		<xs:simpleType>
		<xs:restriction base='xs:string'>
		<xs:maxLength value='200' />
		</xs:restriction>
		</xs:simpleType>
		</xs:element>
		<xs:element name='pubdate' type='xs:dateTime' />
		</xs:sequence>
		</xs:complexType>
		</xs:element>
		</xs:choice>
		</xs:complexType>
		<xs:unique name='PubsKey1' msdata:PrimaryKey='true'>
		<xs:selector xpath='.//mstns:titles' />
		<xs:field xpath='mstns:title_id' />
		</xs:unique>
		</xs:element>
		</xs:schema>";

		#endregion

		[Test]
		public void Simple()
		{
			Entity entity = GetEntity(dataSet_singleEntity, "titles");
			Assert.IsNotNull(entity);
			Assert.AreEqual("Title", entity.ClassName);
			Assert.AreEqual("titles", entity.TableName);
			Assert.AreEqual(10, entity.Attributes.Count);
			foreach (EntityAttribute entityAttribute in entity.Attributes)
			{
				switch(entityAttribute.ColumnName)
				{
					case "title_id":
						Assert.AreEqual("TitleID", entityAttribute.PropertyName);
						Assert.AreEqual("6", entityAttribute.Size);
						Assert.IsTrue(entityAttribute.IsPkColumn, "title_id not a pk");
						Assert.IsTrue(entityAttribute.IsRequired);
						Assert.AreEqual(typeof(String), entityAttribute.PropertyType);
						Assert.AreEqual("VARCHAR", entityAttribute.ColumnType);
						break;
					case "title":
						Assert.AreEqual("Title", entityAttribute.PropertyName);
						Assert.AreEqual("80", entityAttribute.Size);
						Assert.IsFalse(entityAttribute.IsPkColumn, "title a pk");
						Assert.IsTrue(entityAttribute.IsRequired);
						break;
					case "pub_id":
						Assert.AreEqual("PubId", entityAttribute.PropertyName);
						Assert.AreEqual("4", entityAttribute.Size);
						Assert.IsFalse(entityAttribute.IsPkColumn, "pub_id a pk");
						Assert.IsFalse(entityAttribute.IsRequired);
						break;
					case "price":
						Assert.AreEqual("Price", entityAttribute.PropertyName);
						Assert.IsFalse(entityAttribute.IsPkColumn, "price a pk");
						Assert.IsFalse(entityAttribute.IsRequired);
						break;
					case "advance":
						Assert.AreEqual("Advance", entityAttribute.PropertyName);
						Assert.IsFalse(entityAttribute.IsPkColumn, "advance a pk");
						Assert.IsFalse(entityAttribute.IsRequired);
						break;
					case "royalty":
						Assert.AreEqual("Royalty", entityAttribute.PropertyName);
						Assert.IsFalse(entityAttribute.IsPkColumn, "royalty a pk");
						Assert.IsFalse(entityAttribute.IsRequired);
						break;
					case "ytd_sales":
						Assert.AreEqual("YtdSales", entityAttribute.PropertyName);
						Assert.IsFalse(entityAttribute.IsPkColumn, "ytd_sales a pk");
						Assert.IsFalse(entityAttribute.IsRequired);
						break;
					case "notes":
						Assert.AreEqual("Notes", entityAttribute.PropertyName);
						Assert.AreEqual("200", entityAttribute.Size);
						Assert.IsFalse(entityAttribute.IsPkColumn, "notes a pk");
						Assert.IsFalse(entityAttribute.IsRequired);
						break;
				}
				Assert.IsNotNull(entityAttribute);
			}
		}

		#region Test data (relation)

		private const string dataSet_relationship = @"<?xml version='1.0' encoding='utf-8' ?>
		<xs:schema id='Pubs' targetNamespace='http://tempuri.org/Pubs.xsd' elementFormDefault='qualified'
		attributeFormDefault='qualified' xmlns='http://tempuri.org/Pubs.xsd' xmlns:mstns='http://tempuri.org/Pubs.xsd'
		xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:msdata='urn:schemas-microsoft-com:xml-msdata'
		xmlns:codegen='urn:schemas-microsoft-com:xml-msprop'>
		<xs:element name='Pubs' msdata:IsDataSet='true'>
		<xs:complexType>
		<xs:choice maxOccurs='unbounded'>
		<xs:element name='titles' codegen:typedName='Title'>
		<xs:complexType>
		<xs:sequence>
		<xs:element name='title_id' codegen:typedName='TitleID'>
		<xs:simpleType>
		<xs:restriction base='xs:string'>
		<xs:maxLength value='6' />
		</xs:restriction>
		</xs:simpleType>
		</xs:element>
		<xs:element name='title'>
		<xs:simpleType>
		<xs:restriction base='xs:string'>
		<xs:maxLength value='80' />
		</xs:restriction>
		</xs:simpleType>
		</xs:element>
		<xs:element name='type'>
		<xs:simpleType>
		<xs:restriction base='xs:string'>
		<xs:maxLength value='4' />
		</xs:restriction>
		</xs:simpleType>
		</xs:element>
		<xs:element name='pub_id' minOccurs='0'>
		<xs:simpleType>
		<xs:restriction base='xs:string'>
		<xs:maxLength value='4' />
		</xs:restriction>
		</xs:simpleType>
		</xs:element>
		<xs:element name='price' type='xs:decimal' minOccurs='0' />
		<xs:element name='advance' type='xs:decimal' minOccurs='0' />
		<xs:element name='royalty' type='xs:int' minOccurs='0' />
		<xs:element name='ytd_sales' type='xs:int' minOccurs='0' />
		<xs:element name='notes' minOccurs='0'>
		<xs:simpleType>
		<xs:restriction base='xs:string'>
		<xs:maxLength value='200' />
		</xs:restriction>
		</xs:simpleType>
		</xs:element>
		<xs:element name='pubdate' type='xs:dateTime' />
		</xs:sequence>
		</xs:complexType>
		</xs:element>
		<xs:element name='publishers' codegen:typedName='Publisher'>
		<xs:complexType>
		<xs:sequence>
		<xs:element name='pub_id'>
		<xs:simpleType>
		<xs:restriction base='xs:string'>
		<xs:maxLength value='4' />
		</xs:restriction>
		</xs:simpleType>
		</xs:element>
		<xs:element name='pub_name' minOccurs='0'>
		<xs:simpleType>
		<xs:restriction base='xs:string'>
		<xs:maxLength value='40' />
		</xs:restriction>
		</xs:simpleType>
		</xs:element>
		<xs:element name='city' minOccurs='0'>
		<xs:simpleType>
		<xs:restriction base='xs:string'>
		<xs:maxLength value='20' />
		</xs:restriction>
		</xs:simpleType>
		</xs:element>
		<xs:element name='state' minOccurs='0'>
		<xs:simpleType>
		<xs:restriction base='xs:string'>
		<xs:maxLength value='2' />
		</xs:restriction>
		</xs:simpleType>
		</xs:element>
		<xs:element name='country' minOccurs='0'>
		<xs:simpleType>
		<xs:restriction base='xs:string'>
		<xs:maxLength value='30' />
		</xs:restriction>
		</xs:simpleType>
		</xs:element>
		</xs:sequence>
		</xs:complexType>
		</xs:element>
		</xs:choice>
		</xs:complexType>
		<xs:unique name='titlepk' msdata:PrimaryKey='true'>
		<xs:selector xpath='.//mstns:titles' />
		<xs:field xpath='mstns:title_id' />
		</xs:unique>
		<xs:unique name='publisherpk' msdata:PrimaryKey='true'>
		<xs:selector xpath='.//mstns:publishers' />
		<xs:field xpath='mstns:pub_id' />
		</xs:unique>
		<xs:keyref name='publisherstitles' refer='publisherpk' codegen:typedParent='Publisher' codegen:typedChildren='Titles'>
		<xs:selector xpath='.//mstns:titles' />
		<xs:field xpath='mstns:pub_id' />
		</xs:keyref>
		</xs:element>
		</xs:schema>";

		#endregion

		[Test]
		public void Relations()
		{
			Entity titles = GetEntity(dataSet_relationship, "titles");
			Assert.IsNotNull(titles);
			Assert.IsTrue(titles.Relationships.Count > 0);
			EntityRelationship[] relationships = new EntityRelationship[1];
			titles.Relationships.CopyTo(relationships, 0);
			EntityRelationship relationship = relationships[0];
			Assert.AreEqual("Publisher", relationship.VarName);
			Assert.AreEqual("pub_id", relationship.ForeignKey);
			Assert.AreEqual("publishers", relationship.ForeignTableName);
			Assert.AreEqual("pub_id", relationship.LocalKey);

			Entity publishers = GetEntity(dataSet_relationship, "publishers");
			Assert.IsNotNull(publishers);
			Assert.IsTrue(publishers.Relationships.Count > 0);
			publishers.Relationships.CopyTo(relationships, 0);
			relationship = relationships[0];
			Assert.AreEqual("Titles", relationship.VarName);
			Assert.AreEqual("pub_id", relationship.ForeignKey);
			Assert.AreEqual("titles", relationship.ForeignTableName);
			Assert.AreEqual("pub_id", relationship.LocalKey);
		}

	}
}