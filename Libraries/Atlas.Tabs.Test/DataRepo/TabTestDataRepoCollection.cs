using Atlas.Core;
using Atlas.Serialize;
using System.Collections.Generic;
using System.Linq;

namespace Atlas.Tabs.Test.DataRepo
{
	public class TabTestDataRepoCollection : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			private const string RepoId = "TestRepo";

			private ItemCollection<SampleItem> _sampleItems;
			private DataRepoInstance<SampleItem> _dataRepoItems;

			public override void Load(Call call, TabModel model)
			{
				LoadSavedItems(call);
				model.Items = _sampleItems;

				model.Actions = new List<TaskCreator>()
				{
					new TaskDelegate("Add", Add), // Foreground task so we can modify collection
					new TaskDelegate("Add 10", Add10),
					new TaskDelegate("Replace", Replace),
					new TaskDelegate("Delete", Delete),
					new TaskDelegate("Delete All", DeleteAll),
				};

				//tabModel.Notes = "Data Repos store C# objects as serialized data.";
			}

			private void LoadSavedItems(Call call)
			{
				DataRepoInstance = _dataRepoItems = DataApp.Open<SampleItem>(RepoId);

				var dataRefs = _dataRepoItems.LoadAllSorted(call);
				_sampleItems = new ItemCollection<SampleItem>(dataRefs.Values);
			}

			private void Add(Call call)
			{
				var sampleItem = new SampleItem(_sampleItems.Count, "Item " + _sampleItems.Count);
				RemoveItem(sampleItem.Name); // Remove previous result so refocus works
				_dataRepoItems.Save(call, sampleItem.ToString(), sampleItem);
				_sampleItems.Add(sampleItem);
			}

			private void Add10(Call call)
			{
				for (int i = 0; i < 10; i++)
					Add(call);
			}

			private void Replace(Call call)
			{
				var sampleItem = new SampleItem(_sampleItems.Count, "Item 0");
				RemoveItem(sampleItem.Name); // Remove previous result so refocus works
				_dataRepoItems.Save(call, sampleItem.ToString(), sampleItem);
				_sampleItems.Add(sampleItem);
			}

			private void Delete(Call call)
			{
				// can't modify SelectedItems while iterating so create a copy, find better way
				var selectedItems = new List<SampleItem>();
				foreach (SampleItem item in SelectedItems)
				{
					selectedItems.Add(item);
				}

				foreach (SampleItem item in selectedItems)
				{
					RemoveItem(item.Name);
				}
			}

			private void DeleteAll(Call call)
			{
				_dataRepoItems.DeleteAll();
				_sampleItems.Clear();
			}

			public void RemoveItem(string key)
			{
				_dataRepoItems.Delete(key);
				SampleItem existing = _sampleItems.SingleOrDefault(i => i.Name == key);
				if (existing != null)
					_sampleItems.Remove(existing);
			}
		}

		public class SampleItem
		{
			[DataKey]
			public int Id { get; set; }
			public string Name { get; set; }

			public override string ToString() => Name;

			public SampleItem()
			{
			}

			public SampleItem(int id, string name)
			{
				Id = id;
				Name = name;
			}
		}
	}
}
