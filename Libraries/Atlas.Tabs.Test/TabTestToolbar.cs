using Atlas.Core;
using Atlas.Core.Utilities;
using Atlas.Extensions;
using Atlas.Resources;

namespace Atlas.Tabs.Test;

public class TabTestToolbar : ITab
{
	public TabInstance Create() => new Instance();

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonRefresh { get; set; } = new("Refresh", Icons.Svg.Refresh);

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
			toolbar.ButtonOpenBrowser.Action = OpenBrowser;

			model.AddObject(toolbar);
		}

		private void Refresh(Call call)
		{
			Reload();
		}

		private static void OpenBrowser(Call call)
		{
			string uri = "https://www.wikipedia.org/";
			ProcessUtils.OpenBrowser(uri);
		}
	}
}
