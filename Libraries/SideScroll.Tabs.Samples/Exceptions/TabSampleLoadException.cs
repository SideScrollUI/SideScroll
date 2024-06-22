using SideScroll.Core;

namespace SideScroll.Tabs.Samples.Exceptions;

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
