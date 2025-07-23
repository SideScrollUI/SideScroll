using SideScroll.Avalonia.Charts.LiveCharts;
using SideScroll.Charts;
using SideScroll.Collections;
using SideScroll.Tabs;
using SideScroll.Tabs.Samples.Chart;

namespace SideScroll.Avalonia.Samples.Charts;

public class TabChartSizes : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public const int SampleCount = 24;

		public override void LoadUI(Call call, TabModel model)
		{
			model.MinDesiredWidth = 1000;

			DateTime dateTime = DateTime.Now;
			var series = ChartSamples.CreateTimeSeries(dateTime, TimeSpan.FromHours(6), SampleCount);

			var chartView = new ChartView
			{
				LegendPosition = ChartLegendPosition.Hidden,
				ShowTimeTracker = true,
			};
			chartView.AddSeries("Count", series, seriesType: SeriesType.Average);

			var chart = new TabLiveChart(this, chartView)
			{
				Height = 80,
			};
			chart.Chart.MinHeight = 80;
			model.AddObject(chart);
		}
	}
}
