using SideScroll.Attributes;
using SideScroll.Resources;
using SideScroll.Tabs;
using SideScroll.Tabs.Bookmarks;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.Avalonia.Tabs;

public class TabBookmarks(LinkCollection bookmarks) : ITab
{
	public LinkCollection Bookmarks { get; set; } = bookmarks;

	public TabInstance Create() => new Instance(this);

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonRefresh { get; set; } = new("Refresh", Icons.Svg.Refresh);

		[Separator]
		public ToolButton ButtonDeleteAll { get; set; } = new("Delete All", Icons.Svg.DeleteList);

		[Separator]
		public ToolToggleButton? ToggleButtonShowLinkInfoTab { get; set; }
	}

	public class Instance(TabBookmarks tab) : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.CustomSettingsPath = tab.Bookmarks.GroupId;
			model.MinDesiredWidth = 300;

			tab.Bookmarks.Load(call, true);

			model.AddData(tab.Bookmarks.Items);
		}

		public override void LoadUI(Call call, TabModel model)
		{
			Toolbar toolbar = new();
			toolbar.ButtonRefresh.Action = Refresh;
			toolbar.ButtonDeleteAll.Action = DeleteAll;
			ListProperty listProperty = new(tab.Bookmarks, nameof(LinkCollection.ShowLinkInfoTab));
			toolbar.ToggleButtonShowLinkInfoTab = new("Show Link Info Tab", Icons.Svg.PanelLeftContract, Icons.Svg.PanelLeftExpand, listProperty, ShowLinkTab);
			model.AddObject(toolbar);

			if (tab.Bookmarks.NewBookmark != null)
			{
				SelectItem(tab.Bookmarks.NewBookmark);
				tab.Bookmarks.NewBookmark = null;
			}
		}

		private void ShowLinkTab(Call call)
		{
			Reload();
		}

		public override void GetBookmark(TabBookmark tabBookmark)
		{
			base.GetBookmark(tabBookmark);

			// Set links created from this link to always start from the child Link Tab
			foreach (TabBookmark childBookmark in tabBookmark.ChildBookmarks.Values)
			{
				childBookmark.IsRoot = true;
			}
		}

		private void Refresh(Call call)
		{
			Refresh();
		}

		private void DeleteAll(Call call)
		{
			tab.Bookmarks.DeleteAll(call);
			tab.Bookmarks.Load(call, true);
		}
	}
}
