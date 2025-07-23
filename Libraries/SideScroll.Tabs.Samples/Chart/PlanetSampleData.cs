using SideScroll.Collections;
using SideScroll.Time;

namespace SideScroll.Tabs.Samples.Chart;

public static class PlanetSampleData
{
	public const double DaysInAYear = 365.25;

	private static readonly Dictionary<string, (double PeriodYears, double MinAU, double MaxAU)> PlanetOrbitData = new()
	{
		["Mercury"] = (0.24, 0.6, 1.4),
		["Venus"] = (0.62, 0.3, 1.7),
		["Mars"] = (1.88, 0.4, 2.5),
		["Jupiter"] = (11.86, 4.2, 6.2),
		["Saturn"] = (29.45, 8.0, 11.0),
		["Uranus"] = (84.0, 17.0, 21.0),
		["Neptune"] = (164.8, 28.0, 31.0),
		// ["Pluto"] = (248.0, 28.0, 49.0),
	};

	public static List<ListSeries> CreateEarthPlanetDistanceTimeSeries(
		DateTime endTime,
		TimeSpan? sampleDuration = null,
		int sampleCount = 120)
	{
		return PlanetOrbitData
			.Select(p => new ListSeries(p.Key, CreateEarthPlanetDistanceTimeSeries(p.Key, endTime, sampleDuration, sampleCount))
			{
				SeriesType = SeriesType.Average,
			})
			.ToList();
	}

	public static List<TimeRangeValue> CreateEarthPlanetDistanceTimeSeries(
		string planetName,
		DateTime endTime,
		TimeSpan? sampleDuration = null,
		int sampleCount = 120)
	{
		if (!PlanetOrbitData.TryGetValue(planetName, out var data))
		{
			throw new ArgumentException($"Unknown planet '{planetName}'", nameof(planetName));
		}

		sampleDuration ??= TimeSpan.FromDays(30); // monthly samples by default

		TimeSpan totalDuration = sampleCount * sampleDuration.Value;
		DateTime startTime = endTime - totalDuration;

		var series = new List<TimeRangeValue>(sampleCount);
		DateTime currentTime = startTime;

		double avg = (data.MinAU + data.MaxAU) / 2;
		double amp = (data.MaxAU - data.MinAU) / 2;

		for (int i = 0; i < sampleCount; i++)
		{
			double timeYears = (currentTime - new DateTime(2000, 1, 1)).TotalDays / DaysInAYear;
			double phase = 2 * Math.PI * (timeYears % data.PeriodYears) / data.PeriodYears;
			double distance = avg + amp * Math.Sin(phase);

			series.Add(new TimeRangeValue
			{
				StartTime = currentTime,
				EndTime = currentTime + sampleDuration.Value,
				Value = Math.Round(distance, 3)
			});

			currentTime += sampleDuration.Value;
		}

		return series;
	}
}
