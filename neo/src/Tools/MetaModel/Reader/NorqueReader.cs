using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using Neo.MetaModel;


namespace Neo.MetaModel.Reader
{
	public class NorqueReader : IModelReader
	{
		//--------------------------------------------------------------------------------------
		//	fields and constructor
		//--------------------------------------------------------------------------------------
	
		Model			model;
		IEnumerator		entityEnum;


		//--------------------------------------------------------------------------------------
		//	IModelReader impl
		//--------------------------------------------------------------------------------------

		public void ReadConfiguration(string path, ConfigDelegate aDelegate)
		{
		    Regex		attrExp;
			XmlReader	reader;
			
			attrExp = new Regex("(?<attr>[a-z]+?)\\s*=\\s*['\"](?<val>.+?)['\"]");
			reader = new XmlTextReader(path);
			try
			{	
				while(reader.Read())
				{
					if((reader.NodeType != XmlNodeType.ProcessingInstruction) || (reader.Name != "neo"))
						continue;
					Match match = attrExp.Match(reader.Value);
					while(match.Success)
					{
						aDelegate(path, match.Result("${attr}"), match.Result("${val}"));
						match = match.NextMatch();
					}
				}
			}
			finally
			{
				reader.Close();
			}
		}


		public void LoadModel(string path)
		{
			XmlTextReader reader = new XmlTextReader(path);
			try
			{
				LoadModel(reader);
			}
			finally
			{
				reader.Close();
			}
		}

		public void LoadModel(TextReader textReader)
		{
			XmlTextReader reader = new XmlTextReader(textReader);
			try
			{
				LoadModel(reader);
			}
			finally
			{
				reader.Close();
			}
		}

		protected void LoadModel(XmlTextReader xmlReader)
		{
			XmlValidatingReader	reader;
			XmlDocument			doc;

			reader = new XmlValidatingReader(xmlReader);
			reader.ValidationType = ValidationType.DTD;

			doc = new XmlDocument();
			doc.Load(reader);

			model = ReadModel(doc.DocumentElement);
			Console.WriteLine("Read model; found {0} entities.", model.Entities.Count);
			entityEnum = model.Entities.GetEnumerator();
		}

		public Model Model
		{
			get { return model; }
		}

		public Entity GetNextEntity()
		{
			if(entityEnum.MoveNext() == false)
				return null;
			return (Entity)entityEnum.Current;
		}
	

		//--------------------------------------------------------------------------------------
		//	methods to create model objects from dom nodes
		//--------------------------------------------------------------------------------------

		private Model ReadModel(XmlElement el)
		{
			Model m = new Model();

			m.Name = ValueForAttribute(el, "name");
			m.Namespace = ValueForAttribute(el, "package");
			
			foreach(XmlElement entityElement in el.SelectNodes("table"))
				m.AddEntity(ReadEntity(m, entityElement));
		
			return m;
		}


		private Entity ReadEntity(Model model, XmlElement el)
		{
		    Entity e = new Entity(model);

			e.TableName = ValueForAttribute(el, "name");
			e.IdMethod = GetIdMethod(el);

			if((e.ClassName = ValueForAttribute(el, "javaName")) == null)
				e.ClassName = ModelHelper.PrettifyName(ValueForAttribute(el, "name"), GetNamingMethod(el));
			e.BaseClassName = ValueForAttribute(el, "baseClass", "baseClass");
			e.SubPackageName = ValueForAttribute(el, "subPackage");

			foreach(XmlElement attrEl in el.SelectNodes("column"))
				e.AddAttribute(ReadAttribute(e, attrEl));
			foreach(XmlElement relEl in el.SelectNodes("foreign-key"))
				e.AddRelationship(ReadReleationship(e, relEl));
			foreach(XmlElement relEl in el.SelectNodes("iforeign-key"))
				e.AddRelationship(ReadReleationship(e, relEl));
		
			return e;
		}


		private EntityAttribute ReadAttribute(Entity entity, XmlElement el)
		{
		    EntityAttribute a = new EntityAttribute(entity);
		
			a.ColumnName = ValueForAttribute(el, "name");
			a.ColumnType = ValueForAttribute(el, "type");
			a.Size = ValueForAttribute(el, "size");

			if((ValueForAttribute(el, "type") == "VARCHAR") && (ValueForAttribute(el, "size") == null))
				throw new ApplicationException(String.Format("{0}.{1}: Must specify size for VARCHAR.", entity.TableName, a.ColumnName));
			
			a.IsPkColumn = ValueForAttribute(el, "primaryKey").Equals("true");
			a.AllowsNull = (a.IsPkColumn == false) && (ValueForAttribute(el, "required").Equals("true") == false);

			a.IsHidden = ValueForAttribute(el, "hidden").Equals("true");
			a.IsRequired = ValueForAttribute(el, "required").Equals("true");

			if((a.PropertyName = ValueForAttribute(el, "javaName")) == null)
				a.PropertyName = ModelHelper.PrettifyName(ValueForAttribute(el, "name"), GetNamingMethod(el));
			a.PropertyType = GetDotNetType(a.ColumnType);
			a.PropertyAttributes = ValueForAttribute(el, "attributes");
				
			return a;
		}


