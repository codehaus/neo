using System;
using System.Globalization;
using System.Reflection;
using log4net;


namespace Neo.Core.Util
{
	public class ObjectHelper
	{
		protected static ILog logger;

		static ObjectHelper()
		{
			if(logger == null)
				logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
		}


		public static void SetProperty(object obj, string propName, object propValue)
		{
			try
			{
				BindingFlags bflags = BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance;
				obj.GetType().InvokeMember(propName, bflags, null, obj, new object[] { propValue }, CultureInfo.InvariantCulture);
			}
			catch(MissingMethodException e)
			{
				BindingFlags bflags = BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance;
				// maybe it was a mismatch on signature, i.e. right name but wrong type
				MemberInfo[] mi = obj.GetType().GetMember(propName, bflags);
				if(mi.Length == 0)
					throw new InvalidPropertyException(String.Format("{0} is not a valid property for class {1}", propName, obj.GetType()), e);
				// it was, pick the first property and try to convert value to its type
				if(logger.IsDebugEnabled)
					logger.Debug(String.Format("Type of {0}.{1} is {2}, type of value is {3}. Trying to convert.", 
						obj.GetType(), propName, propValue.GetType(), ((PropertyInfo)mi[0]).PropertyType));
				propValue = Convert.ChangeType(propValue, ((PropertyInfo)mi[0]).PropertyType);
				obj.GetType().InvokeMember(propName, bflags, null, obj, new object[] { propValue }, CultureInfo.InvariantCulture);
			}
			catch(TargetInvocationException e)
			{
				// probably a Neo bug, if this called.
			    logger.Error("TargetInvocationException. Cause: " + e.InnerException);
			}
		}


		public static object GetProperty(object obj, string propName)
		{
			try
			{
				BindingFlags bflags = BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance;
				return obj.GetType().InvokeMember(propName, bflags, null, obj, null, CultureInfo.InvariantCulture);
			}
			catch(MissingMethodException e)
			{
				throw new InvalidPropertyException(String.Format("{0} is not a valid property for class {1}", propName, obj.GetType()), e);
			}
			catch(TargetInvocationException e)
			{
				if(e.InnerException != null)
				{
					if(e.InnerException is InvalidDbNullException)
						return null;
					else
						throw e.InnerException;
				}
				throw e;
			}
		}


		public static object GetProperty(object anObject, string prop, ref Type lastType, ref PropertyInfo propInfo)
		{
			Type objType = anObject.GetType();
			if(objType != lastType)
			{
				if((propInfo = objType.GetProperty(prop)) == null)
					throw new InvalidPropertyException(String.Format("{0} is not a valid property for class {1}", prop, objType), null);
				lastType = objType;
			}
			try
			{
				return propInfo.GetValue(anObject, null);
			}
			catch(TargetInvocationException e)
			{
				if(e.InnerException == null)
					throw e;

				if(e.InnerException is InvalidDbNullException)
					return null;
				else
					throw e.InnerException;
			}
		}
	}

}
