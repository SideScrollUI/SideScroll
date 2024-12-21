using SideScroll.Time;

namespace SideScroll.Tabs.Samples.Chart;

public static class ChartSamples
{
	private static readonly Random _random = new();

	public static List<TimeRangeValue> CreateTimeSeries(DateTime endTime, int sampleCount = 24, double maxValue = int.MaxValue)
	{
		DateTime startTime = endTime.Subtract(TimeSpan.FromHours(sampleCount));
		maxValue = Math.Min(maxValue, Math.Max(1, _random.Next()));
		double delta = maxValue / 4;
		double prevValue = _random.Next() % maxValue;
		var list = new List<TimeRangeValue>();
		for (int i = 0; i < sampleCount; i++)
		{
			var value = new TimeRangeValue
			{
				StartTime = startTime,
				EndTime = startTime.AddHours(1),
				Value = prevValue,
			};
			prevValue = Math.Abs(prevValue + (_random.Next() % delta - delta / 2));
			list.Add(value);
			startTime = startTime.AddHours(1);
		}
		return list;
	}

	public static List<TimeRangeValue> CreateIdenticalTimeSeries(DateTime endTime, int sampleCount = 24, double value = 1000)
	{
		DateTime startTime = endTime.Subtract(TimeSpan.FromHours(sampleCount));
		var list = new List<TimeRangeValue>();
		for (int i = 0; i < sampleCount; i++)
		{
			var timeRangeValue = new TimeRangeValue
			{
				StartTime = startTime,
				EndTime = startTime.AddHours(1),
				Value = value,
			};
			list.Add(timeRangeValue);
			startTime = startTime.AddHours(1);
		}
		return list;
	}
}
