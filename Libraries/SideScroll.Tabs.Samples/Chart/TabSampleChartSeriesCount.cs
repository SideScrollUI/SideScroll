using SideScroll.Charts;
using SideScroll.Collections;
using SideScroll.Extensions;

namespace SideScroll.Tabs.Samples.Chart;

public class TabSampleChartSeriesCount : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.ReloadOnThemeChange = true;

			DateTime endTime = DateTime.UtcNow.Trim(TimeSpan.TicksPerHour).AddHours(8);

			var chartView = new ChartView("Lots of Series")
			{
				ShowTimeTracker = true,
			};

			for (int i = 0; i < 25; i++)
			{
				chartView.AddSeries($"Series {i}", ChartSamples.CreateTimeSeries(endTime), seriesType: SeriesType.Average);
			};
			model.AddObject(chartView);
		}
	}
}
