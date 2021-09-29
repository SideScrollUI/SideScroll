using Atlas.Core;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Atlas.Tabs.Test
{
	public class TabTestLog : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			private Call _sampleCall;
			private int _counter = 0;

			public override void Load(Call call, TabModel model)
			{
				TaskInstance = new TaskInstance();
				TaskInstance.Log.Add("Double Tag Test", new Tag("Double", 0.5));

				_sampleCall = new Call(Label);
				_counter = 0;

				model.Items = new ItemCollection<ListItem>()
				{
					new ListItem("Task Instance Log", TaskInstance.Log),
					new ListItem("Sample Call", _sampleCall),
					new ListItem("Sample Call Log", _sampleCall.Log),
					new ListItem("Log Entry", new LogEntry(null, LogLevel.Info, "test", null)),
				};

				model.Actions = new List<TaskCreator>()
				{
					new TaskAction("Add 1 Entry", () => AddEntries(1)),
					new TaskAction("Add 10 Entries",() => AddEntries(10)),
					new TaskAction("Add 100 Entries", () => AddEntries(100)),
					new TaskAction("Add 1,000 Entries", () => AddEntries(1000)),
					new TaskAction("Add 10,000 Entries", () => AddEntries(10000)),
					new TaskDelegate("Reset", Reset),
					// Tests different threading contexts
					new TaskAction("System.Timer: Log 1 Entry / second", () => StartSystemTimer()),
					new TaskAction("Threading.Timer: Log 1 Entry / second", () => StartThreadTimer()),
					new TaskDelegate("Task Delegate Thread:  Log 1 Entry / second", SubTaskInstances, true),
				};
				//actions.Add(new TaskAction("Add Child Entry", () => AddChildEntry()));
			}

			private void Reset(Call call)
			{
				Reintialize(true);
			}

			private void SubTaskInstances(Call call)
			{
				Log logChild = call.Log.Call("child");
				CancellationToken token = call.TaskInstance.TokenSource.Token;
				for (int i = 0; !token.IsCancellationRequested; i++)
				{
					//log.Add("New Log Entry", new Tag("i", counter));
					call.Log.Add("New Call Log Entry", new Tag("i", _counter));
					_sampleCall.Log.Add("New Sample Log Entry", new Tag("counter", _counter));
					logChild.Add("New Child Log Entry", new Tag("i", i));
					_counter++;
					Thread.Sleep(1000);
				}
			}

			private void AddEntries(int count)
			{
				for (int i = 0; i < count; i++)
				{
					//log.Add("test " + counter.ToString());
					_counter++;
					//call.Log.Add("New Log entry", new Tag("name", "value"));
					_sampleCall.Log.Add("New Log entry", new Tag("name", "value"));
				}
			}

			private void StartSystemTimer()
			{
				// would be nice to be able cancel this through the task (Start/Stop) Methods
				//if (systemTimer == null)
				{
					var systemTimer = new System.Timers.Timer();
					systemTimer.Interval = 1000;
					systemTimer.Elapsed += SystemTimer_Elapsed;
					systemTimer.Start();
				}
			}

			private Timer _timer;
			private void StartThreadTimer()
			{
				// would be nice to be able cancel this through the task (Start/Stop) Methods
				_timer = _timer ?? new Timer(TimerCallback, null, 0, 1000);
			}

			public void TimerCallback(object state)
			{
				AddEntries(1);
			}

			private void SystemTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
			{
				AddEntries(1);
			}

			private void AddChildEntry()
			{
				//log.items[log.items.Count - 1].Add("Child Message");
			}
		}
	}
}
