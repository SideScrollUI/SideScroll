using SideScroll.Attributes;
using SideScroll.Resources;
using SideScroll.Tabs;
using SideScroll.Tabs.Bookmarks;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.Avalonia.Tabs;

public class TabBookmarks(BookmarkCollection bookmarks) : ITab
{
	public BookmarkCollection Bookmarks { get; set; } = bookmarks;

	public TabInstance Create() => new Instance(this);

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonRefresh { get; set; } = new ToolButton("Refresh", Icons.Svg.Refresh);

		[Separator]
		public ToolButton ButtonDeleteAll { get; set; } = new ToolButton("Delete All", Icons.Svg.DeleteList);
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
			var toolbar = new Toolbar();
			toolbar.ButtonRefresh.Action = Refresh;
			//toolbar.ButtonReset.Action = Reset;
			toolbar.ButtonDeleteAll.Action = DeleteAll;
			model.AddObject(toolbar);

			if (tab.Bookmarks.NewBookmark != null)
			{
				SelectItem(tab.Bookmarks.NewBookmark);
				tab.Bookmarks.NewBookmark = null;
			}
		}

		public override void GetBookmark(TabBookmark tabBookmark)
		{
			base.GetBookmark(tabBookmark);

			foreach (TabBookmark childBookmark in tabBookmark.ChildBookmarks.Values)
			{
				childBookmark.IsRoot = true;
			}
		}

		private void Refresh(Call call)
		{
			Refresh();
		}

		/*private void Reset(Call call)
		{
			foreach (TabBookmarkItem item in SelectedItems)
			{
				SelectItem(item);
			}
		}*/

		private void DeleteAll(Call call)
		{
			tab.Bookmarks.DeleteAll(call);
			tab.Bookmarks.Load(call, true);
		}
	}
}
