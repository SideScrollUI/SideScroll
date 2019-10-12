using System;
using System.Collections.Generic;
using System.Threading;
using Atlas.Core;
using Atlas.Tabs;

namespace Atlas.Tabs.Test
{
	public class TabTestLog : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			private Call sampleCall;
			private int counter = 0;

			public override void Load(Call call)
			{
				taskInstance = new TaskInstance();
				taskInstance.log.Add("Double Tag Test", new Tag("Double", 0.5));
				sampleCall = new Call(Label);
				counter = 0;

				tabModel.Items = new ItemCollection<ListItem>()
				{
					new ListItem("Task Instance Log", taskInstance.log),
					new ListItem("Sample Call", sampleCall),
					new ListItem("Sample Call Log", sampleCall.log),
					new ListItem("Log Entry", new LogEntry(LogEntry.LogType.Info, "test", null)),
				};

				tabModel.Actions = new ItemCollection<TaskCreator>()
				{
					new TaskAction("Add 1 Entry", new Action(() => AddEntries(1))),
					new TaskAction("Add 10 Entries", new Action(() => AddEntries(10))),
					new TaskAction("Add 100 Entries", new Action(() => AddEntries(100))),
					new TaskAction("Add 1,000 Entries", new Action(() => AddEntries(1000))),
					new TaskAction("Add 10,000 Entries", new Action(() => AddEntries(10000))),
					new TaskDelegate("Reset", Reset),
					// Tests different threading contexts
					new TaskAction("System.Timer: Log 1 Entry / second", new Action(() => StartSystemTimer())),
					new TaskAction("Threading.Timer: Log 1 Entry / second", new Action(() => StartThreadTimer())),
					new TaskDelegate("Task Delegate Thread:  Log 1 Entry / second", SubTaskInstances, true),
				};
				//actions.Add(new TaskAction("Add Child Entry", new Action(() => AddChildEntry())));
			}

			private void Reset(Call call)
			{
				base.Reintialize(true);
			}

			private void SubTaskInstances(Call call)
			{
				Log logChild = call.log.Call("child");
				CancellationToken token = call.taskInstance.tokenSource.Token;
				for (int i = 0; !token.IsCancellationRequested; i++)
				{
					//log.Add("New Log Entry", new Tag("i", counter));
					call.log.Add("New Call Log Entry", new Tag("i", counter));
					sampleCall.log.Add("New Sample Log Entry", new Tag("counter", counter));
					logChild.Add("New Child Log Entry", new Tag("i", i));
					counter++;
					System.Threading.Thread.Sleep(1000);
				}
			}

			private void AddEntries(int count)
			{
				for (int i = 0; i < count; i++)
				{
					//log.Add("test " + counter.ToString());
					counter++;
					//call.log.Add("New Log entry", new Tag("name", "value"));
					sampleCall.log.Add("New Log entry", new Tag("name", "value"));
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

			private Timer timer;
			private void StartThreadTimer()
			{
				// would be nice to be able cancel this through the task (Start/Stop) Methods
				if (timer == null)
				{
					timer = new Timer(TimerCallback, null, 0, 1000);
				}
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
