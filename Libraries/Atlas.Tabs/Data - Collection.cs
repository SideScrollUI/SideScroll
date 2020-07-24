using Atlas.Core;
using Atlas.Serialize;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Atlas.Tabs
{
	public interface ILoadData
	{
		void Load(Call call, object obj);
	}

	// An Item collection that shows adds a Tab interface to every item
	public class DataCollection<TDataType, TTabType> where TTabType : ILoadData, new()
	{
		//public static string DataKey = "Saved";
		//public event EventHandler<EventArgs> OnDelete;

		public string path;
		//private Project project;
		public ItemCollectionUI<TTabType> Items { get; set; } = new ItemCollectionUI<TTabType>();
		public TTabType NewBookmark { get; set; }
		private DataRepoInstance<TDataType> dataRepoInstance;

		public DataCollection(DataRepoInstance<TDataType> dataRepoInstance)
		{
			this.dataRepoInstance = dataRepoInstance;
			//this.project = project;
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

			//dataRepoBookmarks = project.DataApp.Open<TDataType>(null, DataKey);
			foreach (TDataType bookmark in dataRepoInstance.LoadAllSorted().Values)
			{
				// for autoselecting?
				//if (bookmark.Name == TabInstance.CurrentBookmarkName)
				//	continue;
				Add(bookmark);
			}
		}

		public TTabType Add(TDataType dataObject)
		{
			var tabItem = new TTabType();
			//tabItem.OnDelete += Item_OnDelete;
			Items.Add(tabItem);
			return tabItem;
		}

		public void AddNew(Call call, TDataType dataObject)
		{
			Remove(dataObject.ToString()); // Remove previous bookmark
			dataRepoInstance.Save(call, dataObject.ToString(), dataObject);
			NewBookmark = Add(dataObject);
		}

		private void Item_OnDelete(object sender, EventArgs e)
		{
			TTabType bookmark = (TTabType)sender;
			dataRepoInstance.Delete(bookmark.ToString());
			Items.Remove(bookmark);
			//Reload();
		}

		public void Remove(string key)
		{
			dataRepoInstance.Delete(key);
			TTabType existing = Items.SingleOrDefault(i => i.ToString() == key);
			if (existing != null)
				Items.Remove(existing);
		}
	}
}
