using System;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Text;
using Neo.Core;
using Neo.Core.Qualifiers;
using Neo.Core.Util;
using Neo.Framework;


namespace Neo.Database
{
	public class GenericSql92Builder : IDbCommandBuilder, IQualifierVisitor, ICloneable
	{
		//--------------------------------------------------------------------------------------
		//	Fields and Constructor
		//--------------------------------------------------------------------------------------

		protected DataTable					table;
		protected IDbImplementationFactory	implFactory;
		protected IEntityMap				emap;
		protected StringBuilder				builder;
		protected IList						parameters;
		protected bool						usesDelimitedIdentifiers;
		protected Hashtable					delimitedIdentifiers;

		public GenericSql92Builder(DataTable aTable, IDbImplementationFactory aParamFactory)
		{
			table = aTable;
			implFactory = aParamFactory;

			builder = new StringBuilder();
			parameters = new ArrayList();
			delimitedIdentifiers = new Hashtable();
		}


		//--------------------------------------------------------------------------------------
		//	ICloneable impl
		//--------------------------------------------------------------------------------------

		public virtual object Clone()
		{
			return (GenericSql92Builder)this.MemberwiseClone();
		}

		
		//--------------------------------------------------------------------------------------
		//	accessors
		//--------------------------------------------------------------------------------------
		
		public bool UsesDelimitedIdentifiers
		{
			set { usesDelimitedIdentifiers = value; }
			get { return usesDelimitedIdentifiers; }
		}


		public string Command
		{
			get { return builder.ToString(); }
		}
		

		public IList Parameters	
		{
			get { return parameters; }
		}


		//--------------------------------------------------------------------------------------
		//  SELECT
		//--------------------------------------------------------------------------------------

		public virtual void WriteSelect(IFetchSpecification fetchSpec)
		{
			emap = fetchSpec.EntityMap;

			builder.Append("SELECT ");
			WriteColumns(table.Columns);

			builder.Append(" FROM ");
			WriteIdentifier(table.TableName);

			if(fetchSpec.Qualifier != null)
			{
				builder.Append(" WHERE ");
				WriteQualifier(fetchSpec.Qualifier);
			}
			if(fetchSpec.SortOrderings != null)
			{
				builder.Append(" ORDER BY ");
				WriteSortOrderings(fetchSpec.SortOrderings);
			}
			if(fetchSpec.FetchLimit != -1)
			{
				throw new InvalidOperationException("SQL92 does not support fetch limits.");
			}

		}


		protected virtual void WriteQualifier(Qualifier qualifier)
		{
			qualifier.AcceptVisitor(this);
		}


		protected virtual void WriteSortOrderings(PropertyComparer[] orderings)
		{
			bool first = true;
			foreach(PropertyComparer c in orderings)
			{
				if(first == false)
					builder.Append(", ");
				WriteIdentifier(emap.GetColumnForAttribute(c.Property));
				if(c.SortDirection == SortDirection.Descending)
					builder.Append(" DESC");
				first = false;
			}
		}


		//--------------------------------------------------------------------------------------
		//	IQualifierVisitor impl
		//--------------------------------------------------------------------------------------
		
		public object VisitColumnQualifier(ColumnQualifier q)
		{
			WriteIdentifier(q.Column);

			if(q.Predicate.Value == DBNull.Value)
			{
				if(q.Predicate is EqualsPredicate)
					builder.Append(" IS NULL ");
				else if(q.Predicate is NotEqualPredicate)
					builder.Append(" IS NOT NULL ");
				else
					throw new ArgumentException("Invalid predicate with null value; found " + q.Predicate.GetType().FullName);
			}
			else
			{
				// Sequence matters because predicates inherit from each other.
				if(q.Predicate is NotEqualPredicate)
					builder.Append("<>");
				else if(q.Predicate is EqualsPredicate)
					builder.Append("=");
				else if(q.Predicate is LessThanPredicate)
					builder.Append("<");
				else if(q.Predicate is LessOrEqualPredicate)
					builder.Append("<=");
				else if(q.Predicate is GreaterThanPredicate)
					builder.Append(">");
				else if(q.Predicate is GreaterOrEqualPredicate)
					builder.Append(">=");
				else if(q.Predicate is LikePredicate)
					builder.Append(" LIKE ");
				else
					throw new ArgumentException("Invalid predicate in qualifier; found " + q.Predicate.GetType().FullName);
				
				// add the current parameter count as suffix to ensure names are unique
				DataColumn column = table.Columns[q.Column];
				string pname = ConvertToParameterName(column.ColumnName + parameters.Count.ToString());
				IDataParameter param = implFactory.CreateParameter(column, pname, q.Predicate.Value);
				builder.Append(param.ParameterName);
				parameters.Add(param);
			}

