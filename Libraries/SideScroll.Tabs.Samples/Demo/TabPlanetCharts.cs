using SideScroll.Charts;
using SideScroll.Extensions;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Samples.Charts;
using SideScroll.Tabs.Samples.Models;
using SideScroll.Time;

namespace SideScroll.Tabs.Samples.Demo;

public class TabPlanetCharts : ITab
{
	public TabInstance Create() => new Instance();

	private class Instance : TabInstance, ITabSelector
	{
		private readonly SolarSystem _solarSystem = SolarSystem.Sample;

		public new event EventHandler<TabSelectionChangedEventArgs>? OnSelectionChanged;

		public override void Load(Call call, TabModel model)
		{
			DateTime endTime = TimeZoneView.Now.Trim(TimeSpan.TicksPerHour).AddYears(10);

			AddPlanetOrbits(model, endTime);
		}

		private void AddPlanetOrbits(TabModel model, DateTime endTime)
		{
			var chartView = new ChartView("Earth to Planet Distance - AU")
			{
				ShowTimeTracker = true,
				Series = PlanetSampleData.CreateEarthPlanetDistanceTimeSeries(endTime, sampleCount: 480),
			};
			model.AddObject(chartView);
			chartView.SelectionChanged += ChartView_SelectionChanged;
		}

		private void ChartView_SelectionChanged(object? sender, SeriesSelectedEventArgs e)
		{
			SelectedItems = e.Series
				.Select(s => _solarSystem.Planets.SingleOrDefault(p => p.Name == s.Name))
				.OfType<Planet>()
				.Select(p => new ListItem(p.Name!, p))
				.ToList();
			OnSelectionChanged?.Invoke(sender, new TabSelectionChangedEventArgs());
		}
	}
}
