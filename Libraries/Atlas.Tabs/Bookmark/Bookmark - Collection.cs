using Atlas.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Atlas.Tabs
{
	public class BookmarkCollection
	{
		//public event EventHandler<EventArgs> OnDelete;

		public string path;
		private Project project;
		public ItemCollection<TabBookmarkItem> Items { get; set; } = new ItemCollection<TabBookmarkItem>();

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

			var bookmarks = project.DataApp.LoadAllSorted<Bookmark>().Values;
			foreach (Bookmark bookmark in bookmarks)
			{
				if (bookmark.Name == TabInstance.CurrentBookmarkName)
					continue;
				Add(bookmark);
			}
		}

		public void Add(Bookmark bookmark)
		{
			var tabItem = new TabBookmarkItem(bookmark, project);
			tabItem.OnDelete += Item_OnDelete;
			Items.Add(tabItem);
		}

		public void AddNew(Call call, Bookmark bookmark)
		{
			//RemoveResult(tabSearchResult.Key); // Remove previous result
			project.DataApp.Save(bookmark.Address, bookmark, call);
			//dataRepoAccountResults?.Save(call, tabSearchResult.Key, tabSearchResult.Result);
			Add(bookmark);
		}

		private void Item_OnDelete(object sender, EventArgs e)
		{
			TabBookmarkItem bookmark = (TabBookmarkItem)sender;
			project.DataApp.Delete<Bookmark>(bookmark.Bookmark.Address);
			Items.Remove(bookmark);
			//Reload();
		}
	}
}
