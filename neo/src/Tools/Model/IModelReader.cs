using System;
using System.IO;

namespace Neo.Model
{
	public interface IModelReader
	{
		void LoadModel(string path);
		IEntity GetNextEntity();
	}
}