		private EntityRelationship ReadReleationship(Entity entity, XmlElement el)
		{
		    EntityRelationship r = new EntityRelationship(entity);

			r.ForeignTableName = ValueForAttribute(el, "foreignTable");

			XmlNodeList refElementList = el.SelectNodes("reference|ireference");
			if(refElementList.Count > 1)
				throw new ApplicationException("Compound foreign keys are not supported by Neo.");
			r.LocalKey = ValueForAttribute((XmlElement)refElementList[0], "local");
			r.ForeignKey = ValueForAttribute((XmlElement)refElementList[0], "foreign");

			r.UpdateRule = GetDotNetRuleForStringValue(ValueForAttribute(el, "onUpdate"), Rule.Cascade);
			r.DeleteRule = GetDotNetRuleForStringValue(ValueForAttribute(el, "onDelete"), Rule.SetNull);

			r.VarName = ValueForAttribute(el, "name");

			foreach(EntityAttribute attribute in entity.Attributes)
			{
				if(attribute.ColumnName == r.LocalKey)
				{
					r.RelationshipAttributes = attribute.PropertyAttributes;
				}
			}

			return r;
		}

	
		//--------------------------------------------------------------------------------------
		//	helper methods to get data from the dom
		//--------------------------------------------------------------------------------------
		
		protected string ValueForAttribute(XmlElement element, string attrName)
		{
			XmlAttribute attr = element.Attributes[attrName];
			return ((attr != null) && (attr.Value != "null")) ? attr.Value : null;
		}


		protected string ValueForAttribute(XmlElement element, string attrName, bool searchUp)
		{
			string	attrValue = ValueForAttribute(element, attrName);
			if(attrValue != null)
				return attrValue;
			if((searchUp) && (element.ParentNode != null) && (element.ParentNode is XmlElement))
				return ValueForAttribute((XmlElement)element.ParentNode, attrName, searchUp);
			return null;
		}

		
		protected string ValueForAttribute(XmlElement element, string attrName, string defaultAttrName)
		{
			string stringval = ValueForAttribute(element, attrName, false);
			if(stringval != null)
				return stringval;
			return ValueForAttribute(element, defaultAttrName, true);
		}

		
		protected NamingMethod GetNamingMethod(XmlElement element)
		{
			string stringval = ValueForAttribute(element, "javaNamingMethod", "defaultJavaNamingMethod");

			if((stringval == null ) || (stringval == "underscore"))
				return NamingMethod.Underscore;
			else if(stringval == "nochange")
				return NamingMethod.NoChange;
			else if(stringval == "javaname")
				return NamingMethod.CamelCase;

			throw new ArgumentException("Invalid naming method: " + stringval);
		}


		protected IdMethod GetIdMethod(XmlElement element)
		{
			string stringval = ValueForAttribute(element, "idMethod", "defaultIdMethod");
				
			if(stringval == "none")
				return IdMethod.None;
			else if(stringval == "idbroker")
				return IdMethod.IdBroker;
			else if(stringval == "native")
				return IdMethod.Native;
			else if(stringval == "guid")
				return IdMethod.Guid;
			
			throw new ArgumentException("Invalid idmethod: " + stringval);
		}


		protected Rule GetDotNetRuleForStringValue(string stringval, Rule defaultRule)
		{
			if(stringval == null) 
				return defaultRule;
		
			if(stringval == "setnull")
				return Rule.SetNull;
			else if(stringval == "cascade")
				return Rule.Cascade;
			else if(stringval == "none")
				return Rule.None;

			throw new ArgumentException("Invalid update action: " + stringval);
		}



		/*
		  BIT  | TINYINT | SMALLINT    | INTEGER    | BIGINT    | FLOAT
		| REAL | NUMERIC | DECIMAL     | CHAR       | VARCHAR   | LONGVARCHAR
		| DATE | TIME    | TIMESTAMP   | BINARY     | VARBINARY | LONGVARBINARY
		| NULL | OTHER   | JAVA_OBJECT | DISTINCT   | STRUCT    | ARRAY
		| BLOB | CLOB    | REF         | BOOLEANINT | BOOLEANCHAR
		| DOUBLE
		*/

		private static StringDictionary dotnetTypes;

		private Type GetDotNetType(string norqueType)
		{
			if(dotnetTypes == null)
			{
				string[][] typemap1 = {
								new string[] { "System.Boolean",	"BIT", },
								new string[] { "System.Int16",		"TINYINT", "SMALLINT" },
								new string[] { "System.Int32",		"INTEGER" },
								new string[] { "System.Int64",		"BIGINT" },
								new string[] { "System.Double",		"REAL", "DOUBLE", "FLOAT" },
								new string[] { "System.Decimal",	"NUMERIC", "DECIMAL" },
								new string[] { "System.String",		"CHAR", "VARCHAR", "LONGVARCHAR" },
								new string[] { "System.DateTime",	"DATE", "TIME", "TIMESTAMP" },
								new string[] { "System.Byte[]",		"BINARY", "VARBINARY" },
								new string[] { "System.Guid",		"UNIQUEIDENTIFIER" }
							};
				dotnetTypes = new StringDictionary();
				for(int i = 0; i < typemap1.Length; i++)
					for(int j = 1; j < typemap1[i].Length; j++)
						dotnetTypes.Add(typemap1[i][j], typemap1[i][0]);
			}

			if(dotnetTypes[norqueType] == null)
				throw new ArgumentException("Column type " + norqueType + " not supported.");

			return Type.GetType(dotnetTypes[norqueType]);
		}

	}

}