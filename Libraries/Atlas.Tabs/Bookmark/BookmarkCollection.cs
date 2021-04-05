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

		public Project Project;
		public ItemCollectionUI<TabBookmarkItem> Items { get; set; } = new ItemCollectionUI<TabBookmarkItem>();
		public TabBookmarkItem NewBookmark { get; set; }

		public DataRepoView<Bookmark> DataRepoBookmarks;

		public BookmarkCollection(Project project)
		{
			Project = project;
			Reload();
			//Items.CollectionChanged += Items_CollectionChanged;
		}

		/*private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
		}*/

		public void Reload()
		{
			Items.Clear();

			DataRepoBookmarks = Project.DataApp.OpenView<Bookmark>(null, DataKey);
			DataRepoBookmarks.SortBy(nameof(Bookmark.TimeStamp));

			foreach (Bookmark bookmark in DataRepoBookmarks.Items.Values)
			{
				if (bookmark.Name == TabInstance.CurrentBookmarkName)
					continue;

				Add(bookmark);
			}
		}

		public TabBookmarkItem Add(Bookmark bookmark)
		{
			var tabItem = new TabBookmarkItem(bookmark, Project);
			tabItem.OnDelete += Item_OnDelete;
			Items.Add(tabItem);
			return tabItem;
		}

		public void AddNew(Call call, Bookmark bookmark)
		{
			Remove(bookmark.Address); // Remove previous bookmark
			DataRepoBookmarks.Save(call, bookmark.Address, bookmark);
			NewBookmark = Add(bookmark);
		}

		private void Item_OnDelete(object sender, EventArgs e)
		{
			TabBookmarkItem bookmark = (TabBookmarkItem)sender;
			DataRepoBookmarks.Delete(bookmark.Bookmark.Address);
			Items.Remove(bookmark);
			//Reload();
		}

		public void Remove(string key)
		{
			DataRepoBookmarks.Delete(key);
			TabBookmarkItem existing = Items.SingleOrDefault(i => i.Bookmark.Address == key);
			if (existing != null)
				Items.Remove(existing);
		}
	}
}
