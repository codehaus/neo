using System;
using System.Collections;
using Neo.Core.Util;

namespace Neo.Core
{

	public abstract class Qualifier
	{
		//--------------------------------------------------------------------------------------
		//	Static Helper
		//--------------------------------------------------------------------------------------

		public static Qualifier Format(string qformat)
		{
			return new QualifierParser(qformat, new object[0]).GetQualifier();
		}

		
		public static Qualifier Format(string qformat, params object[] values)
		{
			return new QualifierParser(qformat, values).GetQualifier();
		}

		
		public static Qualifier FromPropertyDictionary(IDictionary queryValues)
		{
			ArrayList	qualifiers;

			if(queryValues.Count == 1)
			{
				IDictionaryEnumerator enumerator = queryValues.GetEnumerator();
				enumerator.MoveNext();
				DictionaryEntry e = enumerator.Entry;
				return new PropertyQualifier((string)e.Key, QualifierOperator.Equal, e.Value);
			}
			else
			{
				qualifiers = new ArrayList(queryValues.Count);
				foreach(DictionaryEntry e in queryValues)
					qualifiers.Add(new PropertyQualifier((string)e.Key, QualifierOperator.Equal, e.Value));
				return new ClauseQualifier(QualifierConjunctor.And, qualifiers);
			}
		}


		//--------------------------------------------------------------------------------------
		//	Constructor
		//--------------------------------------------------------------------------------------

		internal Qualifier()
		{
		}


		//--------------------------------------------------------------------------------------
		//	Qualifier evaluation
		//--------------------------------------------------------------------------------------

		public abstract bool EvaluateWithObject(IEntityObject anObject);
	


	}
}
