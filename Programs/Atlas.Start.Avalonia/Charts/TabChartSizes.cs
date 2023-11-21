using Atlas.Core;
using Atlas.Tabs;
using Atlas.Tabs.Test.Chart;
using Atlas.UI.Avalonia.Charts.LiveCharts;

namespace Atlas.Start.Avalonia.Tabs;

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

			var chartView = new ChartView()
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
