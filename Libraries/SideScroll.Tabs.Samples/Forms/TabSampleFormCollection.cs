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
		public ToolButton ButtonSave { get; set; } = new ToolButton("Save", Icons.Svg.Save);
	}

	public class Instance : TabInstance
	{
		private const string DataKey = "Params";

		private ItemCollection<SampleItem> _items = [];
		private SampleItem? _sampleItem;
		private DataRepoInstance<SampleItem>? _dataRepoParams;

		public override void Load(Call call, TabModel model)
		{
			LoadSavedItems(call, model);

			_sampleItem = LoadData<SampleItem>(DataKey, true);
			model.AddForm(_sampleItem!);

			var toolbar = new Toolbar();
			toolbar.ButtonNew.Action = New;
			toolbar.ButtonSave.Action = Save;
			model.AddObject(toolbar);
		}

		private void LoadSavedItems(Call call, TabModel model)
		{
			_dataRepoParams = Data.App.Open<SampleItem>("CollectionTest");
			DataRepoInstance = _dataRepoParams;

			var sortedValues = _dataRepoParams.LoadAll(call).SortedValues;
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
			_dataRepoParams!.Save(call, clone.ToString(), clone);
			//SaveData(DataKey, clone);
			_items.Add(clone);
		}
	}
}
