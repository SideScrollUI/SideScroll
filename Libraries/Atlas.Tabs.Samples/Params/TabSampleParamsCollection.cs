using Atlas.Core;
using Atlas.Resources;
using Atlas.Serialize;
using Atlas.Serialize.DataRepos;
using Atlas.Tabs.Toolbar;

namespace Atlas.Tabs.Samples.Params;

public class TabSampleParamsCollection : ITab
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

		private ItemCollection<SampleParamItem>? _items;
		private SampleParamItem? _paramTestItem;
		private DataRepoInstance<SampleParamItem>? _dataRepoParams;

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
			_dataRepoParams = DataApp.Open<SampleParamItem>("CollectionTest");
			DataRepoInstance = _dataRepoParams;

			var sortedValues = _dataRepoParams.LoadAll(call).SortedValues;
			_items = new ItemCollection<SampleParamItem>(sortedValues);
			model.Items = _items;
		}

		private void New(Call call)
		{
		}

		private void Save(Call call)
		{
			Validate();

			SampleParamItem clone = _paramTestItem.DeepClone(call)!;
			_dataRepoParams!.Save(call, clone.ToString(), clone);
			//SaveData(dataKey, paramTestItem);
			/*var result = new ParamTestResult()
			{
				parameters = clone,
			};*/
			_items!.Add(clone);
		}
	}
}
