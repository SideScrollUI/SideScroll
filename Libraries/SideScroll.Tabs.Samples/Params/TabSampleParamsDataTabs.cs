using SideScroll.Attributes;
using SideScroll.Resources;
using SideScroll.Serialize;
using SideScroll.Serialize.DataRepos;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.Tabs.Samples.Params;

[TabRoot, PublicData]
public class TabSampleParamsDataTabs : ITab
{
	public override string ToString() => "Data Repos";

	public TabInstance Create() => new Instance();

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonNew { get; set; } = new("New", Icons.Svg.BlankDocument);
		public ToolButton ButtonSave { get; set; } = new("Save", Icons.Svg.Save);
	}

	public class Instance : TabInstance
	{
		private const string GroupId = "SampleParams";
		private const string DataKey = "Params";

		private SampleParamItem? _sampleParamItem;
		private DataRepoView<SampleParamItem>? _dataRepoView;

		public override void Load(Call call, TabModel model)
		{
			LoadSavedItems(call, model);

			_sampleParamItem ??= LoadData<SampleParamItem>(DataKey);
			model.AddObject(_sampleParamItem!);

			Toolbar toolbar = new();
			toolbar.ButtonNew.Action = New;
			toolbar.ButtonSave.Action = Save;
			model.AddObject(toolbar);
		}

		private void LoadSavedItems(Call call, TabModel model)
		{
			_dataRepoView = Data.App.LoadView<SampleParamItem>(call, GroupId, nameof(SampleParamItem.Name));
			DataRepoInstance = _dataRepoView; // Allow links to pass the selected items

			var dataCollection = new DataViewCollection<SampleParamItem, TabSampleParamItem>(_dataRepoView);
			model.Items = dataCollection.Items;
		}

		private void New(Call call)
		{
			_sampleParamItem = new();
			Reload();
		}

		private void Save(Call call)
		{
			Validate();

			SampleParamItem clone = _sampleParamItem.DeepClone(call)!;
			_dataRepoView!.Save(call, clone.ToString(), clone);
			SaveData(DataKey, clone);
		}
	}
}
