using Atlas.Core;
using Atlas.Resources;
using Atlas.Tabs;

namespace Atlas.UI.Avalonia.Controls
{
	public class TabBookmarks : ITab
	{
		public static TabBookmarks Global;

		public BookmarkCollection Bookmarks;

		public TabBookmarks(Project project)
		{
			Bookmarks = new BookmarkCollection(project);
			Global = Global ?? this;
		}

		public void AddBookmark(Call call, Bookmark bookmark)
		{
			Bookmarks.AddNew(call, bookmark);
		}

		public TabInstance Create() => new Instance(this);

		public class Toolbar : TabToolbar
		{
			public ToolButton ButtonRefresh { get; set; } = new ToolButton("Refresh", Icons.Streams.Refresh);
			//public ToolButton ButtonReset { get; set; } = new ToolButton("Reset", Icons.Streams.Refresh);

			[Separator]
			public ToolButton ButtonDeleteAll { get; set; } = new ToolButton("Delete All", Icons.Streams.DeleteList);
		}

		public class Instance : TabInstance
		{
			public TabBookmarks Tab;

			public Instance(TabBookmarks tab)
			{
				Tab = tab;
			}

			public override void Load(Call call, TabModel model)
			{
				Tab.Bookmarks.Load(call, true);
			}

			public override void LoadUI(Call call, TabModel model)
			{
				var toolbar = new Toolbar();
				toolbar.ButtonRefresh.Action = Refresh;
				//toolbar.ButtonReset.Action = Reset;
				toolbar.ButtonDeleteAll.Action = DeleteAll;
				model.AddObject(toolbar);

				model.AddData(Tab.Bookmarks.Items);

				if (Tab.Bookmarks.NewBookmark != null)
				{
					SelectItem(Tab.Bookmarks.NewBookmark);
					Tab.Bookmarks.NewBookmark = null;
				}
			}

			public override void GetBookmark(TabBookmark tabBookmark)
			{
				base.GetBookmark(tabBookmark);

				foreach (var child in tabBookmark.ChildBookmarks.Values)
					child.IsRoot = true;
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
				Tab.Bookmarks.DeleteAll();
				Tab.Bookmarks.Load(call, true);
			}
		}
	}
}
