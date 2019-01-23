using System;
using System.Collections.Generic;
using Atlas.Core;

namespace Atlas.Tabs.Test
{
	public class TabTestBookmarks : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			//private ItemCollection<SampleItem> sampleItems;

			public override void Load()
			{
				//tabModel.Items = project.navigator.History;

				Navigator navigator = Serialize.SerializerMemory.Clone<Navigator>(taskInstance.call, project.Navigator);
				navigator.History.RemoveAt(navigator.History.Count - 1); // remove the current in progress bookmark
				navigator.CurrentIndex = navigator.History.Count;
				//navigator.B

				tabModel.Items = new ItemCollection<ListItem>()
				{
					new ListItem("Navigator (snapshot)", navigator),
					//new ListItem("Recursive Tab", new TabSample()),
				};

				tabModel.Notes = "The Navigator class creates a bookmark for every tab change you make, and allows you to move backwards and forwards. The Back/Forward buttons currently use this. Eventually a list/drop down could be used to select the bookmark";

				// Replace this
				/*sampleItems = new ItemCollection<SampleItem>();

				tabModel.Actions = new ItemCollection<TaskCreator>()
				{
					new TaskDelegate("Sleep", Sleep),
					new TaskAction("Add 5 Items", new Action(() => AddItems(5)), false), // Foreground task so we can modify collection
				};*/
			}

			/*private void Sleep(Call call)
			{
				call.taskInstance.ProgressMax = 10;
				for (int i = 0; i < 10; i++)
				{
					System.Threading.Thread.Sleep(1000);
					call.log.Add("Slept 1 second");
					call.taskInstance.Progress++;
				}
			}

			private void AddItems(int count)
			{
				for (int i = 0; i < count; i++)
					sampleItems.Add(new SampleItem(sampleItems.Count, "Item " + sampleItems.Count.ToString()));
			}*/
		}
	}
}
/*
*/
