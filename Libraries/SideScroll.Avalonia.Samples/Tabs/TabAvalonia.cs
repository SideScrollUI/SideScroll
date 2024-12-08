using SideScroll.Avalonia.Samples.Charts;
using SideScroll.Avalonia.Samples.Controls;
using SideScroll.Avalonia.Tabs;
using SideScroll.Tabs;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Samples;

namespace SideScroll.Avalonia.Samples.Tabs;

public class TabAvalonia : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = new List<ListItem>
			{
				new("Samples", new TabSamples()),
				new("Controls", new TabSampleControls()),
				new("Charts", new TabCustomCharts()),
				new("Links", new TabBookmarks(Project)),
				new("Settings", new TabAvaloniaSettings<CustomUserSettings>()),
			};
		}
	}
}
