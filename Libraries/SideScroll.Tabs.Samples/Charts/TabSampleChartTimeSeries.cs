using SideScroll.Charts;
using SideScroll.Collections;
using SideScroll.Extensions;
using SideScroll.Time;
using System.Drawing;

namespace SideScroll.Tabs.Samples.Charts;

public class TabSampleChartTimeSeries : ITab
{
	public TabInstance Create() => new Instance();

	private class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			DateTime endTime = TimeZoneView.Now.Trim(TimeSpan.TicksPerHour).AddHours(12);

			AddAnimals(model, endTime);
			AddToys(model, endTime);
			AddBirds(model, endTime);
		}

		private static void AddAnimals(TabModel model, DateTime endTime)
		{
			var chartView = new ChartView("Animals")
			{
				ShowTimeTracker = true,
				LogBase = 10,
			};

			chartView.AddSeries("Cats", ChartSamples.CreateTimeSeries(endTime), seriesType: SeriesType.Average);
			chartView.AddSeries("Dogs", ChartSamples.CreateTimeSeries(endTime), seriesType: SeriesType.Average);

			chartView.Annotations.Add(new ChartAnnotation
			{
				Text = "Too Many",
				Y = 2_000_000_000,
				Color = Color.Red,
			});
			model.AddObject(chartView);
		}

		private static void AddToys(TabModel model, DateTime endTime)
		{
			var chartViewToys = new ChartView("Toys")
			{
				ShowTimeTracker = true,
			};
			chartViewToys.AddSeries("Toys", ChartSamples.CreateIdenticalTimeSeries(endTime), seriesType: SeriesType.Average);
			model.AddObject(chartViewToys);
		}

		private static void AddBirds(TabModel model, DateTime endTime)
		{
			var chartView = new ChartView("Birds")
			{
				ShowTimeTracker = true,
			};

			chartView.AddSeries("Birds", ChartSamples.CreateTimeSeries(endTime, minValue: 9999999, maxValue: 10000000), seriesType: SeriesType.Average);

			model.AddObject(chartView);
		}
	}
}
