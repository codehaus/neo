using System;
using System.Xml;
using System.Security.Principal;
using Neo.Model;

namespace Neo.CodeGen.Core
{
	/// <summary>
	/// Summary description for DocReader.
	/// </summary>
	public class GenerationContext
	{
		//--------------------------------------------------------------------------------------
		//	fields / constructor
		//--------------------------------------------------------------------------------------

		private IEntity	entity;
		private ModelHelper helper;


		public GenerationContext(IEntity anEntity)
		{
			entity = anEntity;
			helper = new ModelHelper(entity);
		}

		
		//--------------------------------------------------------------------------------------
		//	properties
		//--------------------------------------------------------------------------------------

		public virtual ModelHelper Helper
		{
			get { return helper; }
		}


		public virtual IModel Model
		{
			get { return entity.Model; }
		}


		public virtual IEntity Entity
		{
			get { return entity; }
		}


		public virtual string FileName
		{
			get { return Entity.ClassName + ".cs"; }
		}

	
		public virtual string SupportFileName
		{
			get { return "_" + Entity.ClassName + ".cs"; }
		}


		public virtual string Date
		{
			get { return DateTime.Now.ToString(); }
		}


		public virtual string User
		{
			get { return WindowsIdentity.GetCurrent().Name; }
		}


		public override string ToString()
		{
			return "Knock, knock, Neo.";
		}

	}
}
