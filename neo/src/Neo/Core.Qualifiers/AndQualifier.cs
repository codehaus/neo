using System;
using System.Collections;


namespace Neo.Core.Qualifiers
{

	public sealed class AndQualifier : ClauseQualifier, IObjectQualifier
	{
		//--------------------------------------------------------------------------------------
		//	Fields and Constructor
		//--------------------------------------------------------------------------------------

		public AndQualifier(params Qualifier[] someQualifiers) : base(someQualifiers)
		{
		}

		public AndQualifier(ArrayList someQualifiers) : base()
		{
		}

		
		//--------------------------------------------------------------------------------------
		//	ToString() override
		//--------------------------------------------------------------------------------------

		protected override string OperatorString
		{
			get { return "and"; }
		}


		//--------------------------------------------------------------------------------------
		//	Evaluation
		//--------------------------------------------------------------------------------------

		public override bool EvaluateWithObject(IEntityObject anObject)
		{
			foreach(Qualifier q in Qualifiers)
			{
				if(q.EvaluateWithObject(anObject) == false)
					return false;
			}
			return true;
		}


		public bool EvaluateWithObject(object anObject)
		{
			foreach(Qualifier q in Qualifiers)
			{
				IObjectQualifier objQualifier = q as IObjectQualifier;
				if(objQualifier == null)
					throw new InvalidOperationException("Not all children support generic object evaluation.");
				if(objQualifier.EvaluateWithObject(anObject) == false)
					return false;
			}
			return true;
		}


		//--------------------------------------------------------------------------------------
		//	Visitor
		//--------------------------------------------------------------------------------------

		public override object AcceptVisitor(IQualifierVisitor v)
		{
			return v.VisitAndQualifier(this);
		}


	}
}
