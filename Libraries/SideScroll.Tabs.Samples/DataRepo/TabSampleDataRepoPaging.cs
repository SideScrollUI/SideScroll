using SideScroll.Attributes;
using SideScroll.Collections;
using SideScroll.Resources;
using SideScroll.Serialize.DataRepos;
using SideScroll.Tabs.Toolbar;
using SideScroll.Tasks;

namespace SideScroll.Tabs.Samples.DataRepo;

public class TabSampleDataRepoPaging : ITab
{
	public TabInstance Create() => new Instance();

	private class Toolbar : TabToolbar
	{
		public ToolButton ButtonPrevious { get; } = new("Previous", Icons.Svg.LeftArrow);
		public ToolButton ButtonNext { get; } = new("Next", Icons.Svg.RightArrow);
	}

	private class Instance : TabInstance
	{
		private const string RepoId = "PagingRepo";

		private ItemCollectionUI<SampleItem>? _sampleItems;
		private DataRepoInstance<SampleItem>? _dataRepoItems;
		private DataPageView<SampleItem>? _pageView;

		public override void Load(Call call, TabModel model)
		{
			LoadPageView(call);
			model.Items = _sampleItems;

			Toolbar toolbar = new();
			toolbar.ButtonPrevious.Action = LoadPrevious;
			toolbar.ButtonPrevious.IsEnabledBinding = new PropertyBinding(nameof(IDataPageView.HasPrevious), _pageView);
			toolbar.ButtonNext.Action = LoadNext;
			toolbar.ButtonNext.IsEnabledBinding = new PropertyBinding(nameof(IDataPageView.HasNext), _pageView);
			model.AddObject(toolbar);

			model.Actions = new List<TaskCreator>
			{
				new TaskDelegate("Add", Add), // Foreground task so we can modify collection
				new TaskDelegate("Add 10", Add10),
				new TaskDelegate("Replace", Replace),
				new TaskDelegate("Delete", Delete),
				new TaskDelegate("Delete All", DeleteAll),
				new TaskDelegate("Load All", LoadAll),
			};
		}

		private void LoadPageView(Call call)
		{
			DataRepoInstance = _dataRepoItems = Data.App.Open<SampleItem>(RepoId, true);

			_pageView = _dataRepoItems.LoadPageView(call);
			_pageView!.PageSize = 10;
			_sampleItems = new ItemCollectionUI<SampleItem>(_pageView?.Next(call).Select(d => d.Value) ?? []);
		}

		private void LoadPrevious(Call call)
		{
			var items = new ItemCollectionUI<SampleItem>(_pageView?.Previous(call).Select(d => d.Value) ?? []);
			_sampleItems!.Replace(items);
		}

		private void LoadNext(Call call)
		{
			var items = new ItemCollectionUI<SampleItem>(_pageView?.Next(call).Select(d => d.Value) ?? []);
			_sampleItems!.Replace(items);
		}

		private void Add(Call call)
		{
			int index = _sampleItems?.LastOrDefault()?.Id + 1 ?? 1;
			var sampleItem = new SampleItem(index, "Item " + index);
			RemoveItem(call, sampleItem.Name!); // Remove previous result so refocus works
			_dataRepoItems!.Save(call, sampleItem.ToString()!, sampleItem);
			_sampleItems!.Add(sampleItem);
		}

		private void Add10(Call call)
		{
			for (int i = 0; i < 10; i++)
			{
				Add(call);
			}
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
			// Can't modify SelectedItems while iterating so create a copy
			List<SampleItem> selectedItems = SelectedItems!
				.Cast<SampleItem>()
				.ToList();

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

		private void LoadAll(Call call)
		{
			_dataRepoItems!.LoadAllDataItems(call);
			var dataItemCollection = _dataRepoItems.LoadAll(call);
			_sampleItems!.Replace(dataItemCollection.Values);
		}

		private void RemoveItem(Call call, string key)
		{
			_dataRepoItems!.Delete(call, key);
			SampleItem? existing = _sampleItems!.SingleOrDefault(i => i.Name == key);
			if (existing != null)
			{
				_sampleItems!.Remove(existing);
			}
		}
	}

	private class SampleItem
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
