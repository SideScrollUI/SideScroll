using SideScroll.Collections;
using SideScroll.Resources;
using SideScroll.Serialize;
using SideScroll.Serialize.DataRepos;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.Tabs.Samples.Forms;

public class TabSampleFormCollection : ITab
{
	public TabInstance Create() => new Instance();

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonNew { get; set; } = new ToolButton("New", Icons.Svg.BlankDocument);
		public ToolButton ButtonSave { get; set; } = new ToolButton("Save", Icons.Svg.Save, isDefault: true);
	}

	public class Instance : TabInstance
	{
		private const string DataKey = "FormCollection";

		private ItemCollection<SampleItem> _items = [];
		private SampleItem? _sampleItem;
		private DataRepoInstance<SampleItem>? _dataRepoInstance;

		public override void Load(Call call, TabModel model)
		{
			LoadSavedItems(call, model);

			_sampleItem ??= LoadData<SampleItem>(DataKey) ?? SampleItem.CreateSample();
			model.AddForm(_sampleItem);

			var toolbar = new Toolbar();
			toolbar.ButtonNew.Action = New;
			toolbar.ButtonSave.Action = Save;
			model.AddObject(toolbar);
		}

		private void LoadSavedItems(Call call, TabModel model)
		{
			_dataRepoInstance = Data.App.Open<SampleItem>("CollectionTest");
			DataRepoInstance = _dataRepoInstance;

			var sortedValues = _dataRepoInstance.LoadAll(call).SortedValues;
			_items = new ItemCollection<SampleItem>(sortedValues);
			model.Items = _items;
		}

		private void New(Call call)
		{
			_sampleItem = new();
			Reload();
		}

		private void Save(Call call)
		{
			Validate();

			SampleItem clone = _sampleItem.DeepClone(call)!;
			_dataRepoInstance!.Save(call, clone.ToString()!, clone);
			SaveData(DataKey, clone);
			_items.Add(clone);
		}
	}
}
