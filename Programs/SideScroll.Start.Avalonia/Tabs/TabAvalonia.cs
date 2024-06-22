using SideScroll;
using SideScroll.Start.Avalonia.Charts;
using SideScroll.Tabs;
using SideScroll.Tabs.Samples;
using SideScroll.UI.Avalonia.Samples.Controls;
using SideScroll.UI.Avalonia.Tabs;

namespace SideScroll.Start.Avalonia.Tabs;

public class TabAvalonia : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = new ItemCollection<ListItem>
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
