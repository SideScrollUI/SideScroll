using Atlas.Core;
using Atlas.Serialize;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Atlas.Tabs.Test
{
	public class TabTestParamsTasks : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			private const string dataKey = "Params";
			private ItemCollection<ParamTestResult> items = new ItemCollection<ParamTestResult>();
			private ParamTestItem paramTestItem;

			public override void Load(Call call, TabModel model)
			{
				model.Items = items;

				model.Actions = new List<TaskCreator>()
				{
					new TaskDelegate("Add", Add),
					new TaskDelegateAsync("10s Task", LongTaskAsync, true),
				};

				paramTestItem = LoadData<ParamTestItem>(dataKey);
				if (paramTestItem.DateTime.Ticks == 0)
					paramTestItem.DateTime = DateTime.Now; // in case the serializer loses it
				model.AddObject(paramTestItem);

				model.Notes = "Adding a class of type [Params] to a tabModel creates a TabControlParam\nParameter values can be saved between Tasks";
			}

			private void Add(Call call)
			{
				SaveData(dataKey, paramTestItem);
				var clone = paramTestItem.Clone<ParamTestItem>(call);
				var result = new ParamTestResult()
				{
					parameters = clone,
				};
				items.Add(result);
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
			public ParamTestItem parameters;
			public string String => parameters.String;
		}
	}
}
