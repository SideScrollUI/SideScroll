using SideScroll.Resources;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.Tabs.Samples.Loading;

public class TabSampleLoadAsync : ITab
{
	public TabInstance Create() => new Instance();

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonRefresh { get; } = new("Refresh", Icons.Svg.Refresh);
	}

	public class Instance : TabInstance, ITabAsync
	{
		private const int DelayMs = 5000;

		public Instance()
		{
			LoadingMessage = "Loading ALL the things!";
		}

		public async Task LoadAsync(Call call, TabModel model)
		{
			Toolbar toolbar = new();
			toolbar.ButtonRefresh.Action = ReloadInstance;
			model.AddObject(toolbar);

			call.Log.Add("Sleeping", new Tag("Milliseconds", DelayMs));

			await Task.Delay(DelayMs);

			model.AddObject("Finished");

			model.Items = new List<int>
			{
				1, 2, 3
			};
		}

		private void ReloadInstance(Call call)
		{
			Reload();
		}
	}
}
