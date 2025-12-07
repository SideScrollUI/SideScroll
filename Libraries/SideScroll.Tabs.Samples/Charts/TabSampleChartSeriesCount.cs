using SideScroll.Charts;
using SideScroll.Collections;
using SideScroll.Extensions;
using SideScroll.Tabs.Lists;
using SideScroll.Time;

namespace SideScroll.Tabs.Samples.Charts;

public class TabSampleChartSeriesCount : ITab
{
	public TabInstance Create() => new Instance();

	private class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Items = new List<ListItem>
			{
				new(10, TabModel.Create("10", CreateChartView(10))),
				new(25, TabModel.Create("25", CreateChartView(25))),
			};
		}

		private static ChartView CreateChartView(int seriesCount)
		{
			DateTime endTime = TimeZoneView.Now.Trim(TimeSpan.TicksPerHour).AddHours(-12);

			var chartView = new ChartView($"{seriesCount} Chart Series")
			{
				ShowTimeTracker = true,
			};

			for (int i = 0; i < seriesCount; i++)
			{
				chartView.AddSeries($"Series {i}", ChartSamples.CreateTimeSeries(endTime, TimeSpan.FromDays(1), 12), seriesType: SeriesType.Average);
			}
			return chartView;
		}
	}
}
