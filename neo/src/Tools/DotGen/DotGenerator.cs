using System;
using System.IO;
using Neo.MetaModel;


namespace Neo.DotGen.DotGenerator
{
	/// <summary>
	/// Summary description for DotGen.
	/// </summary>
	public class DotGenerator
	{
		TextWriter	outputWriter;


		public DotGenerator()
		{
		}


		public void Generate(string inputPath)
		{
			IModelReader	reader;
			Entity			entity;

			outputWriter = Console.Out;
			reader = new Neo.MetaModel.Reader.NorqueReader();
			reader.LoadModel(inputPath);
			WritePrologue();
			while((entity = reader.GetNextEntity()) != null)
				WriteEntity(entity);
			WriteEpilogue();
		}


		protected virtual void WritePrologue()
		{
			outputWriter.WriteLine("graph neo_model {");
		}


		protected virtual void WriteEntity(Entity entity)
		{
			foreach(EntityRelationship r in entity.Relationships)
			{
				if (r.Type == RelType.ToOne && notProcessed(entity.ClassName , r.ForeignEntity.ClassName))  {
					string head = "odot";
					string tail = "none";
					EntityRelationship backRel = r.InverseRelationship;
					if (backRel != null && backRel.Type == RelType.ToMany) {
						tail = "crow";
					}

					outputWriter.WriteLine("  {0} -- {1} [arrowhead={2}, arrowtail={3}]", entity.ClassName, r.ForeignEntity.ClassName, head, tail);
				}
			}
		}

		protected virtual bool notProcessed(string one, string two) {
			return true;
		}

		protected virtual void WriteEpilogue()
		{
			outputWriter.WriteLine("}");
		}

	}
}
