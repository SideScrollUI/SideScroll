﻿using System;
using System.Collections.Generic;
using System.Linq;
using Atlas.Core;

//namespace Atlas.Tabs.Test.DataRepo // good idea?
namespace Atlas.Tabs.Test
{
	public class TabTestDataRepoCollection : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			private ItemCollection<SampleItem> sampleItems;
			private string saveDirectory = null;

			public override void Load(Call call)
			{
				LoadSavedItems();
				tabModel.Items = sampleItems;

				tabModel.Actions = new ItemCollection<TaskCreator>()
				{
					new TaskDelegate("Add", Add, false), // Foreground task so we can modify collection
					new TaskDelegate("Delete", Delete),
					new TaskDelegate("Delete All", DeleteAll), // Foreground task so we can modify collection
				};

				//tabModel.Notes = "Data Repos store C# objects as serialized data.";
			}

			private void LoadSavedItems()
			{
				sampleItems = new ItemCollection<SampleItem>();
				var dataRefs = DataApp.LoadAll<SampleItem>(taskInstance.call, saveDirectory);
				foreach (var dataRef in dataRefs)
				{
					sampleItems.Add(dataRef.Value);
				}
			}

			private void Clear(Call call)
			{
				Reload();
			}

			private void Add(Call call)
			{
				var sampleItem = new SampleItem(sampleItems.Count, "Item " + sampleItems.Count.ToString());
				RemoveResult(sampleItem.Name); // Remove previous result so refocus works
				SaveData(sampleItem.ToString(), sampleItem);
				sampleItems.Add(sampleItem);
			}

			private void Delete(Call call)
			{
				var selectedItems = new List<SampleItem>();
				foreach (SampleItem item in SelectedItems)
				{
					selectedItems.Add(item);
				}
				// can't modify SelectedItems while iterating
				foreach (SampleItem item in selectedItems)
				{
					//this.DataApp.Delete<SampleItem>(saveDirectory, item.Name);
					RemoveResult(item.Name);
				}
			}

			private void DeleteAll(Call call)
			{
				this.DataApp.DeleteAll<SampleItem>();
				sampleItems.Clear();
			}

			public void RemoveResult(string key)
			{
				DataApp.Delete<SampleItem>(saveDirectory, key);
				SampleItem existing = null;
				foreach (var searchResult in sampleItems)
				{
					if (searchResult.Name == key)
						existing = searchResult;
				}
				if (existing != null)
					sampleItems.Remove(existing);
			}
		}

		public class SampleItem
		{
			public int ID { get; set; }
			public string Name { get; set; }

			public SampleItem()
			{
			}

			public SampleItem(int id, string name)
			{
				this.ID = id;
				this.Name = name;
			}

			public override string ToString()
			{
				return Name;
			}
		}
	}

}
