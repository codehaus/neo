using System;
using System.Collections;
using Neo.Model;


namespace Neo.Model.Impl
{

	public class ModelImpl : IModel
	{
		//--------------------------------------------------------------------------------------
		//	constructor
		//--------------------------------------------------------------------------------------

		private string		ns;
		private	Hashtable	entities;


		public ModelImpl()
		{
			entities = new Hashtable();
		}


		//--------------------------------------------------------------------------------------
		//	public attributes
		//--------------------------------------------------------------------------------------

		public string Namespace 
		{
			set { ns = value; }
			get	{ return ns; }
		}


		public void AddEntity(EntityImpl anEntity)
		{
			entities.Add(anEntity.TableName, anEntity);
		}

		public void RemoveEntity(EntityImpl anEntity)
		{
			entities.Remove(anEntity.TableName);
		}

		public ICollection Entities
		{
			get { return entities.Values; }
		}
		
		
		//--------------------------------------------------------------------------------------
		//	helper
		//--------------------------------------------------------------------------------------

	
		public EntityImpl GetEntityForTable(string tableName)
		{
			EntityImpl entity;

			if((entity = (EntityImpl)entities[tableName]) == null)
				throw new NullReferenceException("Reference to undefined table " + tableName + ".");
			return entity;
		}



	}

}
