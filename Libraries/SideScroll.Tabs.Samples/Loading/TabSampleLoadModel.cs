namespace SideScroll.Tabs.Samples.Loading;

public class TabSampleLoadModel : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public Instance()
		{
			LoadingMessage = "Loading ALL the things!";
		}

		public override void Load(Call call, TabModel model)
		{
			Thread.Sleep(5000);

			model.AddObject("Finished");
		}
	}
}
