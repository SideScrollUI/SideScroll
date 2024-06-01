using Atlas.Core;
using Atlas.Core.Tasks;
using Atlas.Tabs.Samples.Chart;
using Atlas.Tabs.Samples.DataGrid;
using Atlas.Tabs.Samples.Params;

namespace Atlas.Tabs.Samples;

[PublicData]
public class TabSampleDemo : ITab
{
	public override string ToString() => "Demo";

	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		private ItemCollection<SampleItem> _sampleItems = [];

		public override void Load(Call call, TabModel model)
		{
			_sampleItems = [];
			AddItems(5);

			model.Items = new ItemCollection<ListItem>("Items")
			{
				new("Sample Items", _sampleItems),
				new("Collections", new TabSampleGridCollectionSize()),
				new("Recursive Copy", new TabSampleDemo()),
				new("Chart", new TabSampleChartTimeSeries()),
				new("Params", new TabSampleParamsDataTabs()),
			};

			model.Actions = new List<TaskCreator>
			{
				new TaskDelegate("Sleep 10s", Sleep, true, true),
				new TaskAction("Add 5 Items", () => AddItems(5)), // Foreground task so we can modify collection
			};
		}

		private static void Sleep(Call call)
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
				int id = _sampleItems.Count;
				_sampleItems.Add(new SampleItem(id, "Item " + id));
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
