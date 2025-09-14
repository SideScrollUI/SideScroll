using SideScroll.Attributes;
using SideScroll.Resources;
using SideScroll.Serialize;
using SideScroll.Serialize.DataRepos;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.Tabs.Samples.Forms;

[TabRoot, PublicData]
public class TabSampleFormDataTabs : ITab
{
	public override string ToString() => "Data Repos";

	public TabInstance Create() => new Instance();

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonNew { get; set; } = new("New", Icons.Svg.BlankDocument);
		public ToolButton ButtonSave { get; set; } = new("Save", Icons.Svg.Save, isDefault: true);
	}

	public class Instance : TabInstance
	{
		private const string GroupId = "SampleParams";
		private const string DataKey = "Default";

		private SampleItem? _sampleItem;
		private DataRepoView<SampleItem>? _dataRepoView;

		public override void Load(Call call, TabModel model)
		{
			LoadSavedItems(call, model);

			_sampleItem ??= LoadData<SampleItem>(DataKey) ?? SampleItem.CreateSample();
			model.AddForm(_sampleItem);

			Toolbar toolbar = new();
			toolbar.ButtonNew.Action = New;
			toolbar.ButtonSave.Action = Save;
			model.AddObject(toolbar);
		}

		private void LoadSavedItems(Call call, TabModel model)
		{
			_dataRepoView = Data.App.LoadView<SampleItem>(call, GroupId, nameof(SampleItem.Name));
			DataRepoInstance = _dataRepoView; // Allow links to pass the selected items

			var dataCollection = new DataViewCollection<SampleItem, TabSampleItem>(_dataRepoView);
			model.Items = dataCollection.Items;
		}

		private void New(Call call)
		{
			_sampleItem = new();
			Reload();
		}

		private void Save(Call call)
		{
			Validate();

			SampleItem clone = _sampleItem!.DeepClone(call);
			_dataRepoView!.Save(call, clone);
			SaveData(DataKey, clone);
		}
	}
}
