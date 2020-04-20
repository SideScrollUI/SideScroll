﻿using Atlas.Core;
using Atlas.Tabs.Test.DataGrid;
using System;
using System.Collections.Generic;

namespace Atlas.Tabs.Test
{
	public class TabSample : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			private ItemCollection<SampleItem> sampleItems;

			public override void Load(Call call, TabModel model)
			{
				sampleItems = new ItemCollection<SampleItem>();
				AddItems(5);

				model.Items = new ItemCollection<ListItem>("Items")
				{
					new ListItem("Sample Items", sampleItems),
					new ListItem("Collections", new TabTestGridCollectionSize()),
					new ListItem("Recursive Copy", new TabSample()),
				};

				model.Actions = new ItemCollection<TaskCreator>()
				{
					new TaskDelegate("Sleep 10s", Sleep, true),
					new TaskAction("Add 5 Items", new Action(() => AddItems(5)), false), // Foreground task so we can modify collection
				};

				model.Notes =
@"
This is a sample tab that shows some of the different tab features

Actions
DataGrids
";
			}

			private void Sleep(Call call)
			{
				call.TaskInstance.ProgressMax = 10;
				for (int i = 0; i < 10; i++)
				{
					System.Threading.Thread.Sleep(1000);
					call.Log.Add("Slept 1 second");
					call.TaskInstance.Progress++;
				}
			}

			private void AddItems(int count)
			{
				for (int i = 0; i < count; i++)
					sampleItems.Add(new SampleItem(sampleItems.Count, "Item " + sampleItems.Count));
			}
		}
	}

	public class SampleItem
	{
		public int ID { get; set; }
		public string Name { get; set; }

		public SampleItem(int id, string name)
		{
			ID = id;
			Name = name;
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
