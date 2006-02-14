using System;
using Neo.Core.Qualifiers;


namespace Neo.Core.Util
{

	public class QualifierConverter
	{
		//--------------------------------------------------------------------------------------
		//	Fields and Constructor
		//--------------------------------------------------------------------------------------

		private IEntityMap	emap;

		public QualifierConverter(IEntityMap anEmap)
		{
			emap = anEmap;
		}

		
		//--------------------------------------------------------------------------------------
		//	Property to Column qualifier
		//--------------------------------------------------------------------------------------

		public ColumnQualifier ConvertToColumnQualifier(PropertyQualifier propQualifier)
		{
			string	column;
			object	compVal;

			if(isRelation(propQualifier.Property))
			{
				RelationInfo info = emap.GetRelationInfo(propQualifier.Property);
				column = info.ChildKey;
				IEntityObject eo = (IEntityObject)propQualifier.Predicate.Value;
				compVal = (eo == null) ? null : eo.Row[info.ParentKey];
			}
			else
			{
				column = emap.GetColumnForAttribute(propQualifier.Property);
				compVal = propQualifier.Predicate.Value;
			}
			if(compVal == null)
				compVal = DBNull.Value;
			IPredicate predicate = (IPredicate)Activator.CreateInstance(propQualifier.Predicate.GetType(), new object[] { compVal });

			return new ColumnQualifier(column, predicate);
		}

		private bool isRelation(string property)
		{
			string[] relations = emap.Relations;
			for(int i = 0; i < relations.Length; i++)
			{
				if(relations[i] == property)
					return true;
			}
			return false;
		}


		public Qualifier ConvertToColumnQualifiersRecursively(Qualifier aQualifier)
		{
			return (Qualifier)aQualifier.AcceptVisitor(new PropertyToColumnConverter(this));
		}


		protected class PropertyToColumnConverter : CopyingConverterBase
		{
			
			public PropertyToColumnConverter(QualifierConverter outer) : base(outer)
			{
			}
		
			public override object VisitPropertyQualifier(PropertyQualifier propQualifier)
			{
				return outer.ConvertToColumnQualifier(propQualifier);
			}

		}


		//--------------------------------------------------------------------------------------
		//	Changing entity objects
		//--------------------------------------------------------------------------------------
		
		public PropertyQualifier MoveEntityObject(PropertyQualifier propQualifier, ObjectContext newContext)
		{
			IEntityObject	eo, eoInNewContext;
			IPredicate		predicate;

			if((eo = propQualifier.Predicate.Value as IEntityObject) == null)
				return propQualifier;

			eoInNewContext = newContext.GetLocalObject(eo, false);
			if(eoInNewContext == null)
				throw new ObjectNotFoundException("Cannot convert object in qualifier; object not found in new context.");

			predicate = (IPredicate)Activator.CreateInstance(propQualifier.Predicate.GetType(), new object[] { eoInNewContext });
			return new PropertyQualifier(propQualifier.Property, predicate);
		}

        
		public Qualifier MoveEntityObjectsRecursively(Qualifier aQualifier, ObjectContext newContext)
		{
			return (Qualifier)aQualifier.AcceptVisitor(new EntityObjectConverter(this, newContext));
		}


		protected class EntityObjectConverter : CopyingConverterBase
		{
			private ObjectContext otherContext;

			public EntityObjectConverter(QualifierConverter outer, ObjectContext otherContext) : base(outer)
			{
				this.otherContext = otherContext;
			}


			public override object VisitPropertyQualifier(PropertyQualifier propQualifier)
			{
				return outer.MoveEntityObject(propQualifier, otherContext);
			}

		}
		
		
		//--------------------------------------------------------------------------------------
		//	Generic IQualifierVisitor impl
		//--------------------------------------------------------------------------------------

		protected class CopyingConverterBase : IQualifierVisitor
		{
			protected QualifierConverter	outer;

			public CopyingConverterBase(QualifierConverter outer)
			{
				this.outer = outer;
			}


		public object VisitColumnQualifier(ColumnQualifier columnQualifier)
		{
			return columnQualifier;
		}


			public virtual object VisitPropertyQualifier(PropertyQualifier propQualifier)
		{
				return propQualifier;
		}


		public object VisitPathQualifier(PathQualifier pathQualifier)
		{
			return pathQualifier;
		}


		public object VisitOrQualifier(OrQualifier q)
		{
			return new OrQualifier(ConvertQualfiers(q.Qualifiers));
		}


		public object VisitAndQualifier(AndQualifier q)
		{
			return new AndQualifier(ConvertQualfiers(q.Qualifiers));
		}


		private Qualifier[] ConvertQualfiers(Qualifier[] qualifiers)
		{
			Qualifier[] convertedQualifiers = new Qualifier[qualifiers.Length];
			for(int i = 0; i < qualifiers.Length; i++)
				convertedQualifiers[i] = (Qualifier)qualifiers[i].AcceptVisitor(this);
			return convertedQualifiers;
		}

		}
	

	}
}
