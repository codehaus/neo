using System;
using System.Collections;
using System.Data;
using System.Text;
using Neo.Core;
using Neo.Core.Qualifiers;
using Neo.Core.Util;


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
				if(q.Predicate is NotEqualPredicate)
					builder.Append(" IS NOT NULL ");
				else if(q.Predicate is EqualsPredicate)
					builder.Append(" IS NULL ");
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
				else if(q.Predicate is CaseInsensitiveEqualsPredicate)
					builder.Append(" = ");
				else
					throw new ArgumentException("Invalid predicate in qualifier; found " + q.Predicate.GetType().FullName);
				
				// add the current parameter count as suffix to ensure names are unique
				DataColumn column = table.Columns[q.Column];
				string pname = ConvertToParameterName(column.ColumnName + parameters.Count.ToString());
				IDataParameter param = implFactory.CreateParameter(column, pname, q.Predicate.Value);
				builder.Append(pname);
				parameters.Add(param);
			}

			return null;
		}


		public object VisitPropertyQualifier(PropertyQualifier q)
		{
			ColumnQualifier	cq = new QualifierConverter(emap).ConvertToColumnQualifier(q);
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
			RelationInfo	relInfo;
			IEntityMap		newEmap;
			string			leftColumn, rightColumn;
			
			relInfo = emap.GetRelationInfo(q.PathElements[idx]);
			if(relInfo.ParentEntity == emap)
			{
				newEmap = relInfo.ChildEntity;
				leftColumn = relInfo.ParentKey;
				rightColumn = relInfo.ChildKey;
			}
			else
			{
				newEmap = relInfo.ParentEntity;
				leftColumn = relInfo.ChildKey;
				rightColumn = relInfo.ParentKey;
			}
			newEmap.UpdateSchemaInDataSet(table.DataSet, SchemaUpdate.Basic | SchemaUpdate.Relations);

			if(idx == q.PathElements.Length - 1)
			{
				if(WriteAbbreviatedPathEnd(table, q, newEmap, leftColumn, rightColumn))
					return;
			}

			WriteIdentifier(leftColumn);
			builder.Append(" IN ( SELECT ");
			if(q.Qualifier == null)
				builder.Append("DISTINCT ");
			WriteIdentifier(rightColumn);
			builder.Append(" FROM ");
			WriteIdentifier(newEmap.TableName);

			if(q.Qualifier != null)
			{
				builder.Append(" WHERE ");
				if(idx == q.PathElements.Length - 1)
					WritePathEndQualifier(table, newEmap, q.Qualifier);
				else
					WritePathQualifier(table.DataSet.Tables[newEmap.TableName], newEmap, q, idx + 1);
			}

			builder.Append(" )");
		}

		private bool WriteAbbreviatedPathEnd(DataTable table, PathQualifier q, IEntityMap newEmap, string leftColumn, string rightColumn)
		{
			string endQualifierColum = null;
			IPredicate endQualifierPredicate = null;
//			if(q.Qualifier is PropertyQualifier)
//			{
//				PropertyQualifier pq = (PropertyQualifier)q.Qualifier;
//				endQualifierColum = newEmap.GetColumnForAttribute(pq.Property);
//				endQualifierPredicate = pq.Predicate;
//			}
			if(q.Qualifier is ColumnQualifier)
			{
				ColumnQualifier cq = (ColumnQualifier)q.Qualifier;
				endQualifierColum = cq.Column;
				endQualifierPredicate = cq.Predicate;
			}
	
			if((endQualifierColum != rightColumn) || (endQualifierPredicate is EqualsPredicate == false))
				return false;

			WritePathEndQualifier(table, newEmap, new ColumnQualifier(leftColumn, endQualifierPredicate));
			return true;
		}

		private void WritePathEndQualifier(DataTable table, IEntityMap emap, Qualifier q)
		{
			GenericSql92Builder clone = (GenericSql92Builder)this.Clone();
			clone.table = table.DataSet.Tables[emap.TableName];
			clone.emap = emap;
			clone.WriteQualifier(q);
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

			WriteAllColumnsAndParameters("", ", ", row);

			builder.Append(" WHERE");

			WriteOptimisticLockMatch(row);
		}

		
		//--------------------------------------------------------------------------------------
		//	DELETE
		//--------------------------------------------------------------------------------------

		public virtual void WriteDelete(DataRow row)
		{
			builder.Append("DELETE FROM ");
			WriteIdentifier(row.Table.TableName);
			builder.Append(" WHERE");

			WriteOptimisticLockMatch(row);
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

		
		protected void WriteAllColumnsAndParameters(string suffix, string separator, DataRow row)
		{
			bool firstColumn = true;

			for(int i = 0; i < table.Columns.Count; i++)
			{
				DataColumn column = table.Columns[i];
				string paramName = ConvertToParameterName(column.ColumnName + suffix);
				
				// Workaround for update with autoincrement bug
				if(column.AutoIncrement)
					continue;

				if(IsUnchanged(column, row))
					continue;

				if(firstColumn == false)
					builder.Append(separator);
				firstColumn = false;

				builder.Append(" ");
				WriteIdentifier(column.ColumnName);
				builder.Append(" = ");
				builder.Append(paramName);

				parameters.Add(implFactory.CreateParameter(column, paramName, row[column]));
			}
		}

		protected virtual bool IsUnchanged(DataColumn column, DataRow row)
		{
			object originalValue = row[column, DataRowVersion.Original];
			object currentValue = row[column, DataRowVersion.Current];
			return originalValue.Equals(currentValue);
		}



		protected virtual void WriteOptimisticLockMatch(DataRow row)
		{
			bool isFirstCondition = true;
			for(int i = 0; i < table.Columns.Count; i++)
			{
				DataColumn column = table.Columns[i];
				string paramName = ConvertToParameterName(column.ColumnName + "_ORIG");

				object propValue = column.ExtendedProperties["LockingStrategy"];
				if(propValue == null)
					propValue = column.ExtendedProperties["LockStrategy"];
				String lockingStrategy = propValue as String;

				if((lockingStrategy != null) && (lockingStrategy.ToUpper() == "NONE"))
					continue;

				if(isFirstCondition == false)
					builder.Append(" AND");
				isFirstCondition = false;

				builder.Append(" ((");
				WriteIdentifier(column.ColumnName);
				if(lockingStrategy == null)
					builder.Append(" = ");
				else if(lockingStrategy.ToUpper() == "LIKE")
					builder.Append(" LIKE ");
				else
					throw new ArgumentException("Invalid locking strategy; found " + (String)column.ExtendedProperties["LockStrategy"]);
				builder.Append(paramName);
				builder.Append(" ) ");
				if(column.AllowDBNull == true)
				{
					builder.Append("OR (");
					builder.Append("COALESCE(");
					WriteIdentifier(column.ColumnName);
					builder.Append(", ");
					builder.Append(paramName);
					builder.Append(") IS NULL)");
				}
				builder.Append(")");
				parameters.Add(implFactory.CreateParameter(column, paramName, row[column, DataRowVersion.Original]));
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
