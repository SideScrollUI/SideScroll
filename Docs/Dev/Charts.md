# Charts

## Adding Charts
* Create a `ChartView` object for each Chart you want to display
* Add the `Series` using one of the following methods:
  - `AddSeries(string name, IList list, string? xPropertyName = null, string? yPropertyName = null, SeriesType seriesType = SeriesType.Sum)`
  - `AddDimensions(IList iList, string xPropertyName, string yPropertyName, params string[] dimensionPropertyNames)`
  - Create a `ListSeries` and add it to the `ChartView.Series`
* Add the `ChartView` to the `TabModel` by calling `model.AddObject(chartView)`
* Series like `ItemCollection` that implement the `INotifyCollectionChanged` interface can also add new points after loading

#### Sample Chart Tab
```csharp
using SideScroll.Charts;
using SideScroll.Collections;
using SideScroll.Tasks;

namespace SideScroll.Tabs.Samples.Chart;

public class TabSampleChartLists : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		private List<ItemCollection<int>> _series = [];
		private readonly Random _random = new();

		public override void Load(Call call, TabModel model)
		{
			_series = [];

			model.Actions = new List<TaskCreator>
			{
				new TaskDelegate("Add Entry", AddEntry),
				new TaskDelegate("Start: 1 Entry / second", StartTask, true),
			};

			ChartView chartView = new();
			for (int i = 0; i < 2; i++)
			{
				ItemCollection<int> list = new();
				chartView.AddSeries($"Series {i}", list);
				_series.Add(list);
			}

			for (int i = 0; i < 10; i++)
			{
				AddSample();
			}
			model.AddObject(chartView);
		}

		private void AddEntry(Call call)
		{
			Invoke(call, AddSampleUI);
		}

		private void StartTask(Call call)
		{
			CancellationToken token = call.TaskInstance!.TokenSource.Token;
			for (int i = 0; i < 1000 && !token.IsCancellationRequested; i++)
			{
				Invoke(AddSampleUI, call);
				Thread.Sleep(1000);
			}
		}

		private void AddSample()
		{
			int multiplier = 1;
			foreach (var list in _series)
			{
				int amount = (_random.Next() % 1000) * multiplier;
				list.Add(amount);
				multiplier++;
			}
		}

		// UI context
		private void AddSampleUI(Call call, object state)
		{
			AddSample();
		}
	}
}
```
[Source](../../Libraries/SideScroll.Tabs.Samples/Chart/TabSampleChartLists.cs)

## Custom Controls
- Pass any `ChartView` to a `TabControlLiveChart` to allow updating that control directly

#### Sample Custom Chart
```csharp
public class TabChartSizes : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public const int SampleCount = 24;

		public override void LoadUI(Call call, TabModel model)
		{
			model.MinDesiredWidth = 1400;

			DateTime dateTime = DateTime.Now;
			var series = ChartSamples.CreateTimeSeries(dateTime, SampleCount);

			var chartView = new ChartView
			{
				LegendPosition = ChartLegendPosition.Hidden,
				ShowTimeTracker = true,
			};
			chartView.AddSeries("Count", series, seriesType: SeriesType.Average);

			var chart = new TabControlLiveChart(this, chartView)
			{
				Height = 80,
			};
			chart.Chart.MinHeight = 80;
			model.AddObject(chart);
		}
	}
}
```
[Source](../../Programs/SideScroll.Start.Avalonia/Charts/TabChartSizes.cs)

### Links
- [Sample Tabs](../../Libraries/SideScroll.Tabs.Samples/Chart/TabSampleCharts.cs)
- [LiveCharts 2](https://livecharts.dev/) - Internal Avalonia Chart Library used by SideScroll