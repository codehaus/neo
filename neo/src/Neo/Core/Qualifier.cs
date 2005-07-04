using System;
using System.Collections;
using Neo.Core.Parser;
using Neo.Core.Qualifiers;


namespace Neo.Core
{
	/// <summary>
	/// Qualifiers define criteria for object selections.
	/// </summary>
	/// <remarks>There is a class hierarchy of different qualifiers to express different types
	/// of qualification but normally qualifiers are constructed using formats:
	/// <code>
	/// q = Qualifier.Format("Name = {0}", input);
	/// </code>
	/// Formats can use inlined values and comprise multiple clauses:
	/// <code>
	/// q = Qualifier.Format("Name = &apos;Haruki&apos;");
	/// q = Qualifier.Format("Name = {0} and locked = false", input);
	/// </code>
	/// Formats can contain brackets and paths spanning multiple entities:
	/// <code>
	/// q = Qualifier.Format("Publisher.Name = {0}", pubname);
	/// q = Qualifier.Format("TitleAuthor.Title.(TheTitle like 'A%' or TheTitle like 'B%')");
	/// </code>
	/// Formats provide a shortcut for simple matches:
	/// <code>q = new Qualifier.Format("Name", input);</code>
	/// Qualifiers are used in conjunction with <c>FetchSpecification</c> or can be used to 
	/// evaluate whether an object matches their criteria:
	/// <code>if(q.EvaluateWithObject(anAuthor))
	///     doSomething(anAuthor);</code>
	/// </remarks>
	public abstract class Qualifier
	{
		//--------------------------------------------------------------------------------------
		//	Static Helper
		//--------------------------------------------------------------------------------------

		/// <summary>
		/// Creates a <c>Qualifier</c> for the given query.
		/// </summary>
		/// <param name="qformat">string version of test</param>
		/// <returns><c>Qualifier</c> object for the query</returns>
		public static Qualifier Format(string qformat)
		{
			return new QualifierParser(qformat, new object[0]).GetQualifier();
		}

		
		/// <summary>
		/// Creates a <c>Qualifier</c> for the given query.
		/// </summary>
		/// <param name="formatString">string version of the query</param>
		/// <param name="values">positional parameters for the query string</param>
		/// <returns><c>Qualifier</c> object for the query</returns>
		public static Qualifier Format(string formatString, params object[] values)
		{
			return new QualifierParser(formatString, values).GetQualifier();
		}

		
		/// <summary>
		/// Creates a <c>Qualifier</c> which tests for the supplied property/value pairs.
		/// </summary>
		/// <param name="queryValues">property/value pairs to be compared</param>
		/// <returns><c>Qualifier</c> object for the query</returns>
		public static Qualifier FromPropertyDictionary(IDictionary queryValues)
		{
			ArrayList	qualifiers;

			if(queryValues.Count == 0)
			{
				return null;
			}
			else if(queryValues.Count == 1)
			{
				IDictionaryEnumerator enumerator = queryValues.GetEnumerator();
				enumerator.MoveNext();
				DictionaryEntry e = enumerator.Entry;
				return Qualifier.ForKeyValuePair((string)e.Key, e.Value);
			}
			else
			{
				qualifiers = new ArrayList(queryValues.Count);
				foreach(DictionaryEntry e in queryValues)
					qualifiers.Add(Qualifier.ForKeyValuePair((string)e.Key, e.Value));
				return new AndQualifier(qualifiers);
			}
		}


		protected static PropertyQualifier ForKeyValuePair(string aKey, object aValue)
		{
		    String valueAsString = aValue as string;
			if((valueAsString != null) && (valueAsString.IndexOf("%") >= 0))
				return new PropertyQualifier(aKey, new LikePredicate(valueAsString));
			return new PropertyQualifier(aKey, new EqualsPredicate(aValue));

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
	


		//--------------------------------------------------------------------------------------
		//	Visitor
		//--------------------------------------------------------------------------------------

		/// <summary>
		/// Begins traversal of the qualifier. The visitor passed in will receive methods from
		/// the <c>IQualifierVisitor</c> interface as subqualifiers are reached during 
		/// traversal.
		/// </summary>
		/// <remarks>
		/// This method is not normally used from applications.
		/// </remarks>
		/// <param name="visitor">The visitor that receives the call-back methods</param>
		/// <returns>The object returned from the vistor for this qualifier</returns>
		public abstract object AcceptVisitor(IQualifierVisitor visitor);

	}
}
