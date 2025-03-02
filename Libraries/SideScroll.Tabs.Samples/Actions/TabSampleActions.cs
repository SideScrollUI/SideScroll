using SideScroll.Logs;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Samples.Params;
using SideScroll.Tasks;

namespace SideScroll.Tabs.Samples.Actions;

public class TabSampleActions : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.MinDesiredWidth = 250;

			model.Items = new List<ListItem>
			{
				new("Parameters", new TabSampleParamsDataGrid()),
				new("Async Load", new TabSampleLoadAsync()),
			};

			model.Actions = new List<TaskCreator>
			{
				new TaskDelegate("Add Log Entry", AddEntry),
				new TaskDelegate("Test Exception", TestException, true, true, "Throws an exception"),
				new TaskDelegate("Parallel Task Progress", ParallelTaskProgress, true),
				new TaskDelegateAsync("Task Progress", SubTaskProgressAsync, true),
				new TaskDelegateAsync("Multi Level Progress", MultiLevelRunAsync, true),
				new TaskAction("Action", () => PassParams(1, "abc")),
				new TaskDelegateAsync("Long load (Async)", SleepAsync, true),
				new TaskDelegate("StartAsync error", StartAsyncError),
			};
		}

		private void StartAsyncError(Call call)
		{
			StartAsync(StartAsyncLogErrorAsync, call);
		}

		private static async Task StartAsyncLogErrorAsync(Call call)
		{
			await Task.Delay(10);

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

		private static void ParallelTaskProgress(Call call)
		{
			var downloads = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
			Parallel.ForEach(downloads, new ParallelOptions { MaxDegreeOfParallelism = 10 }, i =>
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

		private static async Task SubTaskProgressAsync(Call call)
		{
			List<int> ids = Enumerable.Range(0, 30).ToList();

			var results = await call.RunAsync(DoTask, ids);
		}

		private static async Task<int> DoTask(Call call, int id)
		{
			using CallTimer callTimer = call.Timer("Task", new Tag(id));

			for (int i = 0; i < id && !callTimer.TaskInstance!.CancelToken.IsCancellationRequested; i++)
			{
				callTimer.Log.Add("Sleeping");
				await Task.Delay(1000, callTimer.TaskInstance.CancelToken);
			}

			return id;
		}

		private static async Task MultiLevelRunAsync(Call call)
		{
			List<int> ids = Enumerable.Range(0, 20).ToList();

			var results = await call.RunAsync(MultiLevelRunIdAsync, ids, maxConcurrentRequests: 5);
		}

		private static async Task<int> MultiLevelRunIdAsync(Call call, int id)
		{
			List<int> ids = Enumerable.Range(0, 100).ToList();

			// Disable logging for high rates
			//call.Log.Settings = call.Log.Settings!.WithMinLogLevel(LogLevel.Warn);

			var results = await call.RunAsync(MultiLevelRunTaskAsync, ids, maxConcurrentRequests: 2);

			return id;
		}

		private static async Task<int> MultiLevelRunTaskAsync(Call call, int id)
		{
			call.Log.Add("Sleeping: " + id);
			await Task.Delay(100, call.TaskInstance!.CancelToken);

			return id;
		}

		private static async Task SleepAsync(Call call)
		{
			using CallTimer callTimer = call.Timer("long op");

			await Task.Delay(1000);
		}
	}
}
