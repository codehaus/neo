using System;
using System.Collections;
using System.Reflection;
using log4net;


namespace Neo.Core.Util
{
	/// <summary>
	/// A useful base class for implementing custom entity map factories.
	/// </summary>
	/// <remarks>
	/// You do not have to use this base class, of course it is sufficient to
	/// implement the IEntityMapFactory interface to be a valid factory.
	/// </remarks>
	public abstract class EntityMapFactoryBase : IEntityMapFactory
	{
		//--------------------------------------------------------------------------------------
		//	Static fields and constructor
		//--------------------------------------------------------------------------------------

		protected static ILog logger;

		//--------------------------------------------------------------------------------------
		//	Fields and constructor
		//--------------------------------------------------------------------------------------
	
		protected ArrayList registeredTypes;
		protected Hashtable mapByObjectTypeTable;
		protected Hashtable mapByTableNameTable;
	

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

			RegisterEntityMaps();
		}

		/// <summary>
		/// Abstract method that should be used to implement assembly discovery. 
		/// </summary>
		/// <remarks>
		/// It is expected that this method calls RegisterEntityMapsInAssembly
		/// which is defined in this baseclass.
		/// </remarks>
		protected abstract void RegisterEntityMaps();

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
					if (mapByTableNameTable.ContainsKey(m.TableName))
						logger.Error(m.TableName + " appears twice - unpredictable behaviour");
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
			if (m == null) 
			{
				throw new ArgumentException("No entity map for table name " + tablename);
			}
			return m;
		}

	}
}
