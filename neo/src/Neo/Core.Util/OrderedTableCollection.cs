using System.Collections;
using System.Data;


namespace Neo.Core.Util
{
	
	public class OrderedTableCollection : ReadOnlyCollectionBase
	{

		//--------------------------------------------------------------------------------------
		//	Fields and constructor
		//--------------------------------------------------------------------------------------
		
		public OrderedTableCollection(DataSet aDataSet)
		{
			InternalDepthFirstSort sort = new InternalDepthFirstSort(aDataSet, this.InnerList);
			sort.Sort();
		}


		//--------------------------------------------------------------------------------------
		//	Indexer
		//--------------------------------------------------------------------------------------

		public DataTable this[int index]
		{
			get { return (DataTable)InnerList[index]; }
		}

		
		//--------------------------------------------------------------------------------------
		//	Sorting algorithm
		//--------------------------------------------------------------------------------------

		// This is split out into a separate class, so I could try different implementations and show
		// which varialbles were clearly part of the implementation.
		// This is a simple recursive version of a DepthFirstSort, following the ParentRelations.
		// It works by selecting each table as a startpoint, and following the ParentRelations.
		// When there are no more ParentRelations, we know we can output the table.
		// We keep track of which tables we've seen as we go, so we only visit tables once.
		// Note: Cycles are not detected, but since we keep track of the tables,
		// the algorithm is guaranteed to finish.

		private class InternalDepthFirstSort
		{
			private Hashtable m_seen;
			private DataSet m_ds;
			private ArrayList m_OrderedTables;

			internal InternalDepthFirstSort(DataSet dataset,ArrayList order)
			{
				m_ds = dataset;
				m_seen = new Hashtable();
				m_OrderedTables = order;
			}
			public ArrayList Sort()
			{
				foreach( DataTable table in m_ds.Tables)
				{
					// work through the tables skipping the ones we've already seen
					if (!m_seen.ContainsKey(table)) 
					{
						Visit(table);
					}
				}
				return m_OrderedTables;
			}
			private void Visit(DataTable table)
			{
				m_seen.Add(table,null);
				foreach (DataRelation rel in table.ParentRelations)
				{
					if (!m_seen.ContainsKey(rel.ParentTable)) 
					{
						Visit(rel.ParentTable);
					}
				}
				m_OrderedTables.Add(table);
			}
		}
	}
}
