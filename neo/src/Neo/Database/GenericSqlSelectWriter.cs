using System;
using System.Data;
using System.Reflection;
using Neo.Core;
using Neo.Core.Qualifiers;
using Neo.Core.Util;
using Neo.Framework;


namespace Neo.Database
{

	public class GenericSqlSelectWriter : GenericSqlWriter, IQualifierVisitor, ICloneable
	{
		//--------------------------------------------------------------------------------------
		//	Fields and Constructor
		//--------------------------------------------------------------------------------------

		protected IEntityMap		emap;

		public GenericSqlSelectWriter(DataTable aTable, IParameterFactory aParamFactory) : base(aTable, aParamFactory)
		{
		}


		//--------------------------------------------------------------------------------------
		//	ICloneable impl
		//--------------------------------------------------------------------------------------

		public object Clone()
		{
			GenericSqlSelectWriter clone = new GenericSqlSelectWriter(table, parameterFactory);
			clone.emap = this.emap;
			clone.builder = this.builder;
			clone.parameters = this.parameters;
			return clone;
		}

		
		//--------------------------------------------------------------------------------------
		//  SELECT
		//--------------------------------------------------------------------------------------

		public void WriteSelect(IFetchSpecification fetchSpec)
		{
			emap = fetchSpec.EntityMap;

			builder.Append("SELECT ");

			if(fetchSpec.FetchLimit != -1)
			{
				builder.Append("TOP ");
				builder.Append(fetchSpec.FetchLimit);
				builder.Append(" ");
			}

			WriteColumns(table.Columns);

			builder.Append(" FROM ");
			builder.Append(table.TableName);

			if(fetchSpec.Qualifier != null)
			{
				builder.Append(" WHERE ");
				fetchSpec.Qualifier.AcceptVisitor(this);
			}

			if(fetchSpec.SortOrderings != null)
			{
				builder.Append(" ORDER BY ");
				WriteSortOrderings(fetchSpec.SortOrderings);
			}
		}


	    //--------------------------------------------------------------------------------------
		//	IQualifierVisitor impl
		//--------------------------------------------------------------------------------------
		
		public object VisitColumnQualifier(ColumnQualifier q)
		{
			builder.Append(q.Column);

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
				if(q.Predicate is EqualsPredicate)
					builder.Append("=");
				else if(q.Predicate is NotEqualPredicate)
					builder.Append("<>");
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
				IDataParameter param = parameterFactory.CreateParameter(table.Columns[q.Column], parameters.Count.ToString(), q.Predicate.Value);
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

			builder.Append((emap == rightEmap) ? rel.ChildColumns[0].ColumnName : rel.ParentColumns[0].ColumnName);
			builder.Append(" IN ( SELECT ");
			builder.Append((emap == rightEmap) ? rel.ParentColumns[0].ColumnName : rel.ChildColumns[0].ColumnName);
			builder.Append(" FROM ");
			builder.Append(newEmap.TableName);
			builder.Append(" WHERE ");

			newEmap.UpdateSchemaInDataSet(table.DataSet, SchemaUpdate.Basic | SchemaUpdate.Relations);
			if(idx < q.PathElements.Length - 1)
			{
				WritePathQualifier(table.DataSet.Tables[newEmap.TableName], newEmap, q, idx + 1);
			}
			else
			{
				GenericSqlSelectWriter clone = (GenericSqlSelectWriter)this.Clone();
				clone.table = table.DataSet.Tables[newEmap.TableName];
				clone.emap = newEmap;
				q.Qualifier.AcceptVisitor(clone);
			}

			builder.Append(" )");
		}



		//--------------------------------------------------------------------------------------
		//	Writing sort orderings
		//--------------------------------------------------------------------------------------

		protected void WriteSortOrderings(PropertyComparer[] orderings)
		{
			bool first = true;
			foreach(PropertyComparer c in orderings)
			{
				if(first == false)
					builder.Append(", ");
				builder.Append(emap.GetColumnForAttribute(c.Property));
				if(c.SortDirection == SortDirection.Descending)
					builder.Append(" DESC");
				first = false;
			}
		}


	}
}
