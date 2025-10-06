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
		public override void Load(Call call, TabModel model)
		{
			DateTime endTime = TimeZoneView.Now.Trim(TimeSpan.TicksPerHour).AddHours(12);

			AddAnimals(model, endTime);
			AddToys(model, endTime);
			AddDecimalPrecision(model, endTime);
		}

		private static DateTime AddAnimals(TabModel model, DateTime endTime)
		{
			var chartView = new ChartView("Animals")
			{
				ShowTimeTracker = true,
			};

			chartView.AddSeries("Cats", ChartSamples.CreateTimeSeries(endTime, maxValue: 0.5), seriesType: SeriesType.Average);
			chartView.AddSeries("Dogs", ChartSamples.CreateTimeSeries(endTime, maxValue: 0.25), seriesType: SeriesType.Average);

			chartView.Annotations.Add(new ChartAnnotation
			{
				Text = "Too Many",
				Y = 0.5,
				Color = Color.Red,
			});
			model.AddObject(chartView);
			return endTime;
		}

		private static void AddToys(TabModel model, DateTime endTime)
		{
			var chartViewToys = new ChartView("Toys")
			{
				ShowTimeTracker = true,
			};
			chartViewToys.AddSeries("Toys", ChartSamples.CreateIdenticalTimeSeries(endTime, value: 0.42), seriesType: SeriesType.Average);
			model.AddObject(chartViewToys);
		}

		private static DateTime AddDecimalPrecision(TabModel model, DateTime endTime)
		{
			var chartView = new ChartView("Decimal Precision")
			{
				ShowTimeTracker = true,
			};

			chartView.AddSeries("Percent", ChartSamples.CreateTimeSeries(endTime, minValue: 99.9999, maxValue: 100), seriesType: SeriesType.Average);

			model.AddObject(chartView);
			return endTime;
		}
	}
}
