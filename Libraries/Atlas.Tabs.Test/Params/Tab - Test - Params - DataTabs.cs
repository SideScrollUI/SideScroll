using Atlas.Core;
using Atlas.Resources;
using Atlas.Serialize;

namespace Atlas.Tabs.Test
{
	public class TabTestParamsDataTabs : ITab
	{
		public TabInstance Create() => new Instance();

		public class Toolbar : TabToolbar
		{
			public ToolButton ButtonNew { get; set; } = new ToolButton("New", Icons.Streams.BlankDocument);
			public ToolButton ButtonSave { get; set; } = new ToolButton("Save", Icons.Streams.Save);
		}

		public class Instance : TabInstance
		{
			private const string dataKey = "Params";
			private ParamTestItem paramTestItem;
			private DataRepoView<ParamTestItem> dataRepoParams;

			public override void Load(Call call, TabModel model)
			{
				LoadSavedItems(call, model);

				/*model.Actions = new List<TaskCreator>()
				{
					new TaskDelegate("Add", Add),
				};*/

				paramTestItem = LoadData<ParamTestItem>(dataKey);
				model.AddObject(paramTestItem);

				var toolbar = new Toolbar();
				toolbar.ButtonNew.Action = New;
				toolbar.ButtonSave.Action = Save;
				model.AddObject(toolbar);
			}

			private void LoadSavedItems(Call call, TabModel model)
			{
				dataRepoParams = DataApp.OpenView<ParamTestItem>(call, "CollectionTest");
				DataRepoInstance = dataRepoParams;

				var dataCollection = new DataCollection<ParamTestItem, TabParamItem>(dataRepoParams);
				model.Items = dataCollection.Items;
			}

			private void New(Call call)
			{
			}

			private void Save(Call call)
			{
				var clone = paramTestItem.DeepClone<ParamTestItem>(call);
				dataRepoParams.Save(call, clone.ToString(), clone);
			}
		}
	}
}
