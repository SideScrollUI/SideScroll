using Atlas.Core;
using Atlas.Tabs;
using Atlas.Tabs.Test;
using Atlas.UI.Avalonia.Controls;

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
					new("Custom Control", new TabCustomControl()),
					new("Bookmarks", new TabBookmarks(Project)),
					//new("Demo", new TabDemo()),
				};
		}
	}
}
