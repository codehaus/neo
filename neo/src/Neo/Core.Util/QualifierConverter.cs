using System;
using System.Data;
using Neo.Core.Qualifiers;


namespace Neo.Core.Util
{

	public class QualifierConverter : IQualifierVisitor
	{
		//--------------------------------------------------------------------------------------
		//	Fields and Constructor
		//--------------------------------------------------------------------------------------

		private IEntityMap	emap;

		public QualifierConverter(IEntityMap anEmap)
		{
			emap = anEmap;
		}

		
		//--------------------------------------------------------------------------------------
		//	Simple conversions
		//--------------------------------------------------------------------------------------

		public ColumnQualifier ConvertPropertyQualifier(PropertyQualifier propQualifier)
		{
			IEntityObject	eo;
			IEntityMap		otherEmap;
		    DataRelation	rel;
			IPredicate		predicate;
			string			column;
			object			compVal;

			if((eo = propQualifier.Predicate.Value as IEntityObject) != null)
			{
				otherEmap = eo.Context.EntityMapFactory.GetMap(eo.GetType());
				if((rel = eo.Context.DataSet.Relations[otherEmap.TableName + "." + emap.TableName]) == null)
					throw new NeoException("Can't to convert PropertyQualifier to ColumnQualifier; did not find relation " + otherEmap.TableName + "." + emap.TableName);
				column = rel.ChildColumns[0].ColumnName;
				compVal = eo.Row[rel.ParentColumns[0]];
			}
			else
			{
				column = emap.GetColumnForAttribute(propQualifier.Property);
				compVal = propQualifier.Predicate.Value;
			}
			if(compVal == null)
				compVal = DBNull.Value;
			predicate = (IPredicate)Activator.CreateInstance(propQualifier.Predicate.GetType(), new object[] { compVal });

			return new ColumnQualifier(column, predicate);
		}

		
		//--------------------------------------------------------------------------------------
		//	Recursive conversion
		//--------------------------------------------------------------------------------------

		public Qualifier ConvertPropertyQualifiers(Qualifier aQualifier)
		{
			return (Qualifier)aQualifier.AcceptVisitor(this);
		}


		//--------------------------------------------------------------------------------------
		//	IQualifierVisitor impl
		//--------------------------------------------------------------------------------------

		public object VisitColumnQualifier(ColumnQualifier columnQualifier)
		{
			return columnQualifier;
		}


		public object VisitPropertyQualifier(PropertyQualifier propQualifier)
		{
			return ConvertPropertyQualifier(propQualifier);
		}


		public object VisitPathQualifier(PathQualifier pathQualifier)
		{
			return pathQualifier;
		}


		public object VisitOrQualifier(OrQualifier q)
		{
			return new OrQualifier(ConvertQualfiers(q.Qualifiers));
		}


		public object VisitAndQualifier(AndQualifier q)
		{
			return new AndQualifier(ConvertQualfiers(q.Qualifiers));
		}


		private Qualifier[] ConvertQualfiers(Qualifier[] qualifiers)
		{
			Qualifier[] convertedQualifiers = new Qualifier[qualifiers.Length];
			for(int i = 0; i < qualifiers.Length; i++)
				convertedQualifiers[i] = (Qualifier)qualifiers[i].AcceptVisitor(this);
			return convertedQualifiers;
		}


	}
}
