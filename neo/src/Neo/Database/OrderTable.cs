using System.Data;


namespace Neo.Database
{
	
	public class OrderTable 
	{
		private readonly DataTable table;
		private int order;

		public OrderTable(DataTable table) 
		{
			this.table = table;
			order = 0;
		}
		
		public DataTable Table
		{
			get
			{
				return table;
			}
		}

		public int Order 
		{
			get 
			{
				return order;
			}
			set 
			{
				order = value;
			}
		}

		public bool IsChildOf(OrderTable parentTable)
		{
			foreach(DataRelation rel in Table.ParentRelations)
			{
				if(rel.ParentTable.Equals(parentTable.Table))
				{
					return true;
				}
			}
			return false;
		}
	}
}
