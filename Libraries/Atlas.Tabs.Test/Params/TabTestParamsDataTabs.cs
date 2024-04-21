using Atlas.Core;
using Atlas.Resources;
using Atlas.Serialize;

namespace Atlas.Tabs.Test.Params;

public class TabTestParamsDataTabs : ITab
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

		private ParamTestItem? _paramTestItem;
		private DataRepoView<ParamTestItem>? _dataRepoParams;

		public override void Load(Call call, TabModel model)
		{
			LoadSavedItems(call, model);

			_paramTestItem = LoadData<ParamTestItem>(DataKey);
			model.AddObject(_paramTestItem!);

			var toolbar = new Toolbar();
			toolbar.ButtonNew.Action = New;
			toolbar.ButtonSave.Action = Save;
			model.AddObject(toolbar);
		}

		private void LoadSavedItems(Call call, TabModel model)
		{
			_dataRepoParams = DataApp.LoadView<ParamTestItem>(call, "DataRepoTest", nameof(ParamTestItem.Name));
			DataRepoInstance = _dataRepoParams;

			var dataCollection = new DataViewCollection<ParamTestItem, TabParamItem>(_dataRepoParams);
			model.Items = dataCollection.Items;
		}

		private void New(Call call)
		{
			Reload();
		}

		private void Save(Call call)
		{
			Validate();

			ParamTestItem clone = _paramTestItem.DeepClone(call)!;
			_dataRepoParams!.Save(call, clone.ToString(), clone);
			SaveData(DataKey, clone);
		}
	}
}
