using Atlas.Core;
using Atlas.Start.Avalonia.Charts;
using Atlas.Tabs;
using Atlas.Tabs.Test;
using Atlas.Tabs.Tools;
using Atlas.UI.Avalonia.Controls;
using Atlas.UI.Avalonia.Samples.Controls;

namespace Atlas.Start.Avalonia.Tabs;

public class TabAvalonia : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = new ItemCollection<ListItem>()
			{
				new("Test", new TabTest()),
				new("Controls", new TabSampleControls()),
				new("Charts", new TabCustomCharts()),
				new("Links", new TabBookmarks(Project)),
				new("Settings", new TabAvaloniaSettings()),
			};
		}
	}
}
