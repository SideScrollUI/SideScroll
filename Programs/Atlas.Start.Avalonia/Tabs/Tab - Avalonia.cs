using System;
using Atlas.Core;
using Atlas.GUI.Avalonia.Controls;
using Atlas.Tabs;
using Atlas.Tabs.Test;

namespace Atlas.Start.Avalonia.Tabs
{
	public class TabAvalonia : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			public Instance()
			{
			}

			public Instance(Project project)
			{
				this.project = project;
				if (project.userSettings.AutoLoad) // did we load successfully last time?
					LoadDefaultBookmark();

				tabModel.Name = "Avalonia";
				tabModel.Bookmarks = new BookmarkCollection(project);
			}

			public override void Load(Call call)
			{
				//TabControlBookmarks bookmarks = new TabControlBookmarks(this);
				ItemCollection<ListItem> items = new ItemCollection<ListItem>()
				{
					//new ListItem("Demo", new TabDemo()),
					new ListItem("Test", new TabTest()),
					//new ListItem("SeriLog", new TabSeriLog()),
					new ListItem("Custom Control", new TabCustomControl()),
					new ListItem("Icons", new TabIcons()),
					//new ListItem("Inputs", new TabParams()),
				};
				/*ItemCollection<ListItem> bookmarkedItems = new ItemCollection<ListItem>()
				{
					//new ListItem("Demo", new TabDemo()),
					new ListItem("Test", new TabTest()),
				};*/
				tabModel.Items = items;

				//tabModel.AddObject(bookmarks);
			}
		}
	}
}
/*

*/
