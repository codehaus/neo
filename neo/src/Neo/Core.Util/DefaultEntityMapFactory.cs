using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using log4net;


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
	public class DefaultEntityMapFactory : EntityMapFactoryBase
	{
		//--------------------------------------------------------------------------------------
		//	Static fields and constructor
		//--------------------------------------------------------------------------------------

		private static DefaultEntityMapFactory sharedInstance;


		//--------------------------------------------------------------------------------------
		//	Singleton
		//--------------------------------------------------------------------------------------

		/// <summary>
		/// Gets a shared instance of this class
		/// </summary>
		/// <remarks>
		/// It is possible to create further instances but unless there is a specific need for
		/// one it is advisable to use the shared one to avoid the overhead of searching through
		/// the assemblies multiple times.
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

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <remarks>
		/// Only create new instances if the shared one cannot be used.
		/// </remarks>
		public DefaultEntityMapFactory()
		{
			if(logger == null)
				logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
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

		protected override void RegisterEntityMaps()
		{
		    Regex ignoreExpr = new Regex("^(" + String.Join("|", assemblyFilters) + ")");
			ArrayList scheduledAssemblies = new ArrayList();
			Queue waitingAssemblies = new Queue();

			foreach(Assembly a in AppDomain.CurrentDomain.GetAssemblies())
			{
				if(ignoreExpr.IsMatch(a.FullName))
				{
					logger.Debug("Ignoring: " + a.FullName + " (uninteresting)");
					continue;
				}
				scheduledAssemblies.Add(a.FullName);
				waitingAssemblies.Enqueue(a);
			}

			while(waitingAssemblies.Count > 0)
			{
				Assembly a = (Assembly)waitingAssemblies.Dequeue();
				logger.Debug("Processing: " + a.FullName);
				RegisterEntityMapsInAssembly(a);
				foreach(AssemblyName an in a.GetReferencedAssemblies())
				{
					if(scheduledAssemblies.Contains(an.FullName))
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
					scheduledAssemblies.Add(an.FullName);
					waitingAssemblies.Enqueue(refd);
				}
			}
		}


		protected override void RegisterEntityMapsInAssembly(Assembly assembly)
		{
			try
			{
				base.RegisterEntityMapsInAssembly(assembly);
			}
			catch(Exception)
			{
			}
		}


	}
}
