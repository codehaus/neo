using System;
using System.Collections;
using System.Data;
using System.IO;


namespace Neo.MetaModel.Reader
{
	public class XsdReader : IModelReader
	{
		private Model model;
		private IEnumerator entityEnum;


		public void LoadModel(string path)
		{
			using(TextReader textReader = File.OpenText(path))
				LoadModel(textReader);
		}

		public void LoadModel(TextReader reader)
		{
			model = ReadModel(reader);
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

		public void ReadConfiguration(string path, ConfigDelegate aDelegate)
		{
			// check CodeGenerator.setPiAttribute(...) to see what can be done
		}

		protected Model ReadModel(TextReader reader)
		{
			DataSet dataSet = new DataSet();
			dataSet.ReadXmlSchema(reader);

			Model model = new Model();
			model.Name = dataSet.DataSetName;
			// this is simply wrong: model.Namespace = dataSet.Namespace;
			foreach (DataTable table in dataSet.Tables)
			{
				model.AddEntity(ReadEntity(model, table));
			}
			return model;
		}

		private Entity ReadEntity(Model model, DataTable table)
		{
			Entity entity = new Entity(model);

			entity.TableName = table.TableName;
			if(table.ExtendedProperties.ContainsKey("typedName"))
				entity.ClassName = (string)table.ExtendedProperties["typedName"];
			else
				entity.ClassName = ModelHelper.PrettifyName(table.TableName, NamingMethod.CamelCase);
			foreach (DataColumn column in table.Columns)
			{
				entity.AddAttribute(CreateAttibute(entity, column));
			}
			foreach (DataRelation relation in table.DataSet.Relations)
			{
				if(relation.ParentTable.Equals(table))
					entity.AddRelationship(CreateRelation(entity, relation, relation.ChildTable.TableName));
				else if(relation.ChildTable.Equals(table))
					entity.AddRelationship(CreateRelation(entity, relation, relation.ParentTable.TableName));
			}
			return entity;
		}

		private EntityRelationship CreateRelation(Entity entity, DataRelation dataRelation, string foreignTableName)
		{
			EntityRelationship relationship = new EntityRelationship(entity);
			relationship.ForeignTableName = foreignTableName;
			if((dataRelation.ParentColumns.Length > 1) || (dataRelation.ChildColumns.Length > 1))
				throw new ApplicationException("Compound foreign keys are not supported by Neo.");
			relationship.LocalKey = dataRelation.ParentColumns[0].ColumnName;
			relationship.ForeignKey = dataRelation.ChildColumns[0].ColumnName;

			relationship.DeleteRule = dataRelation.ChildKeyConstraint.DeleteRule;
			relationship.UpdateRule = dataRelation.ChildKeyConstraint.UpdateRule;
			if((foreignTableName.Equals(dataRelation.ParentTable.TableName)) && (dataRelation.ExtendedProperties.ContainsKey("typedParent")))
				relationship.VarName = (string)dataRelation.ExtendedProperties["typedParent"];
			else if((foreignTableName.Equals(dataRelation.ChildTable.TableName)) && (dataRelation.ExtendedProperties.ContainsKey("typedChildren")))
				relationship.VarName = (string)dataRelation.ExtendedProperties["typedChildren"];
			else
				relationship.VarName = dataRelation.RelationName;


			foreach (EntityAttribute attribute in entity.Attributes)
			{
				if(attribute.ColumnName == relationship.LocalKey)
					relationship.RelationshipAttributes = attribute.PropertyAttributes;
			}
			return relationship;
		}

		private EntityAttribute CreateAttibute(Entity entity, DataColumn column)
		{
			EntityAttribute attribute = new EntityAttribute(entity);

			attribute.ColumnName = column.ColumnName;
			if(column.ExtendedProperties.ContainsKey("typedName"))
				attribute.PropertyName = (string)column.ExtendedProperties["typedName"];
			else
				attribute.PropertyName = ModelHelper.PrettifyName(column.ColumnName, NamingMethod.CamelCase);
			if((column.DataType.Equals(typeof(String))) && (column.MaxLength == -1))
				throw new ApplicationException(String.Format("{0}.{1}: Must set max length for string DataType.", entity.TableName, column.ColumnName));

			attribute.SetDefaultColumnType(column.DataType);
			attribute.Size = column.MaxLength.ToString();
			attribute.AllowsNull = column.AllowDBNull;
			attribute.IsRequired = !column.AllowDBNull;

			foreach (DataColumn primaryKey in column.Table.PrimaryKey)
			{
				if(column.Equals(primaryKey))
				{
					attribute.IsPkColumn = true;
					break;
				}
			}

			attribute.PropertyType = column.DataType;

			return attribute;

		}

	}
}