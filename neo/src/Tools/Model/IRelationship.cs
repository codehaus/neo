using System;
using System.Collections;
using System.Data;


namespace Neo.Model
{
	
	public interface IRelationship
	{		
		string LocalKey { get; }
		string ForeignKey { get; }

		string VarName { get; }
		
		RelDirection Direction { get; }
		RelType Type { get; }

		Rule UpdateRule { get; }
		Rule DeleteRule { get; }
					
		IEntity ForeignEntity { get; }
		IEntity LocalEntity { get; }
		IRelationship InverseRelationship { get; }
	}


	[Flags]
	public enum RelType
	{
		ToOne  = 0x01,
		ToMany = 0x02,
		All    = 0xFF
	}

	[Flags]
	public enum RelDirection
	{
		Parent = 0x01,
		Child  = 0x02,
		Any    = 0xFF
	}
	

}
