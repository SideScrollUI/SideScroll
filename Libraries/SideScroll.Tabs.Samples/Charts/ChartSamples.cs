using SideScroll.Time;

namespace SideScroll.Tabs.Samples.Charts;

public static class ChartSamples
{
	private static readonly Random _random = new();

	public static List<TimeRangeValue> CreateTimeSeries(
		DateTime endTime,
		TimeSpan? sampleDuration = null,
		int sampleCount = 24,
		double minValue = 0,
		double maxValue = int.MaxValue)
	{
		sampleDuration ??= TimeSpan.FromHours(1);

		TimeSpan totalDuration = sampleCount * sampleDuration.Value;
		DateTime startTime = endTime - totalDuration;

		double range = maxValue - minValue;
		double currentValue = minValue + _random.NextDouble() * range;
		double stepRange = currentValue / 4.0;

		var timeSeries = new List<TimeRangeValue>(sampleCount);
		DateTime currentTime = startTime;

		for (int i = 0; i < sampleCount; i++)
		{
			TimeRangeValue value = new()
			{
				StartTime = currentTime,
				EndTime = currentTime + sampleDuration.Value,
				Value = currentValue
			};

			timeSeries.Add(value);

			// Apply a random fluctuation
			double fluctuation = (_random.NextDouble() - 0.5) * 2 * stepRange;
			currentValue = Math.Clamp(currentValue + fluctuation, minValue, maxValue);
			currentTime += sampleDuration.Value;
		}

		return timeSeries;
	}

	public static List<TimeRangeValue> CreateIdenticalTimeSeries(
		DateTime endTime,
		TimeSpan? sampleDuration = null,
		int sampleCount = 24,
		double value = 1000)
	{
		return CreateTimeSeries(endTime, sampleDuration, sampleCount, value, value);
	}
}
