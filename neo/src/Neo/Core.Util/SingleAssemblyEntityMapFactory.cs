using System.Reflection;
using log4net;


namespace Neo.Core.Util
{
	/// <summary>
	/// This class is not used internally by NEO.
	/// You should subclass this class, to provide an entityMapFactory restricted to a single assembly:
	/// Useful when you are using more than one schema at a time and the table names clash.
	/// For example when migrating data between two databases.
	/// </summary>
	/// <remarks>
	/// The minimal subclass is:
	/// public class MyEntityMapFactory : SingleAssemblyEntityMapFactory
	/// {
	///   public override Assembly GetContainingAssembly()
	///   {
	///		return typeof(MyEntityMapFactory).Assembly;
	///   }
	/// }
	/// then use...
	/// context = new ObjectContext();
	/// context.Factory = new MyEntityMapFactory();
	/// 
	/// A more useful factory might be:
	/// 
	/// public class SingletonEntityMapFactory : SingleAsssemblyEntityMapFactory
	/// {
	///		private static SingletonEntityMapFactory sharedInstance;
	///		public SingletonEntityMapFactory() : base()
	///		{
	///		}
	///     public static SingletonEntityMapFactory SharedInstance
	///     {
	///		    get
	///         { 
	/// 	        if(sharedInstance == null)
	/// 	        sharedInstance = new SingletonEntityMapFactory();
	/// 	        return sharedInstance;
	///         }
	///     }
	///   public override Assembly GetContainingAssembly()
	///   {
	///		return typeof(SingletonEntityMapFactory).Assembly;
	///   }
	/// }
	/// 
	/// and use:
 	/// context.Factory = SingletonEntityMapFactory.SharedInstance;
	/// </remarks>
	public abstract class SingleAssemblyEntityMapFactory : EntityMapFactoryBase
	{
		//--------------------------------------------------------------------------------------
		//	Fields and constructor
		//--------------------------------------------------------------------------------------
	
		public SingleAssemblyEntityMapFactory()
		{
			if(logger == null)
				logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
		}
	
		
		//--------------------------------------------------------------------------------------
		//	Registration
		//--------------------------------------------------------------------------------------

		protected override void RegisterEntityMaps()
		{
			Assembly a = GetContainingAssembly();
			RegisterEntityMapsInAssembly(a);
		}


		public abstract Assembly GetContainingAssembly();

	}
}
