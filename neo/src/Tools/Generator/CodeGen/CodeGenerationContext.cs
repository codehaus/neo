using Neo.Generator.Core;
using Neo.MetaModel;


namespace Neo.Generator.CodeGen
{

	public class CodeGenerationContext : GenerationContext
	{
		//--------------------------------------------------------------------------------------
		//	fields / constructor
		//--------------------------------------------------------------------------------------

		private Entity	entity;
		private ModelHelper helper;


		public CodeGenerationContext(Entity anEntity)
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


		public override Model Model
		{
			get { return entity.Model; }
		}


		public virtual Entity Entity
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
	

	}
}
