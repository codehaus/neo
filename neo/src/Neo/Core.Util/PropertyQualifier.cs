using System;
using System.Reflection;


namespace Neo.Core.Util
{

	public sealed class PropertyQualifier : Qualifier, IObjectQualifier
	{
		//--------------------------------------------------------------------------------------
		//	Fields and Constructor
		//--------------------------------------------------------------------------------------

		private string			prop;
		private IPredicate		predicate;
		private Type			lastType;	// cache
		private PropertyInfo	propInfo;	// cache

		public PropertyQualifier(string propName, IPredicate aPredicate) : base()
		{
			prop = propName;
			predicate = aPredicate;
		}


		//--------------------------------------------------------------------------------------
		//	Public properties
		//--------------------------------------------------------------------------------------

		public string Property
		{
			get { return prop; }
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
			return String.Format("{0} {1}", Property, predicate.ToString());
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
			object objValue = ObjectHelper.GetProperty(anObject, prop, ref lastType, ref propInfo);
			return predicate.IsTrueForValue(objValue, null);
		}

	}
}
