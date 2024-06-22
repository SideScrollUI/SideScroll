using SideScroll;
using SideScroll.Utilities;
using SideScroll.Extensions;
using SideScroll.Resources;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.Tabs.Samples;

public class TabSampleToolbar : ITab
{
	public TabInstance Create() => new Instance();

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonRefresh { get; set; } = new("Refresh", Icons.Svg.Refresh);

		[Separator]
		public ToolButton ButtonSearch { get; set; } = new("Search", Icons.Svg.Search)
		{
			ShowTask = true,
		};

		[Separator]
		public ToolButton ButtonOpenBrowser { get; set; } = new("Open in Browser", Icons.Svg.Browser);

		[Separator]
		public ToolComboBox<TimeSpan> Duration { get; set; } = new("Duration", TimeSpanExtensions.CommonTimeSpans, TimeSpan.FromMinutes(5));

		[Separator]
		public string Label => "(Status)";
	}

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			var toolbar = new Toolbar();
			toolbar.ButtonRefresh.Action = Refresh;
			toolbar.ButtonSearch.ActionAsync = SearchAsync;
			toolbar.ButtonOpenBrowser.Action = OpenBrowser;
			model.AddObject(toolbar);
		}

		private void Refresh(Call call)
		{
			Reload();
		}

		private async Task SearchAsync(Call call)
		{
			await Task.Delay(3000);
		}

		private static void OpenBrowser(Call call)
		{
			string uri = "https://www.wikipedia.org/";
			ProcessUtils.OpenBrowser(uri);
		}
	}
}
