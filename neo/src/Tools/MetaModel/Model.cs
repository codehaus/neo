using System;
using System.Collections;


namespace Neo.MetaModel
{

	public class Model
	{
		//--------------------------------------------------------------------------------------
		//	constructor
		//--------------------------------------------------------------------------------------

		private string		ns;
		private string		name;
		private	Hashtable	entities;


		public Model()
		{
			entities = new Hashtable();
		}


		//--------------------------------------------------------------------------------------
		//	public attributes
		//--------------------------------------------------------------------------------------

		public string Name
		{
			set { name = value; }
			get { return name; }
		}

	    public string Namespace 
		{
			set { ns = value; }
			get	{ return ns; }
		}


		public void AddEntity(Entity anEntity)
		{
			entities.Add(anEntity.TableName, anEntity);
		}

		public void RemoveEntity(Entity anEntity)
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

	
		public Entity GetEntityForTable(string tableName)
		{
		    Entity entity;

			if((entity = (Entity)entities[tableName]) == null)
				throw new NullReferenceException("Reference to undefined table " + tableName + ".");
			return entity;
		}



	}

}
