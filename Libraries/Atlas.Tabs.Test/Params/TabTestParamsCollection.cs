using Atlas.Core;
using Atlas.Resources;
using Atlas.Serialize;
using Atlas.Tabs.Toolbar;

namespace Atlas.Tabs.Test.Params;

public class TabTestParamsCollection : ITab
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

		private ItemCollection<ParamTestItem>? _items;
		private ParamTestItem? _paramTestItem;
		private DataRepoInstance<ParamTestItem>? _dataRepoParams;

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
			_dataRepoParams = DataApp.Open<ParamTestItem>("CollectionTest");
			DataRepoInstance = _dataRepoParams;

			var sortedValues = _dataRepoParams.LoadAll(call).SortedValues;
			_items = new ItemCollection<ParamTestItem>(sortedValues);
			model.Items = _items;
		}

		private void New(Call call)
		{
		}

		private void Save(Call call)
		{
			Validate();

			ParamTestItem clone = _paramTestItem.DeepClone(call)!;
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
