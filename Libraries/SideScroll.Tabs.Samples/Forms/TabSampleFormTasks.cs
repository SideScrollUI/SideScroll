using SideScroll.Collections;
using SideScroll.Serialize;
using SideScroll.Tasks;

namespace SideScroll.Tabs.Samples.Forms;

public class TabSampleFormTasks : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		private const string DataKey = "Params";

		private readonly ItemCollectionUI<SampleResult> _items = [];
		private SampleItem? _sampleItem;

		public override void Load(Call call, TabModel model)
		{
			model.Items = _items;

			model.Actions = new List<TaskCreator>
			{
				new TaskDelegate("Add", Add),
				new TaskDelegateAsync("Task with Progress", ShowProgressAsync, true),
				new TaskDelegateAsync("Task with Sub Tasks", TaskCountAsync, true),
			};

			_sampleItem = LoadData<SampleItem>(DataKey);
			if (_sampleItem!.DateTime.Ticks == 0)
			{
				_sampleItem.DateTime = DateTime.Now; // in case the serializer loses it
			}
			model.AddForm(_sampleItem);
		}

		private void Add(Call call)
		{
			Validate();

			SaveData(DataKey, _sampleItem!);

			SampleItem clone = _sampleItem.DeepClone(call)!;
			SampleResult result = new()
			{
				SampleItem = clone,
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

	public class SampleResult
	{
		public SampleItem? SampleItem;
		public string? Name => SampleItem?.Name;
		public DateTime? DateTime => SampleItem?.DateTime;

		public override string? ToString() => Name;
	}
}
