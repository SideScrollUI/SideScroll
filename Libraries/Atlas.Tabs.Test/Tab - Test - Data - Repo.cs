using System;
using System.Collections.Generic;
using Atlas.Core;
using Atlas.Tabs.Tools;

//namespace Atlas.Tabs.Test.DataRepo // good idea?
namespace Atlas.Tabs.Test
{
	public class TabTestDataRepo : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			private ItemCollection<SampleItem> sampleItems;

			public override void Load()
			{
				//new ListItem("Data Repos", new TabDirectory(project.DataApp.RepoPath)),
				// Replace this
				sampleItems = new ItemCollection<SampleItem>();
				AddItems(5);

				tabModel.Items = new ItemCollection<ListItem>()
				{
					new ListItem("Data Repos", new TabDirectory(project.DataApp.RepoPath)),
				};

				tabModel.Actions = new ItemCollection<TaskCreator>()
				{
					new TaskDelegate("Delete", Delete),
					//new TaskAction("Add 5 Items", new Action(() => AddItems(5)), false), // Foreground task so we can modify collection
				};

				tabModel.Notes = "Data Repos store C# objects as serialized data.";
			}

			private void Delete(Call call)
			{
				project.DataShared.DeleteRepo();
				project.DataApp.DeleteRepo();
				Reload();
			}

			private void AddItems(int count)
			{
				for (int i = 0; i < count; i++)
					sampleItems.Add(new SampleItem(sampleItems.Count, "Item " + sampleItems.Count.ToString()));
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

}
/*
*/
