using System;
using System.Collections;
using Neo.Core.Util;

namespace Neo.Core
{
	/// <summary>
	/// Qualifiers define criteria for selections.
	/// </summary>
	/// <remarks>They are normally constructed using formats.
	/// <code>
	/// q = Qualifier.Format("name = {0}", input);
	/// </code>
	/// Formats can use inlined values and comprise multiple clauses:
	/// <code>
	/// q = Qualifier.Format("name = &apos;Haruki&apos;");
	/// q = Qualifier.Format("name = {0} and locked = false", input);
	/// </code>
	/// Formats provide a shortcut for simple matches:
	/// <code>q = new Qualifier.Format("Name", input);</code>
	/// Qualifiers can evaluate whether an object matches their criteria:
	/// <code>if (q.EvaluateWithObject(anAuthor))
	///     doSomething(anAuthor);</code>
	/// </remarks>
	public abstract class Qualifier
	{
		//--------------------------------------------------------------------------------------
		//	Static Helper
		//--------------------------------------------------------------------------------------

		/// <summary>
		/// Returns <c>Qualifier</c> to test IEntityObjects against the qualifier text
		/// </summary>
		/// <param name="qformat">string version of test</param>
		/// <returns><c>Qualifier</c> object to test other objects</returns>
		public static Qualifier Format(string qformat)
		{
			return new QualifierParser(qformat, new object[0]).GetQualifier();
		}

		
		/// <summary>
		/// Returns <c>Qualifier</c> to test IEntityObjects against the qualifier text and supplied values
		/// </summary>
		/// <param name="qformat">string version of test</param>
		/// <param name="values">additional information for test</param>
		/// <returns><c>Qualifier</c> object to test other objects</returns>
		public static Qualifier Format(string qformat, params object[] values)
		{
			return new QualifierParser(qformat, values).GetQualifier();
		}

		
		/// <summary>
		/// Returns <c>Qualifier</c> to test IEntityObjects against the supplied property/value
		/// </summary>
		/// <param name="queryValues">property/value pairs to be compared</param>
		/// <returns><c>Qualifier</c> object to test other objects</returns>
		public static Qualifier FromPropertyDictionary(IDictionary queryValues)
		{
			ArrayList	qualifiers;

			if(queryValues.Count == 1)
			{
				IDictionaryEnumerator enumerator = queryValues.GetEnumerator();
				enumerator.MoveNext();
				DictionaryEntry e = enumerator.Entry;
				return new PropertyQualifier((string)e.Key, new EqualsPredicate(e.Value));
			}
			else
			{
				qualifiers = new ArrayList(queryValues.Count);
				foreach(DictionaryEntry e in queryValues)
					qualifiers.Add(new PropertyQualifier((string)e.Key, new EqualsPredicate(e.Value)));
				return new AndQualifier(qualifiers);
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

		/// <summary>
		/// Returns whether the supplied IEntityObject matches the criteria of this qualifier.
		/// </summary>
		public abstract bool EvaluateWithObject(IEntityObject anObject);
	


	}
}
