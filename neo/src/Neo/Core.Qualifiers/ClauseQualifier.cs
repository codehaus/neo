using System.Collections;
using System.Text;


namespace Neo.Core.Qualifiers
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
		//	Public accessors
		//--------------------------------------------------------------------------------------

		public Qualifier[] Qualifiers
		{
			get 
			{ 
				return qualifiers; 
			}

		}

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
			builder.Append(")");
			return builder.ToString();
		}


		protected abstract string OperatorString
		{
			get;
		}


	}
}