			return null;
		}


		public object VisitPropertyQualifier(PropertyQualifier q)
		{
			ColumnQualifier	cq = new QualifierConverter(emap).ConvertPropertyQualifier(q);
			cq.AcceptVisitor(this);
			
			return null;
		}


		public object VisitAndQualifier(AndQualifier q)
		{
			bool isFirstChild = true;
			builder.Append("(");
			foreach(Qualifier child in q.Qualifiers)
			{
				if(isFirstChild == false)
					builder.Append(" AND ");
				isFirstChild = false;
				child.AcceptVisitor(this);
			}
			builder.Append(")");

			return null;
		}


		public object VisitOrQualifier(OrQualifier q)
		{
			bool isFirstChild = true;
			builder.Append("(");
			foreach(Qualifier child in q.Qualifiers)
			{
				if(isFirstChild == false)
					builder.Append(" OR ");
				isFirstChild = false;
				child.AcceptVisitor(this);
			}
			builder.Append(")");

			return null;
		}


		public object VisitPathQualifier(PathQualifier q)
		{
			WritePathQualifier(table, emap, q, 0);
			
			return null;
		}


		private void WritePathQualifier(DataTable table, IEntityMap emap, PathQualifier q, int idx)
		{
			PropertyInfo	propInfo;
			FieldInfo		fieldInfo;
			Type			destType;
			IEntityMap		leftEmap, rightEmap, newEmap;
			DataRelation	rel;

			if((propInfo = emap.ObjectType.GetProperty(q.PathElements[idx])) != null)
				destType = propInfo.PropertyType;
			else if((fieldInfo = emap.ObjectType.GetField(q.PathElements[idx])) != null)
				destType = fieldInfo.FieldType;
			else
				throw new InvalidPropertyException(String.Format("{0} is not a valid property/field for class {1}", q.PathElements[idx], emap.ObjectType), null);

			if(typeof(ObjectCollectionBase).IsAssignableFrom(destType))
			{
				destType = destType.GetProperty("Item").PropertyType;
				leftEmap = emap;
				rightEmap = newEmap = emap.Factory.GetMap(destType);
			}
			else
			{
				leftEmap = newEmap = emap.Factory.GetMap(destType);
				rightEmap = emap;
			}

			if((rel = table.DataSet.Relations[leftEmap.TableName + "." + rightEmap.TableName]) == null)
				throw new NeoException("Can't to convert write PathQualifier; did not find relation " + leftEmap.TableName + "." + rightEmap.TableName);

			WriteIdentifier((emap == rightEmap) ? rel.ChildColumns[0].ColumnName : rel.ParentColumns[0].ColumnName);
			builder.Append(" IN ( SELECT ");
			WriteIdentifier((emap == rightEmap) ? rel.ParentColumns[0].ColumnName : rel.ChildColumns[0].ColumnName);
			builder.Append(" FROM ");
			WriteIdentifier(newEmap.TableName);
			builder.Append(" WHERE ");

			newEmap.UpdateSchemaInDataSet(table.DataSet, SchemaUpdate.Basic | SchemaUpdate.Relations);
			if(idx < q.PathElements.Length - 1)
			{
				WritePathQualifier(table.DataSet.Tables[newEmap.TableName], newEmap, q, idx + 1);
			}
			else
			{
				GenericSql92Builder clone = (GenericSql92Builder)this.Clone();
				clone.table = table.DataSet.Tables[newEmap.TableName];
				clone.emap = newEmap;
				q.Qualifier.AcceptVisitor(clone);
			}

			builder.Append(" )");
		}


		//--------------------------------------------------------------------------------------
		//	INSERT
		//--------------------------------------------------------------------------------------

		public virtual void WriteInsert(DataRow row, IList columnList)
		{
			builder.Append("INSERT INTO ");
			WriteIdentifier(row.Table.TableName);
			builder.Append(" (");
			WriteColumns(columnList);
			builder.Append(") VALUES (");
			WriteParameters(columnList);
			builder.Append(")");
			
			foreach(DataColumn column in columnList)
			{
				string pname = ConvertToParameterName(column.ColumnName);
				parameters.Add(implFactory.CreateParameter(column, pname, row[column]));
			}
		}


