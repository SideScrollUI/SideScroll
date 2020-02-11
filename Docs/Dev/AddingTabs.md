# Adding new Tabs

* Every tab is composed of an outer class that implements the `ITab` interface. The `ITab` interface allows you to:
  - Set parameters that can be reused each time a `TabInstance` is created
  - Defines a `Create()` method that creates a `TabInstance` that you can pass those parameters to
  - You can also declare properties for a Tab, which when passed in a IList will be displayed as columns for the DataGrid
* A new `TabInstance` will be created each time that Tab becomes visible, meaning the `TabInstance.Load()` is not called until the Tab is shown.

```csharp
namespace Atlas.Tabs.Test
{
	public class TabSample : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance
		{
			public override void Load(Call call)
			{
				tabModel.Items = new ItemCollection<ListItem>()
				{
					new ListItem("Test Item", sampleItems),
					new ListItem("Collections", new TabTestGridCollectionSize()),
				};

				tabModel.Actions = new ItemCollection<TaskCreator>()
				{
					new TaskDelegate("Log this", LogThis, true),
				};
			}

			private void LogThis(Call call)
			{
				call.log.Add("I've been logged");
			}
		}
	}
}
```

## Items
* Most `TabInstance` will contain `Items`. These can be any `IList` and will be displayed in a datagrid
  - Any object properties will automatically be show as columns. If no properties are found, `ToString()` will be called on each object
  - The most common `Items` are a collection of `ListItem`, which lets you set a label and object to display
  - When a row is selected, a child tab will be shown to the right based on the following criteria
    - If the object is an `ITab`, a `TabInstance` will be created
	- If the object is a primitive type, a text editor will be shown
	- Otherwise, all the fields and properties of an object will be displayed

## Actions
* You can declare actions for Tabs, which will show up as buttons
* Actions that set `useTask` to `true` will run in the background and won't block the UI from updating. However, you can't modify the UI or modify the `tabModel` while running in the background. If you want to update the UI after finishing a task you can `Invoke` a different function that does so.
  - `Invoke(UpdateStatus, "param 1", "param 2");`
```csharp
public void UpdateStatus(Call call, params object[] objects)
{
	string param1 = (ItemCollection<TabCloudWatchAlarm>)objects[0];
	textBox.Text = param1;
}
```
* Types
  - TaskDelegate
    - `new TaskDelegate("Add Item", AddItem, false)`
    - `private void AddItem(Call call)`
    - Generally the preferred way if you don't need to pass parameters
    - Allows setting the Task progress through the call
    - Allows logging via the call.log
  - TaskDelegateAsync
    - `new TaskDelegateAsync("Add Item", AddItemAsync, false)`
    - `private async Task AddItemAsync(Call call)`
    - Same as `TaskDelegate`, but called using `async`
  - TaskAction
    - `new TaskAction("Add Item", new Action(() => AddItems(5)))`
    - `private void AddItems(int count)`
    - Useful when you need to pass custom parameters

```csharp
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
				sampleItems = new ItemCollection<SampleItem>();
				AddItems(5);

				tabModel.Items = new ItemCollection<ListItem>("Items")
				{
					new ListItem("Sample Items", sampleItems),
					new ListItem("Collections", new TabTestGridCollectionSize()),
					new ListItem("Child Tab", new TabSample()), // recursive
				};

				tabModel.Actions = new ItemCollection<TaskCreator>()
				{
					new TaskDelegate("Sleep 10s", Sleep, true),
					new TaskAction("Add 5 Items", new Action(() => AddItems(5)), false), // Foreground task so we can modify collection
				};
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
![New Tab](../Images/Screenshots/SampleTab.png)

## Async calls
  - Tabs can load as Async by implementing the `ITabAsync` interface for a `TabInstance`
  - This allows the GUI to continue responding to user actions while it's loading
  - You can also make calls async by using `TaskDelegateAsync`
    - These are useful when you need to make lots of parallel async calls
```csharp
namespace Atlas.Tabs.Test.Actions
{
	public class TabTestAsync : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance, ITabAsync
		{
			//private ItemCollection<ListItem> items;

			public async Task LoadAsync(Call call)
			{
				await Task.Delay(2000);
				tabModel.AddObject("Finished");

				tabModel.Actions = new ItemCollection<TaskCreator>()
				{
					new TaskDelegate("Add Log Entry", AddEntry),
					new TaskDelegateAsync("Sleep (Async)", SleepAsync, true, true),
				};
			}

			private int counter = 1;
			private void AddEntry(Call call)
			{
				call.log.Add("New Log entry", new Tag("counter", counter++));
			}

			private async Task SleepAsync(Call call)
			{
				call.log.Add("Sleeping for 3 seconds");
				await Task.Delay(3000);
				call.log.Add("Waking Up");
			}
		}
	}
}
```

## Custom Controls
