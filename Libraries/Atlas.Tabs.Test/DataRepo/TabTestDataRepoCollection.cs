using Atlas.Core;
using Atlas.Serialize;

namespace Atlas.Tabs.Test.DataRepo;

public class TabTestDataRepoCollection : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		private const string RepoId = "TestRepo";

		private ItemCollection<SampleItem>? _sampleItems;
		private DataRepoInstance<SampleItem>? _dataRepoItems;

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

			var sortedValues = _dataRepoItems.LoadAll(call).SortedValues;
			_sampleItems = new ItemCollection<SampleItem>(sortedValues);
		}

		private void Add(Call call)
		{
			var sampleItem = new SampleItem(_sampleItems!.Count, "Item " + _sampleItems.Count);
			RemoveItem(call, sampleItem!.Name!); // Remove previous result so refocus works
			_dataRepoItems!.Save(call, sampleItem.ToString()!, sampleItem);
			_sampleItems.Add(sampleItem);
		}

		private void Add10(Call call)
		{
			for (int i = 0; i < 10; i++)
				Add(call);
		}

		private void Replace(Call call)
		{
			var sampleItem = new SampleItem(_sampleItems!.Count, "Item 0");
			RemoveItem(call, sampleItem.Name!); // Remove previous result so refocus works
			_dataRepoItems!.Save(call, sampleItem.ToString()!, sampleItem);
			_sampleItems.Add(sampleItem);
		}

		private void Delete(Call call)
		{
			// can't modify SelectedItems while iterating so create a copy, find better way
			List<SampleItem> selectedItems = new();
			foreach (SampleItem item in SelectedItems!)
			{
				selectedItems.Add(item);
			}

			foreach (SampleItem item in selectedItems)
			{
				RemoveItem(call, item.Name!);
			}
		}

		private void DeleteAll(Call call)
		{
			_dataRepoItems!.DeleteAll(call);
			_sampleItems!.Clear();
		}

		public void RemoveItem(Call call, string key)
		{
			_dataRepoItems!.Delete(call, key);
			SampleItem? existing = _sampleItems!.SingleOrDefault(i => i.Name == key);
			if (existing != null)
				_sampleItems!.Remove(existing);
		}
	}

	public class SampleItem
	{
		[DataKey]
		public int Id { get; set; }
		public string? Name { get; set; }

		public override string? ToString() => Name;

		public SampleItem() { }

		public SampleItem(int id, string name)
		{
			Id = id;
			Name = name;
		}
	}
}
