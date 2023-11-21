using Atlas.Core;
using Atlas.Tabs;
using Atlas.Tabs.Test.Chart;
using Atlas.UI.Avalonia.Charts.LiveCharts;

namespace Atlas.Start.Avalonia.Tabs;

public class TabChartUpdating : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public const int SampleCount = 24;

		private readonly Random _random = new();

		private TabControlLiveChart? _chart;

		public override void LoadUI(Call call, TabModel model)
		{
			model.MinDesiredWidth = 1400;

			model.Actions = new List<TaskCreator>()
			{
				new TaskDelegate("Update", Update),
			};

			ChartView chartView = CreateView();
			_chart = new TabControlLiveChart(this, chartView);
			model.AddObject(_chart, true);
		}

		private void Update(Call call)
		{
			ChartView chartView = CreateView();
			_chart!.UpdateView(chartView);
		}

		private ChartView CreateView()
		{
			var chartView = new ChartView()
			{
				LegendPosition = ChartLegendPosition.Right,
				ShowTimeTracker = true,
			};

			DateTime dateTime = DateTime.Now;
			for (int i = 0; i < 2; i++)
			{
				var series = ChartSamples.CreateTimeSeries(dateTime, SampleCount);
				chartView.AddSeries($"Series {2 * i + _random.Next() % 2}", series, seriesType: SeriesType.Average);
			}

			return chartView;
		}
	}
}