		//--------------------------------------------------------------------------------------
		//	UPDATE
		//--------------------------------------------------------------------------------------

		public virtual void WriteUpdate(DataRow row)
		{
			builder.Append("UPDATE ");
			WriteIdentifier(row.Table.TableName);
			builder.Append(" SET");

			WriteAllColumnsAndParameters("", ", ");

			builder.Append(" WHERE");

			WriteOptimisticLockMatch();

			foreach(DataColumn column in row.Table.Columns)
			{
				string pname = ConvertToParameterName(column.ColumnName);
				parameters.Add(implFactory.CreateParameter(column, pname, row[column]));
				pname = ConvertToParameterName(column.ColumnName + "_ORIG");
				parameters.Add(implFactory.CreateParameter(column, pname, row[column, DataRowVersion.Original]));
			}
		}

		
		//--------------------------------------------------------------------------------------
		//	DELETE
		//--------------------------------------------------------------------------------------

		public virtual void WriteDelete(DataRow row)
		{
			builder.Append("DELETE FROM ");
			WriteIdentifier(row.Table.TableName);
			builder.Append(" WHERE");

			WriteOptimisticLockMatch();

			foreach(DataColumn column in row.Table.Columns)
			{
				string pname = ConvertToParameterName(column.ColumnName + "_ORIG");
				parameters.Add(implFactory.CreateParameter(column, pname, row[column, DataRowVersion.Original]));
			}

		}

		
		//--------------------------------------------------------------------------------------
		//	low-level writers
		//--------------------------------------------------------------------------------------

		protected void WriteColumns(ICollection columns)
		{
			bool first = true;

			foreach(DataColumn c in columns)
			{
				if(first == false)
					builder.Append(", ");
				WriteIdentifier(c.ColumnName);
				first = false;
			}
		}

		protected void WriteParameters(ICollection columns)
		{
			bool first = true;

			foreach(DataColumn c in columns)
			{
				if(first == false)
					builder.Append(", ");
				builder.Append(ConvertToParameterName(c.ColumnName));
				first = false;
			}
		}

		
		protected void WriteAllColumnsAndParameters(string suffix, string separator)
		{
			bool firstColumn = true;

			for(int i = 0; i < table.Columns.Count; i++)
			{
				// Workaround for update with autoincrement bug
				if(table.Columns[i].AutoIncrement)
					continue;

				if(firstColumn == false)
					builder.Append(separator);
				firstColumn = false;

				builder.Append(" ");
				WriteIdentifier(table.Columns[i].ColumnName);
				builder.Append(" = ");
				builder.Append(ConvertToParameterName(table.Columns[i].ColumnName + suffix));
			}
		}


		protected virtual void WriteOptimisticLockMatch()
		{
			for(int i = 0; i < table.Columns.Count; i++)
			{
				DataColumn column = table.Columns[i];

				if(i > 0)
					builder.Append(" AND");
				builder.Append(" ((");
				WriteIdentifier(column.ColumnName);
				if(column.ExtendedProperties.ContainsKey("LockStrategy") == false)
					builder.Append(" = ");
				else if((String)column.ExtendedProperties["LockStrategy"] == "LIKE")
					builder.Append(" LIKE ");
				else
					throw new ArgumentException("Invalid locking strategy; found " + (String)column.ExtendedProperties["LockStrategy"]);
				builder.Append(ConvertToParameterName(column.ColumnName + "_ORIG"));
				builder.Append(" ) OR (");
				builder.Append("COALESCE(");
				WriteIdentifier(column.ColumnName);
				builder.Append(", ");
				builder.Append(ConvertToParameterName(column.ColumnName + "_ORIG"));
				builder.Append(") IS NULL))");
			}
		}


		protected void WriteIdentifier(string identifier)
		{
			if(UsesDelimitedIdentifiers == false)
			{
				builder.Append(identifier);
			}
			else
			{
				string entry = (string)delimitedIdentifiers[identifier];
				if(entry == null) 
				{
					entry = ConvertToDelimitedIdentifier(identifier);
					delimitedIdentifiers.Add(identifier, entry);
				} 
				builder.Append(entry);
			}
		}


		protected virtual string ConvertToParameterName(string columnName)
		{
			return columnName;
		}


		protected virtual string ConvertToDelimitedIdentifier(string identifier)
		{
			StringBuilder newEntry = new StringBuilder();
			newEntry.Append("\"");
			newEntry.Append(identifier);
			newEntry.Append("\"");
			return newEntry.ToString();
		}

	}
}
