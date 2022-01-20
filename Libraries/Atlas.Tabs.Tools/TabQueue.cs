using Atlas.Core;
using System.Collections;
using System.Threading;

namespace Atlas.Tabs.Tools;

// Unused
public class TabQueue : ITab
{
	public IList Tasks;

	public TabQueue(IList tasks)
	{
		Tasks = tasks;
	}

	public TabInstance Create() => new Instance(this);

	public class Instance : TabInstance
	{
		public TabQueue Tab;

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
			Tab = tab;
		}

		public override void Load(Call call, TabModel model)
		{
			var items = new ItemCollection<ListItem>();
			listItemTasks = new ListItem("Tasks", Tab.Tasks);

			items.Add(listItemTasks);
			//list.Add(listItemActive);
			//list.Add(listItemFinished);

			model.Items = items;

			var actions = new ItemCollection<TaskCreator>();
			listItemStart = new TaskDelegate("Start", Start);
			listItemStop = new TaskDelegate("Stop", Stop);
			actions.Add(listItemStart);
			actions.Add(listItemStop);

			model.Actions = actions;
		}

		public void Start(Call call)
		{
			tokenSource = new CancellationTokenSource();

			while (nextIndex < Tab.Tasks.Count)
			{
				if (tokenSource.IsCancellationRequested)
					break;

				TaskCreator taskCreator = (TaskCreator)Tab.Tasks[nextIndex];
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

/*
should we change this to create multiple TabDatas?
*/
