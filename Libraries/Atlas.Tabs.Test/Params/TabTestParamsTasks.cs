using Atlas.Core;
using Atlas.Serialize;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Atlas.Tabs.Test
{
	public class TabTestParamsTasks : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			private const string DataKey = "Params";

			private readonly ItemCollectionUI<ParamTestResult> _items = new ItemCollectionUI<ParamTestResult>();
			private ParamTestItem _paramTestItem;

			public override void Load(Call call, TabModel model)
			{
				model.Items = _items;

				model.Actions = new List<TaskCreator>()
				{
					new TaskDelegate("Add", Add),
					new TaskDelegateAsync("10s Task", LongTaskAsync, true),
				};

				_paramTestItem = LoadData<ParamTestItem>(DataKey);
				if (_paramTestItem.DateTime.Ticks == 0)
					_paramTestItem.DateTime = DateTime.Now; // in case the serializer loses it
				model.AddObject(_paramTestItem);

				model.Notes = "Adding a class of type [Params] to a tabModel creates a TabControlParam\nParameter values can be saved between Tasks";
			}

			private void Add(Call call)
			{
				SaveData(DataKey, _paramTestItem);

				ParamTestItem clone = _paramTestItem.DeepClone(call);
				var result = new ParamTestResult()
				{
					Parameters = clone,
				};
				_items.Add(result);
			}

			private async Task LongTaskAsync(Call call)
			{
				call.TaskInstance.ProgressMax = 10;

				for (int i = 0; i < 10; i++)
				{
					await Task.Delay(1000);
					call.Log.Add("Slept 1 second");
					call.TaskInstance.Progress++;
				}
			}
		}

		public class ParamTestResult
		{
			public ParamTestItem Parameters;
			public string String => Parameters.Name;
		}
	}
}
