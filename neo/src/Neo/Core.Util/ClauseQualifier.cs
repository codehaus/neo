using System;
using System.Collections;
using System.Text;
using Neo.Core;


namespace Neo.Core.Util
{
	public enum QualifierConjunctor
	{
		And,
		Or
	}

	
	public sealed class ClauseQualifier : Qualifier, IObjectQualifier
	{
		//--------------------------------------------------------------------------------------
		//	Fields and Constructor
		//--------------------------------------------------------------------------------------

		private QualifierConjunctor	conjunctor;
		private Qualifier[]			qualifiers;


		public ClauseQualifier(QualifierConjunctor aConjunctor, params Qualifier[] someQualifiers) : base()
		{
			conjunctor = aConjunctor;
			qualifiers = someQualifiers;
		}

		public ClauseQualifier(QualifierConjunctor aConjunctor, ArrayList someQualifiers) : base()
		{
			conjunctor = aConjunctor;
			qualifiers = (Qualifier[])someQualifiers.ToArray(typeof(Qualifier));
		}


		//--------------------------------------------------------------------------------------
		//	Public properties
		//--------------------------------------------------------------------------------------

		public Qualifier[] Qualifiers
		{
			get { return qualifiers; }
		}

		public QualifierConjunctor Conjunctor
		{
			get { return conjunctor; }
		}


		//--------------------------------------------------------------------------------------
		//	Public properties
		//--------------------------------------------------------------------------------------

		public void AddToQualifiers(Qualifier aQualifier)
		{
			Qualifier[] oldQualifiers = qualifiers;
			qualifiers = new Qualifier[oldQualifiers.Length + 1];
			oldQualifiers.CopyTo(qualifiers, 0);
			qualifiers[oldQualifiers.Length] = aQualifier;
		}


		//--------------------------------------------------------------------------------------
		//	ToString() override
		//--------------------------------------------------------------------------------------

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("(");
			for(int i = 0; i < qualifiers.Length; i++)
			{
				if(i > 0)
					builder.Append(" " + ((Conjunctor == QualifierConjunctor.Or) ? "or" : "and") + " ");
				builder.Append(qualifiers[i]);
			}
			builder.Append(" ");
			return builder.ToString();
		}

		
		//--------------------------------------------------------------------------------------
		//	Creating Column Qualifiers
		//--------------------------------------------------------------------------------------

		public ClauseQualifier GetWithColumnQualifiers(IEntityMap thisEmap)
		{
			Qualifier[]	newQualifiers;
			int			i;
			
			newQualifiers = new Qualifier[qualifiers.Length];
			i = 0;
			foreach(Qualifier q in qualifiers)
			{
				if(q is ClauseQualifier)
					newQualifiers[i++] = ((ClauseQualifier)q).GetWithColumnQualifiers(thisEmap);
				else if(q is PropertyQualifier)
					newQualifiers[i++] = new ColumnQualifier((PropertyQualifier)q, thisEmap);
				else
					newQualifiers[i++] = q;
			}
			
			return new ClauseQualifier(conjunctor, newQualifiers);
		}


		//--------------------------------------------------------------------------------------
		//	Evaluation
		//--------------------------------------------------------------------------------------

		public override bool EvaluateWithObject(IEntityObject anObject)
		{
			switch(conjunctor)
			{
				case QualifierConjunctor.And:
					foreach(Qualifier q in qualifiers)
					{
						if(q.EvaluateWithObject(anObject) == false)
							return false;
					}
					return true;

				case QualifierConjunctor.Or:
					foreach(Qualifier q in qualifiers)
					{
						if(q.EvaluateWithObject(anObject) == true)
							return true;
					}
					return false;

				default:
					throw new InvalidOperationException(String.Format("Invalid conjunctor for qualifier."));
			}
		}


		public bool EvaluateWithObject(object anObject)
		{
			switch(conjunctor)
			{
			case QualifierConjunctor.And:
				foreach(Qualifier q in qualifiers)
				{
					IObjectQualifier objQualifier = q as IObjectQualifier;
					if(objQualifier == null)
						throw new InvalidOperationException("Not all children support generic object evaluation.");
					if(objQualifier.EvaluateWithObject(anObject) == false)
						return false;
				}
				return true;

			case QualifierConjunctor.Or:
				foreach(Qualifier q in qualifiers)
				{
					IObjectQualifier objQualifier = q as IObjectQualifier;
					if(objQualifier == null)
						throw new InvalidOperationException("Not all children support generic object evaluation.");
					if(objQualifier.EvaluateWithObject(anObject) == true)
						return true;
				}
				return false;

			default:
				throw new InvalidOperationException(String.Format("Invalid conjunctor for qualifier."));
			}
		}
	}
}
