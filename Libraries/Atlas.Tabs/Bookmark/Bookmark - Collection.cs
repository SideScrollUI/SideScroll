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
		public ItemCollection<ViewBookmarkName> Names { get; set; } = new ItemCollection<ViewBookmarkName>();

		public BookmarkCollection(Project project)
		{
			this.project = project;
			Reload();
		}

		public void Reload()
		{
			Names.Clear();
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
				ViewBookmarkName bookmarkName = new ViewBookmarkName(bookmark);
				Names.Add(bookmarkName);
			}
		}
	}
}
