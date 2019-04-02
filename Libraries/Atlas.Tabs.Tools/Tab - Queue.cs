using System;
using System.Collections.Generic;
using System.Threading;
using Atlas.Core;
using System.Collections;
using Atlas.Tabs;

namespace Atlas.Tabs.Tools
{
	public class TabQueue : ITab
	{
		public IList tasks;

		public TabQueue(IList tasks)
		{
			this.tasks = tasks;
		}

		public TabInstance Create() { return new Instance(this); }

		public class Instance : TabInstance
		{
			private TabQueue tab;

			//public IList waiting;
			int nextIndex = 0;

			public ListItem listItemTasks;
			//public ListItem listItemActive;
			//public ListItem listItemFinished;

			public TaskDelegate listItemStart;
			public TaskDelegate listItemStop;
			public ListItem listItemLog;

			private CancellationTokenSource tokenSource;

			public Instance(TabQueue tab)
			{
				this.tab = tab;
			}

			public override void Load(Call call)
			{
				ItemCollection<ListItem> items = new ItemCollection<ListItem>();
				listItemTasks = new ListItem("Tasks", tab.tasks);

				items.Add(listItemTasks);
				//list.Add(listItemActive);
				//list.Add(listItemFinished);

				tabModel.Items = items;

				ItemCollection<TaskCreator> actions = new ItemCollection<TaskCreator>();
				listItemStart = new TaskDelegate("Start", Start);
				listItemStop = new TaskDelegate("Stop", Stop);
				actions.Add(listItemStart);
				actions.Add(listItemStop);

				tabModel.Actions = actions;
			}

			public void Start(Call call)
			{
				tokenSource = new CancellationTokenSource();

				while (nextIndex < tab.tasks.Count)
				{
					if (tokenSource.IsCancellationRequested)
						break;

					TaskCreator taskCreator = (TaskCreator)tab.tasks[nextIndex];
					nextIndex++;

					//waiting.RemoveAt(0);
					//active.Add(listTask);
					TaskInstance childTaskInstance = taskCreator.Start(call);
					childTaskInstance.Task.Wait(tokenSource.Token);
					//childTaskInstance.Task.ContinueWith((taskFinished => TaskFinished(call)));

					//listView.SelectedItems = new List<ListItem> { listItemWaiting, listItemLog };
				}
			}

			/*public void TaskFinished(Call call)
			{
				if (tokenSource.IsCancellationRequested)
					return;

				StartOne(call);
			}*/

			public void Stop(Call call)
			{
				tokenSource.Cancel();
			}
		}
	}
}

/*
should we change this to create multiple TabDatas?
*/