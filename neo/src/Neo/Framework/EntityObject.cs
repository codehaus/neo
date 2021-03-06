using System;
using System.Collections;
using System.Data;
using log4net;
using Neo.Core;
using Neo.Core.Util;


namespace Neo.Framework
{
	/// <summary>
	/// A good base class for your entity objects. CodeGen/Norque uses these.
	/// </summary>

	public class EntityObject : IEntityObject
	{
		//--------------------------------------------------------------------------------------
		//	Fields and constructors
		//--------------------------------------------------------------------------------------

		protected readonly DataRow row;
		protected readonly ObjectContext context;

		private EntityObject()
		{
		}

		protected EntityObject(DataRow aRow, ObjectContext aContext)
		{
			row = aRow;
			context = aContext;
		}


		//--------------------------------------------------------------------------------------
		//	IEntityObject impl
		//--------------------------------------------------------------------------------------

		public DataRow Row 
		{
			get { return row; }
		}

		public ObjectContext Context
		{
			get { return context; }
		}
			

		//--------------------------------------------------------------------------------------
		//	System overrides
		//--------------------------------------------------------------------------------------

		private static Hashtable loggerTable;

		protected static ILog GetLogger(Type type)
		{
			if(loggerTable == null)
				loggerTable = new Hashtable();

			ILog logger = (ILog)loggerTable[type];
			if(logger == null)
			{
				logger = LogManager.GetLogger(type);
				loggerTable[type] = logger;
			}
			return logger;
		}


		//--------------------------------------------------------------------------------------
		//	System overrides
		//--------------------------------------------------------------------------------------

		public override string ToString()
		{
			return ToStringAllProperties();
		}


		public string ToStringAllProperties()
		{
			if(row.RowState == DataRowState.Detached)
				return this.GetType().Name + "[detached]";
			
			DataRowVersion lookupVersion = (row.RowState == DataRowState.Deleted) ? DataRowVersion.Original : DataRowVersion.Current;
			ArrayList values = new ArrayList();
			foreach(DataColumn c in row.Table.Columns)
			{
				object v = row[c, lookupVersion];
				if(v.GetType() == typeof(Guid))
					continue;
				values.Add(v.ToString());
			}
			return this.GetType().Name + "[" + string.Join(", ", (string[])values.ToArray(typeof(string))) + "]";
		}

		
		//--------------------------------------------------------------------------------------
		//	Useful methods
		//--------------------------------------------------------------------------------------

		public object[] GetPrimaryKeyValues()
		{
			IEntityMap emap = Context.EntityMapFactory.GetMap(GetType());
			return Context.GetPrimaryKeyValuesForRow(emap, this.Row, DataRowVersion.Current);
		}


		public bool IsSame(EntityObject eo)
		{
			// #ed# revisit. they only need to inherit from the same specific base
			if(this.GetType() != eo.GetType())
				return false;

			IEntityMap emap = Context.EntityMapFactory.GetMap(GetType());
			object[] thisPk = Context.GetPrimaryKeyValuesForRow(emap, this.Row, DataRowVersion.Current);
			object[] otherPk = Context.GetPrimaryKeyValuesForRow(emap, eo.Row, DataRowVersion.Current);
			
			for(int i = 0; i < thisPk.Length; i++)
				if(thisPk[i].Equals(otherPk[i]) == false)
					return false;
			return true;
		}


        public virtual bool HasChanges()
		{
			return Row.RowState != DataRowState.Unchanged;
		}


		public bool IsDeleted()
		{
			return (Row.RowState == DataRowState.Deleted) || (Row.RowState == DataRowState.Detached);
		}

        
		public void SetProperty(string propName, object propValue)
		{
		    ObjectHelper.SetProperty(this, propName, propValue);
		}

		public virtual object GetProperty(string propName)
		{
			return ObjectHelper.GetProperty(this, propName);
		}

		public virtual void RejectChanges()
		{
			row.RejectChanges();
		}

		
		public virtual void Delete()
		{
			Context.DeleteObject(this);
		}

		public IList GetReferences()
		{
			return Context.GetReferencingObjects(this, false);
		}

		public IList GetReferences(bool excludeCascades)
		{
			return Context.GetReferencingObjects(this, excludeCascades);
		}


		protected virtual object HandleNullValueForProperty(string propName)
		{
			throw new InvalidDbNullException(propName + " is a value type. DBNulls can only be converted to reference types.");
		}

		protected virtual void SetRelatedObject(IEntityObject eo, string keyColumnInThisEntity, string keyColumnInOtherEntity)
		{
			if(eo == null)
			{
				Row[keyColumnInThisEntity] = DBNull.Value;
				return;
			}
			if(eo.Context != Context)
			{
				throw new ArgumentException("Assignment of object from different context.");
			}
			Row[keyColumnInThisEntity] = eo.Row[keyColumnInOtherEntity];
		}

		protected virtual object GetRelatedObject(string tableName, object fkvalue)
		{
			IEntityObject eo = Context.GetObjectFromTable(tableName, new object[] { fkvalue });
			if(eo == null)
				throw new ObjectNotFoundException(String.Format("Failed to look up object for foreign key; looking for {0}[{1}]", tableName, fkvalue));
			return eo;
		}
			
	}

}
