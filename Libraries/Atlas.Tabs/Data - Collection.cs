﻿using Atlas.Core;
using Atlas.Serialize;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Atlas.Tabs
{
	public interface IDataTab
	{
		void Load(Call call, object obj, params object[] tabParams);
		event EventHandler<EventArgs> OnDelete;
	}

	// An Item collection that shows a Tab interface for every item
	public class DataCollection<TDataType, TTabType> where TTabType : IDataTab, new()
	{
		//public static string DataKey = "Saved";
		//public event EventHandler<EventArgs> OnDelete;

		public string path;
		public ItemCollectionUI<TTabType> Items { get; set; } = new ItemCollectionUI<TTabType>();
		public TTabType NewTabItem { get; set; }
		private DataRepoView<TDataType> dataRepoView;
		private object[] tabParams;
		private Dictionary<TTabType, IDataItem> dataItemLookup;

		public DataCollection(DataRepoView<TDataType> dataRepoView, params object[] tabParams)
		{
			this.dataRepoView = dataRepoView;
			this.tabParams = tabParams;
			this.dataRepoView.Items.CollectionChanged += Items_CollectionChanged;
			Reload();
			//Items.CollectionChanged += Items_CollectionChanged;
		}

		private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				foreach (IDataItem item in e.NewItems)
				{
					Add(item);
				}
			}
			else if (e.Action == NotifyCollectionChangedAction.Remove)
			{
				foreach (IDataItem item in e.OldItems)
				{
					Remove(item.Key);
				}
			}
		}

		public void Reload()
		{
			Items.Clear();
			dataItemLookup = new Dictionary<TTabType, IDataItem>();

			//dataRepoBookmarks = project.DataApp.Open<TDataType>(null, DataKey);
			foreach (DataItem<TDataType> dataItem in dataRepoView.Items)
			{
				// for autoselecting?
				//if (bookmark.Name == TabInstance.CurrentBookmarkName)
				//	continue;
				Add(dataItem);
			}
		}

		public TTabType Add(IDataItem dataItem)
		{
			var tabItem = new TTabType();
			tabItem.Load(new Call(), dataItem.Object, tabParams);
			tabItem.OnDelete += Item_OnDelete;
			Items.Add(tabItem);
			dataItemLookup.Add(tabItem, dataItem);
			return tabItem;
		}

		/*public TTabType Add(TDataType dataObject)
		{
			var tabItem = new TTabType();
			tabItem.Load(new Call(), dataObject);
			tabItem.OnDelete += Item_OnDelete;
			Items.Add(tabItem);
			return tabItem;
		}

		public void AddNew(Call call, TDataType dataObject)
		{
			Remove(dataObject.ToString()); // Remove previous version
			dataRepoInstance.Save(call, dataObject.ToString(), dataObject);
			NewTabItem = Add(dataObject);
		}*/

		private void Item_OnDelete(object sender, EventArgs e)
		{
			TTabType tab = (TTabType)sender;
			if (!dataItemLookup.TryGetValue(tab, out IDataItem dataItem))
				return;

			dataRepoView.Delete(dataItem.Key);
			Items.Remove(tab);
			//Reload();
		}

		public void Remove(string key)
		{
			dataRepoView.Delete(key);
			TTabType existing = Items.SingleOrDefault(i => i.ToString() == key);
			if (existing != null)
				Items.Remove(existing);
		}
	}
}
