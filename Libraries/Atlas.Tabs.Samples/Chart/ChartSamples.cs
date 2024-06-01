using Atlas.Core;

namespace Atlas.Tabs.Samples.Chart;

public static class ChartSamples
{
	private static readonly Random _random = new();

	public static List<TimeRangeValue> CreateTimeSeries(DateTime endTime, int sampleCount = 24)
	{
		DateTime startTime = endTime.Subtract(TimeSpan.FromHours(sampleCount));
		int maxValue = Math.Max(1, _random.Next());
		int delta = maxValue / 4;
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

	public static List<TimeRangeValue> CreateIdenticalTimeSeries(DateTime endTime, int sampleCount = 24)
	{
		DateTime startTime = endTime.Subtract(TimeSpan.FromHours(sampleCount));
		var list = new List<TimeRangeValue>();
		for (int i = 0; i < sampleCount; i++)
		{
			var value = new TimeRangeValue
			{
				StartTime = startTime,
				EndTime = startTime.AddHours(1),
				Value = 1000,
			};
			list.Add(value);
			startTime = startTime.AddHours(1);
		}
		return list;
	}
}
