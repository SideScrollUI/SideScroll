using Atlas.Core;
using Atlas.Tabs;
using Atlas.Tabs.Test;
using Atlas.UI.Avalonia.Controls;

namespace Atlas.Start.Avalonia.Tabs
{
	public class TabAvalonia : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			public override void Load(Call call, TabModel model)
			{
				model.Items = new ItemCollection<ListItem>()
				{
					new ListItem("Test", new TabTest()),
					new ListItem("Custom Control", new TabCustomControl()),
					new ListItem("Icons", new TabIcons()),
					new ListItem("Bookmarks", new TabBookmarks(Project)),
					//new ListItem("Demo", new TabDemo()),
					//new ListItem("SeriLog", new TabSeriLog()),
				};
			}
		}
	}
}
