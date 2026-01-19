using SideScroll.Avalonia.Charts.LiveCharts;
using SideScroll.Charts;
using SideScroll.Collections;
using SideScroll.Extensions;
using SideScroll.Tabs;
using SideScroll.Tabs.Samples.Charts;

namespace SideScroll.Avalonia.Samples.Charts;

public class TabChartSizes : ITab
{
	public TabInstance Create() => new Instance();

	private class Instance : TabInstance
	{
		public override void LoadUI(Call call, TabModel model)
		{
			model.MinDesiredWidth = 1000;

			DateTime dateTime = DateTime.Now.Trim();
			var series = ChartSamples.CreateTimeSeries(dateTime, TimeSpan.FromHours(6));

			var chartView = new ChartView
			{
				LegendPosition = ChartLegendPosition.Hidden,
				ShowTimeTracker = true,
			};
			chartView.AddSeries("Count", series, seriesType: SeriesType.Average);

			var liveChart = new TabLiveChart(chartView)
			{
				Height = 80,
			};
			liveChart.Chart.MinHeight = 80;
			model.AddObject(liveChart);
		}
	}
}
