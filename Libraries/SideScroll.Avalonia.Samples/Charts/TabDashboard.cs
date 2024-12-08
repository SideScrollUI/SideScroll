using Avalonia.Controls;
using SideScroll.Avalonia.Charts.LiveCharts;
using SideScroll.Charts;
using SideScroll.Collections;
using SideScroll.Tabs;
using SideScroll.Tabs.Samples.Chart;
using SideScroll.Time;

namespace SideScroll.Avalonia.Samples.Charts;

public class TabDashboard : ITab
{
	public TabInstance Create() => new Instance();

	public class Instance : TabInstance
	{
		public const int RowCount = 5;
		public const int ColumnCount = 2;
		public const int SampleCount = 24;

		private readonly Random _random = new();

		public override void LoadUI(Call call, TabModel model)
		{
			model.MinDesiredWidth = 1400;

			var grid = new Grid
			{
				ColumnDefinitions = new("*, *"),
			};

			DateTime endTime = DateTime.Now;

			var timeWindow = new TimeWindow(endTime.AddHours(-SampleCount), endTime);

			for (int row = 0; row < RowCount; row++)
			{
				grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
				for (int column = 0; column < ColumnCount; column++)
				{
					var chartView = new ChartView("Animals")
					{
						ShowTimeTracker = true,
						TimeWindow = timeWindow,
					};

					for (int i = 0; i < _random.Next(1, 10); i++)
					{
						var series = ChartSamples.CreateTimeSeries(endTime, SampleCount);

						chartView.AddSeries($"Series {i}", series, seriesType: SeriesType.Average);

						var chart = new TabControlLiveChart(this, chartView)
						{
							MinHeight = 250,
							IsTitleSelectable = true,
							[Grid.ColumnProperty] = column,
							[Grid.RowProperty] = row,
						};
						grid.Children.Add(chart);
					}
				}
			}
			model.AddObject(grid, true, true);
		}
	}
}
