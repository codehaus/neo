using System;
using System.Reflection;


namespace Neo.Core.Util
{

	public sealed class PropertyQualifier : ComparisonQualifier, IObjectQualifier
	{
		//--------------------------------------------------------------------------------------
		//	Fields and Constructor
		//--------------------------------------------------------------------------------------

		private string			prop;
		private Type			lastType;
		private PropertyInfo	propInfo;

		public PropertyQualifier(string propName, QualifierOperator theOp, object aValue) : base()
		{
			if((op != QualifierOperator.Equal) && (op != QualifierOperator.NotEqual) && (aValue is IComparable == false))
				throw new ArgumentException("Comparison value must implement IComparable to be used with relational operators such as less than and greater than.");
	
			prop = propName;
			op = theOp;
			compVal = aValue;
		}

		//--------------------------------------------------------------------------------------
		//	Public properties
		//--------------------------------------------------------------------------------------

		public string Property
		{
			get { return prop; }
		}

	
		//--------------------------------------------------------------------------------------
		//	ToString() override
		//--------------------------------------------------------------------------------------

		//--------------------------------------------------------------------------------------
		//	ToString() override
		//--------------------------------------------------------------------------------------

		public override string ToString()
		{
			return String.Format("{0} {1} {2}", Property, OperatorForToString, Value);
		}

		
		//--------------------------------------------------------------------------------------
		//	Evaluation
		//--------------------------------------------------------------------------------------

		public override bool EvaluateWithObject(IEntityObject anObject)
		{
			return EvaluateWithObject(anObject);
		}


		public bool EvaluateWithObject(object anObject)
		{
			object objVal = ObjectHelper.GetProperty(anObject, prop, ref lastType, ref propInfo);
			return Compare(objVal, null);
		}

	}
}
