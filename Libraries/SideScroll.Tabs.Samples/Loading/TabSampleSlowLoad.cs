using SideScroll.Core;

namespace SideScroll.Tabs.Samples.Loading;

public class TabSampleSlowLoad : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			Thread.Sleep(5000);

			model.AddObject("Finished");
		}
	}
}
