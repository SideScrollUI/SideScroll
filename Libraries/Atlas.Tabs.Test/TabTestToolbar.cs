using Atlas.Core;
using Atlas.Extensions;
using Atlas.Resources;
using System;

namespace Atlas.Tabs.Test
{
	public class TabTestToolbar : ITab
	{
		public TabInstance Create() => new Instance();

		public class TestToolbar : TabToolbar
		{
			public ToolButton ButtonRefresh { get; set; } = new ToolButton("Refresh", Icons.Streams.Refresh);

			[Separator]
			public ToolButton ButtonOpenBrowser { get; set; } = new ToolButton("Open in Browser", Icons.Streams.Browser);

			[Separator]
			public ToolComboBox<TimeSpan> Duration { get; set; } = new ToolComboBox<TimeSpan>("Duration", TimeSpanExtensions.CommonTimeSpans, TimeSpan.FromMinutes(5));

			[Separator]
			public string Label => "(Status)";
		}

		public class Instance : TabInstance
		{
			public override void Load(Call call, TabModel model)
			{
				var toolbar = new TestToolbar();
				toolbar.ButtonRefresh.Action = ButtonRefresh_Click;
				toolbar.ButtonOpenBrowser.Action = ButtonOpenBrowser_Click;

				model.AddObject(toolbar);
			}

			private void ButtonRefresh_Click(Call call)
			{
				Reload();
			}

			private void ButtonOpenBrowser_Click(Call call)
			{
				string uri = "https://www.wikipedia.org/";
				ProcessUtils.OpenBrowser(uri);
			}
		}
	}
}
