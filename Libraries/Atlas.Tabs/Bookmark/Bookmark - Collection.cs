using Atlas.Core;
using Atlas.Serialize;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Atlas.Tabs
{
	public class BookmarkCollection
	{
		public static string DataKey = "Bookmarks";
		//public event EventHandler<EventArgs> OnDelete;

		private Project project;
		public ItemCollectionUI<TabBookmarkItem> Items { get; set; } = new ItemCollectionUI<TabBookmarkItem>();
		public TabBookmarkItem NewBookmark { get; set; }
		private DataRepoInstance<Bookmark> dataRepoBookmarks;

		public BookmarkCollection(Project project)
		{
			this.project = project;
			Reload();
			//Items.CollectionChanged += Items_CollectionChanged;
		}

		/*private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
		}*/

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

			dataRepoBookmarks = project.DataApp.Open<Bookmark>(null, DataKey);
			foreach (Bookmark bookmark in dataRepoBookmarks.LoadAllSorted().Values)
			{
				if (bookmark.Name == TabInstance.CurrentBookmarkName)
					continue;
				Add(bookmark);
			}
		}

		public TabBookmarkItem Add(Bookmark bookmark)
		{
			var tabItem = new TabBookmarkItem(bookmark, project);
			tabItem.OnDelete += Item_OnDelete;
			Items.Add(tabItem);
			return tabItem;
		}

		public void AddNew(Call call, Bookmark bookmark)
		{
			Remove(bookmark.Address); // Remove previous bookmark
			dataRepoBookmarks.Save(call, bookmark.Address, bookmark);
			NewBookmark = Add(bookmark);
		}

		private void Item_OnDelete(object sender, EventArgs e)
		{
			TabBookmarkItem bookmark = (TabBookmarkItem)sender;
			dataRepoBookmarks.Delete(bookmark.Bookmark.Address);
			Items.Remove(bookmark);
			//Reload();
		}

		public void Remove(string key)
		{
			dataRepoBookmarks.Delete(key);
			TabBookmarkItem existing = Items.SingleOrDefault(i => i.Name == key);
			if (existing != null)
				Items.Remove(existing);
		}
	}
}
