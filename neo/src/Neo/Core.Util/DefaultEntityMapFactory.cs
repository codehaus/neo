using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using log4net;
using Neo.Core;


namespace Neo.Core.Util
{
	/// <summary>
	/// Summary description for EntityMapFactory.
	/// </summary>
	public class DefaultEntityMapFactory : IEntityMapFactory
	{
		//--------------------------------------------------------------------------------------
		//	Static fields and constructor
		//--------------------------------------------------------------------------------------

		private static DefaultEntityMapFactory sharedInstance;
		protected static ILog logger;


		//--------------------------------------------------------------------------------------
		//	Singleton
		//--------------------------------------------------------------------------------------

		public static DefaultEntityMapFactory SharedInstance
		{
			get
			{
				if(sharedInstance == null)
					sharedInstance = new DefaultEntityMapFactory();
				return sharedInstance;
			}
		}


		//--------------------------------------------------------------------------------------
		//	Fields and constructor
		//--------------------------------------------------------------------------------------
		
		private string[] assemblyFilters;
		private ArrayList registeredTypes;
		private Hashtable mapByObjectTypeTable;
		private Hashtable mapByTableNameTable;
		

		protected DefaultEntityMapFactory()
		{
			if(logger == null)
				logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString());
			assemblyFilters = new string[] { "mscorlib", "System", "nunit", "log4net" };
		}


		//--------------------------------------------------------------------------------------
		//	Access filters
		//--------------------------------------------------------------------------------------

		public ArrayList AssemblyFilters
		{
			get
			{
				return ArrayList.ReadOnly(new ArrayList(assemblyFilters));
			}
		}

		
		public void AddToAssemblyFilters(string partialName)
		{
			string[] newFilters = new string[assemblyFilters.Length + 1];
			assemblyFilters.CopyTo(newFilters, 0);
			newFilters[newFilters.Length - 1] = partialName;
			assemblyFilters = newFilters;
		}

		
		//--------------------------------------------------------------------------------------
		//	Registration
		//--------------------------------------------------------------------------------------

		protected virtual void EnsureMapsAreRegistered()
		{
			if(registeredTypes != null)
				return;

			registeredTypes = new ArrayList();
			mapByObjectTypeTable = new Hashtable();
			mapByTableNameTable = new Hashtable();

			Regex ignoreExpr = new Regex("^(" + String.Join("|", assemblyFilters) + ")");
			ArrayList seenAssemblies = new ArrayList();
			Queue waitingAssemblies = new Queue();

			foreach(Assembly a in AppDomain.CurrentDomain.GetAssemblies())
			{
				if(ignoreExpr.IsMatch(a.FullName))
				{
					logger.Debug("Ignoring: " + a.FullName + " (uninteresting)");
					continue;
				}
				waitingAssemblies.Enqueue(a);
			}

			while(waitingAssemblies.Count > 0)
			{
				Assembly a = (Assembly)waitingAssemblies.Dequeue();
				logger.Debug("Processing: " + a.FullName);
				seenAssemblies.Add(a.FullName);
				RegisterEntityMapsInAssembly(a);
				foreach(AssemblyName an in a.GetReferencedAssemblies())
				{
					if(seenAssemblies.Contains(an.FullName))
					{
						logger.Debug("Ignoring: " + an.FullName + " (processed before)");
						continue;
					}
					if(ignoreExpr.IsMatch(an.FullName))
					{
						logger.Debug("Ignoring: " + an.FullName + " (uninteresting)");
						continue;
					}
					Assembly refd = Assembly.Load(an);
					waitingAssemblies.Enqueue(refd);
				}
			}
		}


		protected virtual void RegisterEntityMapsInAssembly(Assembly assembly)
		{
			TypeFilter filter = new TypeFilter(IsIEntityMap);
			foreach(Type t in assembly.GetTypes())
			{
				if((t.IsClass == true) && (t.IsAbstract == false) && 
					(t.FindInterfaces(filter, null).Length > 0) && (registeredTypes.Contains(t) == false))
				{
					registeredTypes.Add(t);
					IEntityMap m = (IEntityMap)Activator.CreateInstance(t);
					m.Factory = this;
					mapByObjectTypeTable[m.ObjectType] = m;
					mapByTableNameTable[m.TableName] = m;
				}
			}
		}


		public bool IsIEntityMap(Type typeObj, Object criteriaObj)
		{
			return (typeObj.IsInterface) && (typeObj == typeof(IEntityMap));
		}


		public virtual ICollection GetRegisteredTypes()
		{
			EnsureMapsAreRegistered();
			return registeredTypes;
		}


		//--------------------------------------------------------------------------------------
		//	IEntityMapFactory impl
		//--------------------------------------------------------------------------------------
	
		#region IEntityMapFactory Members

		public virtual ICollection GetAllMaps()
		{
			EnsureMapsAreRegistered();
			return mapByObjectTypeTable.Values;
		}

		public virtual IEntityMap GetMap(Type type)
		{
			EnsureMapsAreRegistered();
			IEntityMap m = (IEntityMap)mapByObjectTypeTable[type];
			if(m == null)
				throw new ArgumentException("No entity map for object type " + type.FullName);
			return m;
		}

		public virtual IEntityMap GetMap(string tablename)
		{
			EnsureMapsAreRegistered();
			IEntityMap m = (IEntityMap)mapByTableNameTable[tablename];
			if(m == null)
				throw new ArgumentException("No entity map for table name " + tablename);
			return m;
		}

		#endregion

		}
}
