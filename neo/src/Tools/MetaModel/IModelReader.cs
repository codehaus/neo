namespace Neo.MetaModel
{
	public delegate void ConfigDelegate(string inputPath, string name, string val);


	public interface IModelReader
	{
		void ReadConfiguration(string path, ConfigDelegate aDelegate);
		void LoadModel(string path);
		Model Model { get; }
		Entity GetNextEntity();
	}
}
