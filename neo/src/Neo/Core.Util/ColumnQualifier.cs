using System;
using System.Reflection;
using System.Data;


namespace Neo.Core.Util
{

	public sealed class ColumnQualifier : Qualifier
	{

		//--------------------------------------------------------------------------------------
		//	Fields and Constructor
		//--------------------------------------------------------------------------------------

		private string		column;
		private IPredicate	predicate;


		public ColumnQualifier(string aColumn, IPredicate aPredicate) : base()
		{
			column = aColumn;
			predicate = aPredicate;
			//predicate = (aValue == null) ? DBNull.Value : aValue;
		}


		public ColumnQualifier(PropertyQualifier propQualifier, IEntityMap emap) : base()
		{
			IEntityObject	eo;
			IEntityMap		otherEmap;
			DataRelation	rel;
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
		}


		//--------------------------------------------------------------------------------------
		//	Public properties
		//--------------------------------------------------------------------------------------

		public string Column
		{
			get { return column; }
		}

		public IPredicate Predicate
		{
			get { return predicate; }
		}


		//--------------------------------------------------------------------------------------
		//	ToString() override
		//--------------------------------------------------------------------------------------

		public override string ToString()
		{
			return String.Format("row[{0}] {1}", Column, predicate.ToString());
		}

		
		//--------------------------------------------------------------------------------------
		//	Evaluation
		//--------------------------------------------------------------------------------------

		public override bool EvaluateWithObject(IEntityObject anObject)
		{
			return predicate.IsTrueForValue(anObject.Row[column], DBNull.Value);
		}
	}

}
