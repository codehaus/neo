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
		private ArrayList	entities;
		private	Hashtable	entityMap;


		public Model()
		{
			entities = new ArrayList();
			entityMap = new Hashtable();
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
			entities.Add(anEntity);
			entityMap.Add(anEntity.TableName, anEntity);
		}

		public void RemoveEntity(Entity anEntity)
		{
			entities.Remove(anEntity);
			entityMap.Remove(anEntity.TableName);
		}

		public ICollection Entities
		{
			get { return entities; }
		}
		
		
		//--------------------------------------------------------------------------------------
		//	helper
		//--------------------------------------------------------------------------------------
	
		public Entity GetEntityForTable(string tableName)
		{
		    Entity entity;

			if((entity = (Entity)entityMap[tableName]) == null)
				throw new NullReferenceException("Reference to undefined table " + tableName + ".");
			return entity;
		}



	}

}
