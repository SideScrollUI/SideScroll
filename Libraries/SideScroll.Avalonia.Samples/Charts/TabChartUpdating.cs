using SideScroll.Attributes;
using SideScroll.Avalonia.Charts.LiveCharts;
using SideScroll.Charts;
using SideScroll.Collections;
using SideScroll.Resources;
using SideScroll.Tabs;
using SideScroll.Tabs.Samples.Charts;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.Avalonia.Samples.Charts;

public class TabChartUpdating : ITab
{
	public TabInstance Create() => new Instance();

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonRefresh { get; set; } = new("Refresh", Icons.Svg.Refresh);

		[Separator]
		public ToolButton ButtonStart { get; set; } = new("Start", Icons.Svg.Play);
		public ToolButton ButtonStop { get; set; } = new("Stop", Icons.Svg.Stop);
	}

	public class Instance : TabInstance
	{
		public const int SampleCount = 24;

		private readonly Random _random = new();

		private TabLiveChart? _chart;

		public override void LoadUI(Call call, TabModel model)
		{
			model.MinDesiredWidth = 1000;

			Toolbar toolbar = new();
			toolbar.ButtonRefresh.Action = Update;
			toolbar.ButtonStart.ActionAsync = StartTaskAsync;
			toolbar.ButtonStop.Action = StopTask;
			model.AddObject(toolbar);

			ChartView chartView = CreateView();
			_chart = new TabLiveChart(this, chartView);
			model.AddObject(_chart, true);
		}

		private ChartView CreateView()
		{
			var chartView = new ChartView
			{
				LegendPosition = ChartLegendPosition.Right,
				ShowTimeTracker = true,
			};

			DateTime dateTime = DateTime.Now;
			for (int i = 0; i < 2; i++)
			{
				var series = ChartSamples.CreateTimeSeries(dateTime, TimeSpan.FromHours(1), SampleCount);
				chartView.AddSeries($"Series {2 * i + _random.Next() % 2}", series, seriesType: SeriesType.Average);
			}

			return chartView;
		}

		private void Update(Call call)
		{
			ChartView chartView = CreateView();
			_chart!.UpdateView(chartView);
		}

		private Call? _addCall;
		private async Task StartTaskAsync(Call call)
		{
			_addCall = call;

			CancellationToken token = call.TaskInstance!.CancelToken;
			for (int i = 0; i < 60 && !token.IsCancellationRequested; i++)
			{
				Post(call, () => Update(call));
				await Task.Delay(1000);
			}
		}

		private void StopTask(Call call)
		{
			_addCall?.TaskInstance?.Cancel();
			_addCall = null;
		}
	}
}
