using SideScroll.Charts;
using SideScroll.Collections;
using SideScroll.Extensions;
using SideScroll.Time;
using System.Drawing;

namespace SideScroll.Tabs.Samples.Charts;

public class TabSampleChartTimeSeriesFractional : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		private static readonly TimeSpan SampleDuration = TimeSpan.FromMilliseconds(10);

		public override void Load(Call call, TabModel model)
		{
			DateTime endTime = TimeZoneView.Now.Trim(SampleDuration);

			AddCoinFlips(model, endTime);
			AddDecimalPrecision(model, endTime);
		}

		private static DateTime AddCoinFlips(TabModel model, DateTime endTime)
		{
			var chartView = new ChartView("Coin Flips")
			{
				DefaultPeriodDuration = SampleDuration,
				ShowTimeTracker = true,
			};

			chartView.AddSeries("Heads", ChartSamples.CreateTimeSeries(endTime, maxValue: 0.5, sampleDuration: SampleDuration), seriesType: SeriesType.Average);
			chartView.AddSeries("Tails", ChartSamples.CreateTimeSeries(endTime, maxValue: 0.25, sampleDuration: SampleDuration), seriesType: SeriesType.Average);

			chartView.Annotations.Add(new ChartAnnotation
			{
				Text = "Average",
				Y = 0.5,
				Color = Color.Red,
			});
			model.AddObject(chartView);
			return endTime;
		}

		private static DateTime AddDecimalPrecision(TabModel model, DateTime endTime)
		{
			var chartView = new ChartView("Decimal Precision")
			{
				DefaultPeriodDuration = SampleDuration,
				ShowTimeTracker = true,
			};

			chartView.AddSeries("Percent", ChartSamples.CreateTimeSeries(endTime, minValue: 99.9999, maxValue: 100, sampleDuration: SampleDuration), seriesType: SeriesType.Average);

			model.AddObject(chartView);
			return endTime;
		}
	}
}
