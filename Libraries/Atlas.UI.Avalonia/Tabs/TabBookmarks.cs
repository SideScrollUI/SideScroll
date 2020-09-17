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
			public ToolButton ButtonReset { get; set; } = new ToolButton("Reset", Icons.Streams.Refresh);
		}

		public class Instance : TabInstance
		{
			//private Toolbar toolbar;
			public TabBookmarks Tab;

			public Instance(TabBookmarks tab)
			{
				Tab = tab;
			}

			public override void LoadUI(Call call, TabModel model)
			{
				/*toolbar = new Toolbar();
				toolbar.ButtonReset.Action = Reset;
				model.AddObject(toolbar);*/

				model.AddData(Tab.Bookmarks.Items);
				if (Tab.Bookmarks.NewBookmark != null)
					SelectItem(Tab.Bookmarks.NewBookmark);
			}

			public override void GetBookmark(TabBookmark tabBookmark)
			{
				base.GetBookmark(tabBookmark);
				foreach (var child in tabBookmark.ChildBookmarks.Values)
					child.IsRoot = true;
			}

			private void Reset(Call call)
			{
				foreach (TabBookmarkItem item in SelectedItems)
				{
					SelectItem(item);
				}
			}
		}
	}
}
