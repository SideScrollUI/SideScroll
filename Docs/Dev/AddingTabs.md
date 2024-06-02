# Adding new Tabs

* Every tab is composed of an outer class that implements the `ITab` interface. The `ITab` interface allows you to:
  - Set parameters that can be reused each time a `TabInstance` is created
  - Defines a `Create()` method that creates a `TabInstance` that you can pass those parameters to
  - You can also declare properties for a Tab, which when passed in a IList will be displayed as columns for the DataGrid
* A new `TabInstance` will be created each time that Tab becomes visible, meaning the `TabInstance.Load()` is not called until the Tab is shown.
* There are 3 different Load methods that can be used for a `TabInstance`. You can use any combination of these. They are called in the order below:
  - `public async Task LoadAsync(Call call, TabModel model)`
    - Use when you need to call async methods
  - `public override void Load(Call call, TabModel model)`
    - The default Load method if you don't need to call an async method
  - `public override void LoadUI(Call call, TabModel model)`
    - Use when you need to create an Avalonia control, since those can only be created on the UI thread.
```csharp
namespace Atlas.Tabs.Samples
{
	public class TabSample : ITab
	{
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			public override void Load(Call call, TabModel model)
			{
				model.Items = new ItemCollection<ListItem>()
				{
					new("Tab 1", new Tab1()),
					new("Tab 2", new Tab2()),
				};

				model.Actions = new List<TaskCreator>()
				{
					new TaskDelegate("Log this", LogThis),
				};
			}

			private void LogThis(Call call)
			{
				call.Log.Add("I've been logged");
			}
		}
	}
}
```

## Items
* Most `TabInstance` will contain `Items`. These can be any `IList` and will be displayed in a DataGrid
  - Any object properties will automatically be show as columns. If no properties are found, the object's `ToString()` will be used for the item
  - The most common `Items` are a collection of `ListItem`, which lets you set a label and object to display
  - When a row is selected, a child tab will be shown to the right based on the following criteria
    - If the object is an `ITab`, a `TabInstance` will be created
	- If the object is a primitive type, a text editor will be shown
	- Otherwise, all the fields and properties of an object will be displayed

## Actions
* You can declare actions for Tabs, which will show up as buttons
* Actions that set `useTask` to `true` will run in the background and won't block the UI from updating. However, you can't modify the UI or modify the `tabModel` while running in the background. If you want to update the UI after finishing a task you can `Invoke` a different function that does so.
  - `Dispatcher.UIThread.Post(() => SetStatusUI(call, "Finished"));`
```csharp
public void UpdateStatus(Call call, string text)
{
	textBox.Text = text;
}
```
* Types
  - TaskDelegate
    - `new TaskDelegate("Add Item", AddItem, false)`
    - `private void AddItem(Call call)`
    - Generally the preferred way if you don't need to pass parameters
  - TaskDelegateAsync
    - `new TaskDelegateAsync("Add Item", AddItemAsync, false)`
    - `private async Task AddItemAsync(Call call)`
    - Same as `TaskDelegate`, but called using `async`. Use whenever you need to call async functions.
  - TaskAction
    - `new TaskAction("Add Item", new Action(() => AddItems(5)))`
    - `private void AddItems(int count)`
    - Useful when you need to pass custom parameters
* ItemCollectionUI
  - This is a User Interface version of the ItemCollection, which allows you to add items to a collection that appears in the user interface from a background thread. Adding an item to a List or ItemCollection from a background thread normally isn't safe and can cause an exception.
```csharp
namespace Atlas.Tabs.Samples
{
	public class TabSample(int count) : ITab
	{
		private int Count = count;

		public TabInstance Create() => new Instance();

		public class Instance(TabSample tab) : TabInstance
		{
			private ItemCollectionUI<SampleItem> _sampleItems;

			public override void Load(Call call, TabModel model)
			{
				_sampleItems = new ItemCollectionUI<SampleItem>();
				AddItems(tab.Count);

				model.Items = new ItemCollection<ListItem>("Items")
				{
					new("Sample Items", _sampleItems),
					new("Collections", new TabTestGridCollectionSize()),
					new("Copy", new TabSample()), // Recursive
				};

				model.Actions = new List<TaskCreator>()
				{
					new TaskDelegateAsync("Sleep 10s", SleepAsync),
					new TaskDelegate("Add Items", AddItems),
				};
			}

			private async Task SleepAsync(Call call)
			{
				call.TaskInstance.ProgressMax = 10;
				for (int i = 0; i < 10; i++)
				{
					await Task.Delay(1000);
					call.Log.Add("Slept 1 second");
					call.TaskInstance.Progress++;
				}
			}

			private void AddItems(int count)
			{
				for (int i = 0; i < count; i++)
				{
					_sampleItems.Add(new SampleItem(_sampleItems.Count, "Item " + _sampleItems.Count));
				}
			}
		}
	}

	public class SampleItem(int id, string name)
	{
		public int Id { get; set; } = id;
		public string Name { get; set; } = name;

		public override string ToString() => Name;
	}
}
```
* Here's the resulting tab
  - Note how all the properties in the SampleItem automatically appear as columns
  - The `Recursive Tab` will show a new instance of the tab nested until it runs out of room on the screen
![New Tab](../Images/Screenshots/SampleTab.png)

## Async calls
  - Tabs can load as Async by implementing the `ITabAsync` interface for a `TabInstance`
  - This allows calling async methods
  - You can also make calls async by using `TaskDelegateAsync`
    - These are useful when you need to make lots of parallel async calls
```csharp
namespace Atlas.Tabs.Samples.Actions
{
	public class TabSampleAsync : ITab
	{
		public TabInstance Create() { return new Instance(); }

		public class Instance : TabInstance, ITabAsync
		{
			//private ItemCollection<ListItem> items;

			public async Task LoadAsync(Call call, TabModel model)
			{
				await Task.Delay(2000);
				model.AddObject("Finished");

				model.Actions = new ItemCollection<TaskCreator>()
				{
					new TaskDelegate("Add Log Entry", AddEntry),
					new TaskDelegateAsync("Sleep (Async)", SleepAsync, true, true),
				};
			}

			private int _counter = 1;
			private void AddEntry(Call call)
			{
				call.Log.Add("New Log entry", new Tag("counter", _counter++));
			}

			private async Task SleepAsync(Call call)
			{
				call.Log.Add("Sleeping for 3 seconds");
				await Task.Delay(3000);
				call.Log.Add("Waking Up");
			}
		}
	}
}
```

## Toolbars

## Custom Controls
