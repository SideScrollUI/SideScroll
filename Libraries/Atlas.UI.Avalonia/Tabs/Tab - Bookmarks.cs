using Atlas.Core;
using Atlas.Resources;
using Atlas.Tabs;

namespace Atlas.UI.Avalonia.Controls
{
	public class TabBookmarks : ITab
	{
		public static TabBookmarks Global;

		private BookmarkCollection bookmarks;

		public TabBookmarks(Project project)
		{
			bookmarks = new BookmarkCollection(project);
			Global = Global ?? this;
		}

		public void AddBookmark(Call call, Bookmark bookmark)
		{
			bookmarks.AddNew(call, bookmark);
		}

		public TabInstance Create() => new Instance(this);

		public class Toolbar : TabToolbar
		{
			public ToolButton ButtonReset { get; set; } = new ToolButton("Reset", Icons.Streams.Refresh);
		}

		public class Instance : TabInstance
		{
			//private Toolbar toolbar;
			private TabBookmarks tab;

			public Instance(TabBookmarks tab)
			{
				this.tab = tab;
			}

			public override void LoadUI(Call call, TabModel model)
			{
				/*toolbar = new Toolbar();
				toolbar.ButtonReset.Action = Reset;
				model.AddObject(toolbar);*/

				model.AddData(tab.bookmarks.Items);
				if (tab.bookmarks.NewBookmark != null)
					SelectItem(tab.bookmarks.NewBookmark);
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
