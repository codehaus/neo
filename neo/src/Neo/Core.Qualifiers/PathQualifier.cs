using System;
using System.Reflection;
using System.Collections;
using Neo.Core.Util;

namespace Neo.Core.Qualifiers
{
	public sealed class PathQualifier : Qualifier, IObjectQualifier
	{
		//--------------------------------------------------------------------------------------
		//	Fields and Constructor
		//--------------------------------------------------------------------------------------

		private string[]	pathElements;
		private Qualifier	qualifier;

		public PathQualifier(string[] somePathElement, Qualifier aQualifier)
		{
			pathElements = somePathElement;
			qualifier = aQualifier;
		}


		public PathQualifier(string aPath, Qualifier aQualifier)
		{
			pathElements = aPath.Split('.');
			qualifier = aQualifier;
		}


		//--------------------------------------------------------------------------------------
		//	Public properties
		//--------------------------------------------------------------------------------------

		public string Path
		{
			get { return String.Join(".", pathElements); }
		}

		
		public string[] PathElements
		{
			get { return pathElements; }
		}


		public Qualifier Qualifier
		{
			get { return qualifier; }
		}

	
		//--------------------------------------------------------------------------------------
		//	ToString() override
		//--------------------------------------------------------------------------------------

		public override string ToString()
		{
			return String.Format("{0}.{1}", this.Path, this.Qualifier);
		}

		
		//--------------------------------------------------------------------------------------
		//	Evaluation
		//--------------------------------------------------------------------------------------

		public override bool EvaluateWithObject(IEntityObject anObject)
		{
			return EvaluateWithObject(anObject, 0);
		}


		private bool EvaluateWithObject(IEntityObject anObject, int index)
		{
			Type			objectType;
			PropertyInfo	propInfo;
			FieldInfo		fieldInfo;
			string			member;
			object			target;

			if(index >= pathElements.Length)
			{
				return qualifier.EvaluateWithObject(anObject);
			}

			objectType = anObject.GetType();
			member = pathElements[index];
			if((propInfo = objectType.GetProperty(member)) != null)
				target = propInfo.GetValue(anObject, null);
			else if((fieldInfo = objectType.GetField(member)) != null)
				target = fieldInfo.GetValue(anObject);
			else
				throw new InvalidPropertyException(String.Format("{0} is not a valid property/field for class {1}", member, objectType), null);
	
			if(target == null)
			{
				return false;
			}
			if(target is IEntityObject)
			{
				return EvaluateWithObject((IEntityObject)target, index + 1);
			}
			else if(target is ICollection)
			{
				foreach(IEntityObject eo in (ICollection)target)
				{
					if(EvaluateWithObject(eo, index + 1) == true)
						return true;
				}
				return false;
			}
			else
			{
				throw new InvalidPropertyException(String.Format("{0} returned an invalid object; only IEntityObject and ICollection are allowed", member), null);
			}
		}


		public bool EvaluateWithObject(object anObject)
		{
			IObjectQualifier objQualifier = qualifier as IObjectQualifier;

			if(objQualifier == null)
				throw new InvalidOperationException("Qualifier does not support generic object evaluation.");

			foreach(string prop in pathElements)
				anObject = ObjectHelper.GetProperty(anObject, prop);

			return objQualifier.EvaluateWithObject(anObject);
		}


		//--------------------------------------------------------------------------------------
		//	Visitor
		//--------------------------------------------------------------------------------------

		public override object AcceptVisitor(IQualifierVisitor v)
		{
			return v.VisitPathQualifier(this);
		}

	}
}
