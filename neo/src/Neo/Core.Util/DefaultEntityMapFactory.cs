using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using log4net;
using Neo.Core;


namespace Neo.Core.Util
{
	/// <summary>
	/// Default concrete implementation of IEntityMapFactory
	/// </summary>
	/// <remarks>
	/// Uses the singleton pattern to hold maps in memory once they have been processed.
	/// As a side effect it brings all assemblies that the current app depends on into memory.
	/// To avoid loading certain assemblies that are known not to contain entity maps use
	/// the AddAssemblyFilters method.
	/// </remarks>
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

		/// <summary>
		/// Gets shared and only instance of this class
		/// </summary>
		/// <remarks>
		/// Uses the <c>singleton</c> pattern to ensure only one instance ever exists.
		/// </remarks>
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
		

		/// <summary>
		/// Default, and non-public constructor since this class uses the singleton pattern.
		/// </summary>
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

		/// <summary>
		/// Checks whether Maps are registered. If not, processes assemblies in the current 
		/// domain.
		/// </summary>
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


		/// <summary>
		/// Runs through all concrete classes in the assembly and adds all <c>IEntityMap</c> 
		/// classes to the list of registered types, if they are not already registered.
		/// </summary>
		/// <param name="assembly">Assembly to be examined</param>
		/// <remarks>
		/// When registered, an instance of each <c>IEntityMap</c> class is added to the 
		/// internal maps for rapid lookup as required.
		/// </remarks>
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


		/// <summary>
		/// Determines whether the supplied type is a concrete subclass of <c>IEntityMap</c>
		/// </summary>
		/// <param name="typeObj">Class to be checked</param>
		/// <param name="criteriaObj">Not used</param>
		/// <returns><c>true</c> if this is a concrete class and a subclass of <c>IEntityMap</c></returns>
		public bool IsIEntityMap(Type typeObj, Object criteriaObj)
		{
			return (typeObj.IsInterface) && (typeObj == typeof(IEntityMap));
		}


		/// <summary>
		/// Gets all registered types, forcing a mapping check if necessary
		/// </summary>
		/// <returns>All registered types</returns>
		public virtual ICollection GetRegisteredTypes()
		{
			EnsureMapsAreRegistered();
			return registeredTypes;
		}


		//--------------------------------------------------------------------------------------
		//	IEntityMapFactory impl
		//--------------------------------------------------------------------------------------
	
		public virtual void AddCustomType(Type objType, IEntityMap map)
		{
			mapByObjectTypeTable[objType] = map;
		}


		/// <summary>
		/// Gets a collection of all Object Type Maps
		/// </summary>
		/// <returns>A collection of all Object Type Maps</returns>
		public virtual ICollection GetAllMaps()
		{
			EnsureMapsAreRegistered();
			return mapByObjectTypeTable.Values;
		}


		/// <summary>
		/// Gets the corresponding map for a type 
		/// </summary>
		/// <param name="type">Type to map</param>
		/// <returns>IEntityMap object matching the supplied Type</returns>
		public virtual IEntityMap GetMap(Type type)
		{
			EnsureMapsAreRegistered();
			IEntityMap m = (IEntityMap)mapByObjectTypeTable[type];
			if(m == null)
				throw new ArgumentException("No entity map for object type " + type.FullName);
			return m;
		}

		/// <summary>
		/// Gets the corresponding map for a table name 
		/// </summary>
		/// <param name="tablename">Table Name to map</param>
		/// <returns>IEntityMap object matching the supplied table name</returns>
		public virtual IEntityMap GetMap(string tablename)
		{
			EnsureMapsAreRegistered();
			IEntityMap m = (IEntityMap)mapByTableNameTable[tablename];
			if(m == null)
				throw new ArgumentException("No entity map for table name " + tablename);
			return m;
		}

	}
}
