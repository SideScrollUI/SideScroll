using Atlas.Core;
using Atlas.Resources;
using Atlas.Serialize;

namespace Atlas.Tabs.Test
{
	public class TabTestParamsCollection : ITab
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
			private ItemCollection<ParamTestItem> items;
			private ParamTestItem paramTestItem;
			private DataRepoInstance<ParamTestItem> dataRepoParams;

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
				dataRepoParams = DataApp.Open<ParamTestItem>(call, "CollectionTest");
				DataRepoInstance = dataRepoParams;
				items = new ItemCollection<ParamTestItem>();
				var dataRefs = dataRepoParams.LoadAllSorted(call);
				foreach (var dataRef in dataRefs)
				{
					items.Add(dataRef.Value);
				}
				model.Items = items;
			}

			private void New(Call call)
			{
			}

			private void Save(Call call)
			{
				var clone = paramTestItem.Clone<ParamTestItem>(call);
				dataRepoParams.Save(call, clone.ToString(), clone);
				//SaveData(dataKey, paramTestItem);
				/*var result = new ParamTestResult()
				{
					parameters = clone,
				};*/
				items.Add(clone);
			}
		}
	}
}
