using SideScroll.Logs;
using SideScroll.Tabs.Lists;
using SideScroll.Tasks;

namespace SideScroll.Tabs.Samples;

public class TabSampleLogs : ITab
{
	public TabInstance Create() => new Instance();

	private class Instance : TabInstance
	{
		private Call? _sampleCall;
		private int _counter;

		public override void Load(Call call, TabModel model)
		{
			_sampleCall = call.AddSubCall(Label);
			_counter = 0;

			model.Items = new List<ListItem>
			{
				new("Sample Call Log", _sampleCall.Log),
				new("Sample Call", _sampleCall),
				new("Log Entry", new LogEntry(null, LogLevel.Info, "test", null)),
			};

			model.Actions =
			[
				new TaskAction("Add 1 Entry", () => AddEntries(1)),
				new TaskAction("Add 10 Entries",() => AddEntries(10)),
				new TaskAction("Add 100 Entries", () => AddEntries(100)),
				new TaskAction("Add 1,000 Entries", () => AddEntries(1_000)),
				new TaskAction("Add 10,000 Entries", () => AddEntries(10_000)),
				new TaskDelegate("Reset", Reset),
				new TaskDelegate("Sync Task Delegate Thread: Log 1 Entry / second", SyncTaskThread, true, true),
				new TaskDelegateAsync("Async Task Delegate Thread: Log 1 Entry / second", ASyncTaskThreadAsync, true, true),
			];
		}

		private void AddEntries(int count)
		{
			for (int i = 0; i < count; i++)
			{
				_counter++;
				_sampleCall!.Log.Add("New Log entry", new Tag("Id", _counter));
			}
		}

		private void Reset(Call call)
		{
			Reinitialize(true);
		}

		private void SyncTaskThread(Call call)
		{
			Log logChild = call.Log.AddChild("Child");
			CancellationToken cancelToken = call.TaskInstance!.CancelToken;
			for (int i = 0; i < 60 && !cancelToken.IsCancellationRequested; i++)
			{
				AddNextEntry(call, logChild);
				Thread.Sleep(1000);
			}
		}

		private async Task ASyncTaskThreadAsync(Call call)
		{
			Log logChild = call.Log.AddChild("Child");
			CancellationToken token = call.TaskInstance!.CancelToken;
			for (int i = 0; i < 60 && !token.IsCancellationRequested; i++)
			{
				AddNextEntry(call, logChild);
				await Task.Delay(1000);
			}
		}

		private void AddNextEntry(Call call, Log logChild)
		{
			var tag = new Tag("Counter", _counter);
			call.Log.Add("New Call Log Entry", tag);
			logChild.Add("New Child Log Entry", tag);
			_sampleCall!.Log.Add("New Sample Log Entry", tag);
			_counter++;
		}
	}
}
