using System;
using System.Collections;
using System.Data;


namespace Neo.Model
{
	public interface IEntity
	{
		string TableName { get; }

		string SubPackageName { get; }

		string Namespace { get; }
		string ClassName { get ; }
		string BaseClassName { get; }
		
		IdMethod IdMethod { get; }

		IModel Model { get; }

		ICollection PkColumns { get; }
		ICollection Attributes { get; }

		ICollection Relationships { get; }
		ICollection ToOneRelationships { get; }
		ICollection ToManyRelationships { get; }
		IRelationship RelationshipForAttribute(IAttribute attr);
		IList RelationshipsForAttribute(IAttribute attr);
		IList[] RelationshipSetsForColumns(IList attrList);

		
		ICollection UsedNamespaces { get; }
		bool PrimaryKeyIsForeignKey { get; }
	}


	public enum IdMethod
	{
		IdBroker,
		Native,
		Guid,
		None
	}


}