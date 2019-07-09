using Atlas.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Atlas.Tabs
{
	public class BookmarkCollection
	{
		public string path;
		private Project project;
		public ItemCollection<TabBookmarkItem> Items { get; set; } = new ItemCollection<TabBookmarkItem>();

		public BookmarkCollection(Project project)
		{
			this.project = project;
			Reload();
		}

		public void Reload()
		{
			Items.Clear();
			// Add ID indices?
			/*ItemCollection<string> ids = project.DataShared.GetObjectIds(typeof(Bookmark));
			foreach (string id in ids)
			{
				if (id == TabInstance.CurrentBookmarkName)
					continue;
				BookmarkName bookmarkName = new BookmarkName(id);
				Names.Add(bookmarkName);
			}*/

			var bookmarks = project.DataApp.LoadAll<Bookmark>().Values;
			foreach (Bookmark bookmark in bookmarks)
			{
				if (bookmark.Name == TabInstance.CurrentBookmarkName)
					continue;
				TabBookmarkItem bookmarkItem = new TabBookmarkItem(bookmark);
				Items.Add(bookmarkItem);
			}
		}
	}
}
