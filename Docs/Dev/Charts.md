# Charts

## Adding Charts

* Create a `ChartView` object for each Chart you want to display
* Add the `Series` using one of the following methods:
  - `AddSeries(string name, IList list, string? xPropertyName = null, string? yPropertyName = null, SeriesType seriesType = SeriesType.Sum)`
  - `AddDimensions(IList iList, string xPropertyName, string yPropertyName, params string[] dimensionPropertyNames)`
  - Create a `ListSeries` and add it to the `ChartView.Series`
* Add the `ChartView` to the `TabModel` by calling `model.AddObject(chartView)`
* Series like `ItemCollection` that implement the `INotifyCollectionChanged` interface can also add new points after loading

### Sample Chart Tab

```csharp
using SideScroll.Attributes;
using SideScroll.Charts;
using SideScroll.Collections;
using SideScroll.Resources;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.Tabs.Samples.Chart;

public class TabSampleChartLists : ITab
{
	public TabInstance Create() => new Instance();

	private class Toolbar : TabToolbar
	{
		public ToolButton ButtonAdd { get; } = new("Add", Icons.Svg.Add);

		[Separator]
		public ToolButton ButtonStart { get; } = new("Start", Icons.Svg.Play);
		public ToolButton ButtonStop { get; } = new("Stop", Icons.Svg.Stop);
	}

	private class Instance : TabInstance
	{
		private List<ItemCollection<int>> _series = [];
		private readonly Random _random = new();

		public override void Load(Call call, TabModel model)
		{
			_series = [];

			Toolbar toolbar = new();
			toolbar.ButtonAdd.Action = AddEntry;
			toolbar.ButtonStart.ActionAsync = StartTaskAsync;
			toolbar.ButtonStop.Action = StopTask;
			model.AddObject(toolbar);

			ChartView chartView = new();
			for (int i = 0; i < 2; i++)
			{
				ItemCollection<int> list = [];
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

		private Call? _addCall;
		private async Task StartTaskAsync(Call call)
		{
			_addCall = call;

			CancellationToken token = call.TaskInstance!.TokenSource.Token;
			for (int i = 0; i < 1000 && !token.IsCancellationRequested; i++)
			{
				Invoke(AddSampleUI, call);
				await Task.Delay(1000);
			}
		}

		private void StopTask(Call call)
		{
			_addCall?.TaskInstance?.Cancel();
			_addCall = null;
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

### Sample Custom Chart

```csharp
public class TabChartSizes : ITab
{
	public TabInstance Create() => new Instance();

	private class Instance : TabInstance
	{
		private const int SampleCount = 24;

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
[Source](../../Libraries/SideScroll.Avalonia.Samples/Charts/TabChartSizes.cs)

## Links

- [Sample Tabs](../../Libraries/SideScroll.Tabs.Samples/Chart/TabSampleCharts.cs)
- [LiveCharts 2](https://livecharts.dev/) - Integrated Avalonia Chart Library used by SideScroll