using Atlas.Core;
using Atlas.Extensions;
using System.Drawing;

namespace Atlas.Tabs.Test.Chart;

public class TabTestChartTimeRangeValue : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			DateTime endTime = DateTime.UtcNow.Trim(TimeSpan.TicksPerHour).AddHours(8);

			AddAnimals(model, endTime);
			AddToys(model, endTime);
		}

		private DateTime AddAnimals(TabModel model, DateTime endTime)
		{
			var chartView = new ChartView("Animals")
			{
				ShowTimeTracker = true,
				LogBase = 10,
			};

			chartView.AddSeries("Cats", ChartSamples.CreateTimeSeries(endTime), seriesType: SeriesType.Average);
			chartView.AddSeries("Dogs", ChartSamples.CreateTimeSeries(endTime), seriesType: SeriesType.Average);

			chartView.Annotations.Add(new ChartAnnotation()
			{
				Text = "Too Many",
				Y = 2_000_000_000,
				Color = Color.Red,
			});
			model.AddObject(chartView);
			return endTime;
		}

		private void AddToys(TabModel model, DateTime endTime)
		{
			var chartViewToys = new ChartView("Toys")
			{
				ShowTimeTracker = true,
			};
			chartViewToys.AddSeries("Toys", ChartSamples.CreateIdenticalTimeSeries(endTime), seriesType: SeriesType.Average);
			model.AddObject(chartViewToys);
		}
	}
}
