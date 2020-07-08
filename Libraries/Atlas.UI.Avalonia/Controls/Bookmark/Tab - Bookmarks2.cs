using Atlas.Core;
using Atlas.Resources;
using Atlas.Start.Avalonia.Tabs;
using Atlas.Tabs;

namespace Atlas.UI.Avalonia.Controls
{
	public class TabBookmarks2 : ITab
	{
		public static TabBookmarks2 Global;

		private Project project;
		private BookmarkCollection bookmarks;

		public TabBookmarks2(Project project)
		{
			this.project = project;
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
			private Toolbar toolbar;
			private TabBookmarks2 tab;

			public Instance(TabBookmarks2 tab)
			{
				this.tab = tab;
				//this.Project = tab.project;
				//Model.Name = "Bookmarks";
				//Model.Bookmarks = new BookmarkCollection(Project);
			}

			public override void LoadUI(Call call, TabModel model)
			{
				toolbar = new Toolbar();
				toolbar.ButtonReset.Action = Reset;
				model.AddObject(toolbar);

				model.AddData(tab.bookmarks.Items);
				if (tab.bookmarks.NewBookmark != null)
					SelectItem(tab.bookmarks.NewBookmark);

				/*foreach (var item in tabModel.Bookmarks.Items)
				{
					item.OnDelete += Item_OnDelete;
				}*/
			}

			// Overriding here would break navigation
			public override void GetBookmark(TabBookmark tabBookmark)
			{
				tabBookmark.IsRoot = true;
				base.GetBookmark(tabBookmark);
			}

			private void Reset(Call call)
			{
				foreach (TabBookmarkItem item in SelectedItems)
				{
					SelectItem(item);
				}
			}

			// move into BookmarkCollection?
			/*private void Item_OnDelete(object sender, EventArgs e)
			{
				TabBookmarkItem bookmark = (TabBookmarkItem)sender;
				project.DataApp.Delete<Bookmark>(null, bookmark.Bookmark.Name);
				tabModel.Bookmarks.Reload();
				tabModel.Bookmarks.Items.Insert(0, new TabBookmarkItem(currentBookMark));
				//tabModel.Bookmarks.Items.Remove(new TabBookmarkItem(bookmark));
				//Reload();
			}*/
		}
	}
}
