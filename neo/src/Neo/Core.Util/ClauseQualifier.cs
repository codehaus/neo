using System;
using System.Collections;
using System.Text;
using Neo.Core;


namespace Neo.Core.Util
{
	
	public abstract class ClauseQualifier : Qualifier
	{
		//--------------------------------------------------------------------------------------
		//	Fields and Constructor
		//--------------------------------------------------------------------------------------

		private Qualifier[]	qualifiers;


		public ClauseQualifier(params Qualifier[] someQualifiers) : base()
		{
			qualifiers = someQualifiers;
		}

		public ClauseQualifier(ArrayList someQualifiers) : base()
		{
			qualifiers = (Qualifier[])someQualifiers.ToArray(typeof(Qualifier));
		}


		//--------------------------------------------------------------------------------------
		//	Public properties
		//--------------------------------------------------------------------------------------

		public Qualifier[] Qualifiers
		{
			get { return qualifiers; }
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
					builder.Append(" " + OperatorString + " ");
				builder.Append(qualifiers[i]);
			}
			builder.Append(" ");
			return builder.ToString();
		}


		protected abstract string OperatorString
		{
			get;
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
			
			return (ClauseQualifier)Activator.CreateInstance(this.GetType(), newQualifiers);
		}

	}
}
