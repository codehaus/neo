using System;

namespace Neo.Core.Util
{
	/// <summary>
	/// Describes a relation between two entities.
	/// </summary>
	public class RelationInfo
	{
		public readonly IEntityMap ParentEntity;
		public readonly IEntityMap ChildEntity;
		public readonly string ParentKey;
		public readonly string ChildKey;

		public RelationInfo(IEntityMap parentEntity, IEntityMap childEntity, string parentKey, string childKey)
		{
			ParentEntity = parentEntity;
			ChildEntity = childEntity;
			ParentKey = parentKey;
			ChildKey = childKey;
		}

		public RelationInfo(IEntityMapFactory factory, Type parentType, Type childType, string parentKey, string childKey)
		{
			ParentEntity = factory.GetMap(parentType);
			ChildEntity = factory.GetMap(childType);
			ParentKey = parentKey;
			ChildKey = childKey;
		}

		public override bool Equals(object obj)
		{
			RelationInfo other = obj as RelationInfo;
			if (other == null)
				return false;

			return (this.ParentEntity == other.ParentEntity && this.ChildEntity == other.ChildEntity &&
				this.ParentKey == other.ParentKey && this.ChildKey == other.ChildKey);
		}

		public override int GetHashCode()
		{
			return ParentEntity.GetHashCode() ^ ChildEntity.GetHashCode();
		}


	}
}