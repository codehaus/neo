using System;
using System.Security.Principal;
using Neo.MetaModel;


namespace Neo.Generator.Core
{

	public class GenerationContext
	{
		//--------------------------------------------------------------------------------------
		//	fields / constructor
		//--------------------------------------------------------------------------------------

		private Model	model;

		protected GenerationContext()
		{
		}

		public GenerationContext(Model aModel)
		{
		    model = aModel;
		}

		
		//--------------------------------------------------------------------------------------
		//	properties
		//--------------------------------------------------------------------------------------

		public virtual Model Model
		{
			get { return model; }
		}

		public string Date
		{
			get { return DateTime.Now.ToString(); }
		}

		public string User
		{
			get { return WindowsIdentity.GetCurrent().Name; }
		}

		public override string ToString()
		{
			return "Knock, knock, Neo.";
		}
	
	}
}
