using System.Collections;
using System.Data;
using Neo.Core;


namespace Neo.Database
{
	public interface IDbCommandBuilder
	{
		void WriteSelect(IFetchSpecification fetchSpec);
		void WriteInsert(DataRow row, IList columnList);
		void WriteUpdate(DataRow row);
		void WriteDelete(DataRow row);

		string Command { get; }
		IList Parameters { get; }

	}
}
