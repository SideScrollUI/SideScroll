using SideScroll.Attributes;
using SideScroll.Avalonia.Charts.LiveCharts;
using SideScroll.Charts;
using SideScroll.Collections;
using SideScroll.Extensions;
using SideScroll.Resources;
using SideScroll.Tabs;
using SideScroll.Tabs.Samples.Charts;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.Avalonia.Samples.Charts;

public class TabChartUpdating : ITab
{
	public TabInstance Create() => new Instance();

	private class Toolbar : TabToolbar
	{
		public ToolButton ButtonRefresh { get; } = new("Refresh", Icons.Svg.Refresh);

		[Separator]
		public ToolButton ButtonStart { get; } = new("Start", Icons.Svg.Play, backgroundThread: true);
		public ToolButton ButtonStop { get; } = new("Stop", Icons.Svg.Stop);
	}

	private class Instance : TabInstance
	{
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
			ChartView chartView = new()
			{
				LegendPosition = ChartLegendPosition.Right,
				ShowTimeTracker = true,
			};

			DateTime dateTime = DateTime.Now.Trim();
			for (int i = 0; i < 2; i++)
			{
				var series = ChartSamples.CreateTimeSeries(dateTime, TimeSpan.FromHours(1));
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

			CancellationToken cancelToken = call.TaskInstance!.CancelToken;
			for (int i = 0; i < 60 && !cancelToken.IsCancellationRequested; i++)
			{
				Post(call, () => Update(call));
				await Task.Delay(1000, cancelToken);
			}
		}

		private void StopTask(Call call)
		{
			_addCall?.TaskInstance?.Cancel();
			_addCall = null;
		}
	}
}
