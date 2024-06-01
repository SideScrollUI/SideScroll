using Atlas.Core;
using Atlas.Core.Tasks;
using Atlas.Serialize;

namespace Atlas.Tabs.Samples.Params;

public class TabSampleParamsTasks : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		private const string DataKey = "Params";

		private readonly ItemCollectionUI<ParamTestResult> _items = [];
		private SampleParamItem? _paramTestItem;

		public override void Load(Call call, TabModel model)
		{
			model.Items = _items;

			model.Actions = new List<TaskCreator>
			{
				new TaskDelegate("Add", Add),
				new TaskDelegateAsync("Task with Progress", ShowProgressAsync, true),
				new TaskDelegateAsync("Task with Sub Tasks", TaskCountAsync, true),
			};

			_paramTestItem = LoadData<SampleParamItem>(DataKey);
			if (_paramTestItem!.DateTime.Ticks == 0)
				_paramTestItem.DateTime = DateTime.Now; // in case the serializer loses it
			model.AddObject(_paramTestItem);

			model.Notes = "Adding a class of type [Params] to a tabModel creates a TabControlParam\nParameter values can be saved between Tasks";
		}

		private void Add(Call call)
		{
			Validate();

			SaveData(DataKey, _paramTestItem!);

			SampleParamItem clone = _paramTestItem.DeepClone(call)!;
			ParamTestResult result = new()
			{
				Parameters = clone,
			};
			_items.Add(result);
		}

		private static async Task ShowProgressAsync(Call call)
		{
			call.TaskInstance!.ProgressMax = 10;

			for (int i = 0; i < 10; i++)
			{
				await Task.Delay(1000);
				call.Log.Add("Slept 1 second");
				call.TaskInstance.Progress++;
			}
		}

		private static async Task TaskCountAsync(Call call)
		{
			call.TaskInstance!.TaskCount = 10;

			for (int i = 0; i < 10; i++)
			{
				using var taskCall = call.StartTask("Sleeping 1 second");
				await Task.Delay(1000);
				taskCall.Log.Add("Slept 1 second");
			}
		}
	}

	public class ParamTestResult
	{
		public SampleParamItem? Parameters;
		public string? Name => Parameters?.Name;
		public DateTime? DateTime => Parameters?.DateTime;

		public override string? ToString() => Name;
	}
}
