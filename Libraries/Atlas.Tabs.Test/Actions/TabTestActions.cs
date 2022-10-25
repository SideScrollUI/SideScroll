using Atlas.Core;

namespace Atlas.Tabs.Test.Actions;

public class TabActions : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.MinDesiredWidth = 250;

			model.Items = new List<ListItem>()
			{
				new("Parameters", new TabParamsDataGrid()),
				new("Async Load", new TabTestLoadAsync()),
			};

			model.Actions = new List<TaskCreator>()
			{
				new TaskDelegate("Add Log Entry", AddEntry),
				new TaskDelegate("Test Exception", TestException, true, true, "Throws an exception"),
				new TaskDelegate("Parallel Task Progress", ParallelTaskProgress, true),
				new TaskDelegateAsync("Task Progress", SubTaskProgressAsync, true),
				new TaskAction("Action", () => PassParams(1, "abc")),
				new TaskDelegateAsync("Long load (Async)", SleepAsync, true),
				new TaskDelegate("StartAsync error", StartAsyncError),
			};

			model.Notes = @"
Actions add Buttons to the tab. When clicked, it will:
* Start a task that calls this action
* Add a Tasks grid to the tab
  - Add a new Task to that grid

* Tasks
";
		}

		private void StartAsyncError(Call call)
		{
			StartAsync(StartAsyncLogErrorAsync, call);
		}

		private async Task StartAsyncLogErrorAsync(Call call)
		{
			await Task.Delay(1);

			call.Log.AddError("This should show the task");
		}

		private static void PassParams(int param1, string param2)
		{
			Log log = new();
			log.Add("If you log and no one's listening, are you really logging?",
				new Tag("param1", param1),
				new Tag("param2", param2));
		}

		private int _counter = 1;
		private void AddEntry(Call call)
		{
			call.Log.Add("New Log entry", new Tag("counter", _counter++));
		}

		private void TestException(Call call)
		{
			throw new NotImplementedException();
		}

		private void ParallelTaskProgress(Call call)
		{
			var downloads = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
			Parallel.ForEach(downloads, new ParallelOptions() { MaxDegreeOfParallelism = 10 }, i =>
			{
				using CallTimer sleepCall = call.Timer(i.ToString());

				sleepCall.AddSubTask();
				sleepCall.TaskInstance!.ProgressMax = i;
				for (int j = 0; j < i; j++)
				{
					Thread.Sleep(1000);
					sleepCall.TaskInstance.Progress = j + 1;
				}
			});
		}

		private async Task SubTaskProgressAsync(Call call)
		{
			var ids = new List<int>();
			for (int i = 0; i < 30; i++)
				ids.Add(i);

			List<int> results = await call.RunAsync(DoTask, ids);
		}

		public static async Task<int> DoTask(Call call, int id)
		{
			using CallTimer callTimer = call.Timer("Task", new Tag(id));

			for (int i = 0; i < id && !callTimer.TaskInstance!.CancelToken.IsCancellationRequested; i++)
			{
				callTimer.Log.Add("Sleeping");
				await Task.Delay(1000, callTimer.TaskInstance.CancelToken);
			}

			return id;
		}

		private async Task SleepAsync(Call call)
		{
			using CallTimer callTimer = call.Timer("long op");

			await Task.Delay(1000);
		}
	}
}
