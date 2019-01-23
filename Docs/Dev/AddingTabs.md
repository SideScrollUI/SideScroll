
# Adding new Tabs
---
* Every tab is composed of an outer class that implements the ITab interface. The ITab interface allows you to:
  - Set parameters that can be reused each time an instance is created
  - Defines a `Create()` method that creates a TabInstance that you can pass said parameters to
* A new Instance will be created each time that Tab becomes visible, meaning the TabInstance `Load()` is delayed until the Tab is shown.
* Actions can be added in one of two ways
  - TaskDelegate
    - `new TaskDelegate("Add Item", AddItem, false)`
    - `private void AddItem(Call call)`
    - Generally the preferred way if you don't need to pass parameters
    - Allows setting the Task progress through the call
    - Allows logging via the call.log
  - TaskAction
    - `new TaskAction("Add Item", new Action(() => AddItems(5)))`
    - `private void AddItems(int count)`
    - Useful when you need to pass custom parameters
```csharp
namespace Atlas.Start.Test
{
	public class TabSample : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			private ItemCollection<SampleItem> sampleItems;

			public override void Load()
			{
				sampleItems = new ItemCollection<SampleItem>();

				ItemCollection<ListItem> items = new ItemCollection<ListItem>();
				items.Add(new ListItem("Sample Items", sampleItems));
				items.Add(new ListItem("Recursive Tab", new TabSample()));
				tabCollection.Items = items;

				ItemCollection<TaskCreator> actions = new ItemCollection<TaskCreator>();
				actions.Add(new TaskDelegate("Sleep", Sleep));
				actions.Add(new TaskAction("Add 5 Items", new Action(() => AddItems(5)), false)); // Foreground task so we can modify collection
				tabCollection.Actions = actions;
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
```
* Here's the resulting tab
  - Note how all the properties in the SampleItem automatically appear as columns
  - The `Recursive Tab` will show a new instance of the tab nested until it runs out of room on the screen
![New Tab](/Screenshots/SampleTab.png)