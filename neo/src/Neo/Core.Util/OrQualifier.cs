using System;
using System.Collections;


namespace Neo.Core.Util
{

	public sealed class OrQualifier : ClauseQualifier, IObjectQualifier
	{
		//--------------------------------------------------------------------------------------
		//	Fields and Constructor
		//--------------------------------------------------------------------------------------

		public OrQualifier(params Qualifier[] someQualifiers) : base(someQualifiers)
		{
		}

		public OrQualifier(ArrayList someQualifiers) : base()
		{
		}

		
		//--------------------------------------------------------------------------------------
		//	ToString() override
		//--------------------------------------------------------------------------------------

		protected override string OperatorString
		{
			get { return "or"; }
		}


		//--------------------------------------------------------------------------------------
		//	Evaluation
		//--------------------------------------------------------------------------------------

		public override bool EvaluateWithObject(IEntityObject anObject)
		{
			foreach(Qualifier q in Qualifiers)
			{
				if(q.EvaluateWithObject(anObject) == true)
					return true;
			}
			return false;
		}


		public bool EvaluateWithObject(object anObject)
		{
			foreach(Qualifier q in Qualifiers)
			{
				IObjectQualifier objQualifier = q as IObjectQualifier;
				if(objQualifier == null)
					throw new InvalidOperationException("Not all children support generic object evaluation.");
				if(objQualifier.EvaluateWithObject(anObject) == true)
					return true;
			}
			return false;
		}


	}
}
