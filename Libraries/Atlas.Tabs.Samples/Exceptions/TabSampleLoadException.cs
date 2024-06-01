using Atlas.Core;

namespace Atlas.Tabs.Samples.Exceptions;

public class TabSampleLoadException : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			throw new Exception("Load exception");
		}
	}
}
