using Atlas.Core;
using Atlas.Tabs.Test.DataGrid;

namespace Atlas.Tabs.Test;

[PublicData]
public class TabSample : ITab
{
	public override string ToString() => "Sample";

	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		private ItemCollection<SampleItem>? _sampleItems;

		public override void Load(Call call, TabModel model)
		{
			_sampleItems = new ItemCollection<SampleItem>();
			AddItems(5);

			model.Items = new ItemCollection<ListItem>("Items")
			{
				new("Sample Items", _sampleItems),
				new("Collections", new TabTestGridCollectionSize()),
				new("Recursive Copy", new TabSample()),
			};

			model.Actions = new List<TaskCreator>()
			{
				new TaskDelegate("Sleep 10s", Sleep, true),
				new TaskAction("Add 5 Items", () => AddItems(5), false), // Foreground task so we can modify collection
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
			call.TaskInstance!.ProgressMax = 10;
			for (int i = 0; i < 10; i++)
			{
				Thread.Sleep(1000);
				call.Log.Add("Slept 1 second");
				call.TaskInstance.Progress++;
			}
		}

		private void AddItems(int count)
		{
			for (int i = 0; i < count; i++)
			{
				int id = _sampleItems!.Count;
				_sampleItems.Add(new SampleItem(id, "Item " + id));
			}
		}
	}
}

public class SampleItem
{
	public int Id { get; set; }
	public string Name { get; set; }

	public override string ToString() => Name;

	public SampleItem(int id, string name)
	{
		Id = id;
		Name = name;
	}
}
