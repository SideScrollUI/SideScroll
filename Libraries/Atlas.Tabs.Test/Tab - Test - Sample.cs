﻿using System;
using System.Collections.Generic;
using Atlas.Core;
using Atlas.Tabs.Test.DataGrid;

namespace Atlas.Tabs.Test
{
	public class TabSample : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			private ItemCollection<SampleItem> sampleItems;

			public override void Load(Call call)
			{
				// Replace this
				sampleItems = new ItemCollection<SampleItem>();
				AddItems(5);

				tabModel.Items = new ItemCollection<ListItem>()
				{
					new ListItem("Sample Items", sampleItems),
					new ListItem("Small Collection", new TabTestGridCollectionSize()),
					new ListItem("Child Tab", new TabSample()), // recursive
				};

				tabModel.Actions = new ItemCollection<TaskCreator>()
				{
					new TaskDelegate("Sleep 10s", Sleep, true),
					new TaskAction("Add 5 Items", new Action(() => AddItems(5)), false), // Foreground task so we can modify collection
				};

				tabModel.Notes =
@"
This is a sample tab that shows some of the different tab features

Actions
DataGrids
";
			}

			private void Sleep(Call call)
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
			}
		}
	}

	public class SampleItem
	{
		public int ID { get; set; }
		public string Name { get; set; }

		public SampleItem(int id, string name)
		{
			this.ID = id;
			this.Name = name;
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
