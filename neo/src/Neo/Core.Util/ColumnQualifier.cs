using System;
using System.Data;


namespace Neo.Core.Util
{

	public sealed class ColumnQualifier : ComparisonQualifier
	{

		//--------------------------------------------------------------------------------------
		//	Fields and Constructor
		//--------------------------------------------------------------------------------------

		private string column;


		public ColumnQualifier(string aColumn, QualifierOperator theOp, object aValue) : base()
		{
			column = aColumn;
			op = theOp;
			compVal = (aValue == null) ? DBNull.Value : aValue;
		}


		public ColumnQualifier(PropertyQualifier propQualifier, IEntityMap emap) : base()
		{
			IEntityObject	eo;
			IEntityMap		otherEmap;
			DataRelation	rel;

			if((eo = propQualifier.Value as IEntityObject) != null)
			{
				otherEmap = eo.Context.EntityMapFactory.GetMap(eo.GetType());
				if((rel = eo.Context.DataSet.Relations[otherEmap.TableName + "." + emap.TableName]) == null)
					throw new NeoException("Can't to convert PropertyQualifier to ColumnQualifier; did not find relation " + otherEmap.TableName + "." + emap.TableName);
				column = rel.ChildColumns[0].ColumnName;
				compVal = eo.Row[rel.ParentColumns[0]];
				op = propQualifier.Operator;
			}
			else
			{
				column = emap.GetColumnForAttribute(propQualifier.Property);
				compVal = propQualifier.Value;
				op = propQualifier.Operator;
			}
		}


		//--------------------------------------------------------------------------------------
		//	Public properties
		//--------------------------------------------------------------------------------------

		public string Column
		{
			get { return column; }
		}


		//--------------------------------------------------------------------------------------
		//	ToString() override
		//--------------------------------------------------------------------------------------

		public override string ToString()
		{
			return String.Format("row[{0}] {1} {2}", Column, OperatorForToString, Value);
		}

		
		//--------------------------------------------------------------------------------------
		//	Evaluation
		//--------------------------------------------------------------------------------------

		public override bool EvaluateWithObject(IEntityObject anObject)
		{
			object	objVal = anObject.Row[column];
			return Compare(objVal, DBNull.Value);
		}
	}

}
