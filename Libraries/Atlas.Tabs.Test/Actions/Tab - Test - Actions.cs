using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Atlas.Core;
using Atlas.Tabs;

namespace Atlas.Tabs.Test.Actions
{
	public class TabActions : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			//private ItemCollection<ListItem> items;

			public override void Load()
			{
				tabModel.Notes = "";
				tabModel.Items = new ItemCollection<ListItem>()
				{
					new ListItem("Parameters", new TabParamsDataGrid()),
					new ListItem("Async", new TabTestAsync()),
				};

				tabModel.Actions = new ItemCollection<TaskCreator>()
				{
					new TaskDelegate("Add Log Entry", AddEntry),
					new TaskDelegate("Test Exception", TestException, true, "Throws an exception"),
					new TaskDelegate("Task Instance Progress", SubTaskInstances, true),
					new TaskAction("Action", new Action(() => PassParams(1, "abc"))),
				};

				tabModel.Notes = @"
Actions add Buttons to the tab. When clicked, it will:
* Start a task that calls this action
* Add a Tasks grid to the tab
  - Add a new Task to that grid

* Tasks
";
			}

			private void PassParams(int v1, string v2)
			{
				Log log = new Log();
				log.Add("If you log and no one's listening, are you really logging?");
			}

			private int counter = 1;
			private void AddEntry(Call call)
			{
				call.log.Add("New Log entry", new Tag("counter", counter++));
			}

			private void TestException(Call call)
			{
				throw new NotImplementedException();
			}

			private void SubTaskInstances(Call call)
			{
				List<int> downloads = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
				Parallel.ForEach(downloads, new ParallelOptions() { MaxDegreeOfParallelism = 10 }, i =>
				{
					using (CallTimer sleepCall = call.Timer(i.ToString()))
					{
						sleepCall.AddSubTask();
						sleepCall.taskInstance.ProgressMax = i;
						for (int j = 0; j < i; j++)
						{
							System.Threading.Thread.Sleep(1000);
							sleepCall.taskInstance.Progress = j + 1;
						}
					}
				});
			}
		}
	}
}
/*
*/
