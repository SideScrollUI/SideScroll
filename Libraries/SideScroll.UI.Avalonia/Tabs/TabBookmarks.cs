using SideScroll.Resources;
using SideScroll.Tabs;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.UI.Avalonia.Tabs;

public class TabBookmarks : ITab
{
	public static TabBookmarks? Global { get; set; }

	public readonly BookmarkCollection Bookmarks;

	public TabBookmarks(Project project)
	{
		Bookmarks = new BookmarkCollection(project);
		Global ??= this;
	}

	public void AddBookmark(Call call, Bookmark bookmark)
	{
		Bookmarks.AddNew(call, bookmark);
	}

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
			tab.Bookmarks.Load(call, true);

			model.MinDesiredWidth = 300;
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
