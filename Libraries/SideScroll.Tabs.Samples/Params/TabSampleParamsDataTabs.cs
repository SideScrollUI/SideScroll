using SideScroll;
using SideScroll.Resources;
using SideScroll.Serialize;
using SideScroll.Serialize.DataRepos;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.Tabs.Samples.Params;

public class TabSampleParamsDataTabs : ITab
{
	public override string ToString() => "Data Repos";

	public TabInstance Create() => new Instance();

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonNew { get; set; } = new ToolButton("New", Icons.Svg.BlankDocument);
		public ToolButton ButtonSave { get; set; } = new ToolButton("Save", Icons.Svg.Save);
	}

	public class Instance : TabInstance
	{
		private const string DataKey = "Params";

		private SampleParamItem? _paramTestItem;
		private DataRepoView<SampleParamItem>? _dataRepoParams;

		public override void Load(Call call, TabModel model)
		{
			LoadSavedItems(call, model);

			_paramTestItem = LoadData<SampleParamItem>(DataKey);
			model.AddObject(_paramTestItem!);

			var toolbar = new Toolbar();
			toolbar.ButtonNew.Action = New;
			toolbar.ButtonSave.Action = Save;
			model.AddObject(toolbar);
		}

		private void LoadSavedItems(Call call, TabModel model)
		{
			_dataRepoParams = DataApp.LoadView<SampleParamItem>(call, "DataRepoTest", nameof(SampleParamItem.Name));
			DataRepoInstance = _dataRepoParams;

			var dataCollection = new DataViewCollection<SampleParamItem, TabSampleParamItem>(_dataRepoParams);
			model.Items = dataCollection.Items;
		}

		private void New(Call call)
		{
			Reload();
		}

		private void Save(Call call)
		{
			Validate();

			SampleParamItem clone = _paramTestItem.DeepClone(call)!;
			_dataRepoParams!.Save(call, clone.ToString(), clone);
			SaveData(DataKey, clone);
		}
	}
}
