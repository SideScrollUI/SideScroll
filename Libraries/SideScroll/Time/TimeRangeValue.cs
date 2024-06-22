using SideScroll.Utilities;
using SideScroll.Extensions;

namespace SideScroll.Time;

public interface ITags
{
	List<Tag> Tags { get; }
}

public class TimeRangeValue : ITags
{
	[XAxis]
	public DateTime StartTime { get; set; }
	public DateTime EndTime { get; set; }

	public TimeSpan Duration => EndTime.Subtract(StartTime);

	public string TimeText => DateTimeUtils.FormatTimeRange(StartTime, EndTime, false);

	public string? Name { get; set; }

	[YAxis]
	public double Value { get; set; }

	//[Tags]
	public List<Tag> Tags { get; set; } = [];

	public string Description => string.Join(", ", Tags);

	public TimeWindow TimeWindow => new(StartTime, EndTime);

	public override string ToString() => Name ?? DateTimeUtils.FormatTimeRange(StartTime, EndTime) + " - " + Value;

	public TimeRangeValue() { }

	public TimeRangeValue(DateTime startTime)
	{
		StartTime = startTime;
		EndTime = startTime;
	}

	public TimeRangeValue(DateTime startTime, DateTime endTime, double value, params Tag[] tags)
	{
		StartTime = startTime;
		EndTime = endTime;
		Value = value;
		Tags = tags.ToList();
	}

	public TimeRangeValue(DateTime startTime, DateTime endTime, double value, List<Tag> tags)
	{
		StartTime = startTime;
		EndTime = endTime;
		Value = value;
		Tags = tags;
	}

	private static TimeSpan GetMinGap(List<TimeRangeValue> input, TimeSpan periodDuration)
	{
		if (input.Count < 10)
			return periodDuration;

		TimeSpan minDistance = TimeSpan.FromSeconds(2 * (int)periodDuration.TotalSeconds);
		DateTime? prevTime = null;
		foreach (TimeRangeValue point in input)
		{
			DateTime startTime = point.StartTime;
			if (prevTime != null)
			{
				TimeSpan duration = startTime.Subtract(prevTime.Value);
				duration = TimeSpan.FromSeconds(Math.Abs((int)duration.TotalSeconds));
				minDistance = minDistance.Min(duration);
			}

			prevTime = startTime;
		}
		return periodDuration.Max(minDistance);
	}

	// Adds a single NaN point between all gaps greater than minGap so the chart will add gaps in lines
	public static List<TimeRangeValue> AddGaps(IEnumerable<TimeRangeValue> input, TimeSpan periodDuration)
	{
		var sorted = input.OrderBy(p => p.StartTime).ToList();
		TimeSpan minGap = GetMinGap(sorted, periodDuration);

		DateTime? prevTime = null;
		var output = new List<TimeRangeValue>();
		foreach (TimeRangeValue point in sorted)
		{
			DateTime startTime = point.StartTime;
			double value = point.Value;
			if (prevTime != null)
			{
				DateTime expectedTime = prevTime.Value.Add(minGap);
				if (expectedTime < startTime)
				{
					var insertedPoint = new TimeRangeValue
					{
						StartTime = expectedTime.ToUniversalTime(),
						Value = double.NaN,
					};
					output.Add(insertedPoint);
				}
			}

			output.Add(point);
			prevTime = point.EndTime;
		}

		return output;
	}

	public static List<TimeRangeValue> AddGaps(List<TimeRangeValue> input, DateTime startTime, DateTime endTime, TimeSpan periodDuration)
	{
		var output = new List<TimeRangeValue>();
		if (input.Count == 0)
		{
			AddGap(startTime, endTime, periodDuration, output);
			return output;
		}

		List<TimeRangeValue> merged = MergeIdenticalMiddleValues(input);

		//bool hasDuration = merged.First().Duration.TotalSeconds > 0;
		DateTime prevTime = startTime;
		foreach (TimeRangeValue point in merged)
		{
			AddGap(prevTime, point.StartTime, periodDuration, output);
			output.Add(point);
			prevTime = point.EndTime;
		}
		AddGap(prevTime, endTime, periodDuration, output);
		return output;
	}

	private static List<TimeRangeValue> MergeIdenticalValues(IEnumerable<TimeRangeValue> input)
	{
		var sorted = input.OrderBy(p => p.StartTime).ToList();

		// Merge continuous points with the same value together to improve storage speeds
		var merged = new List<TimeRangeValue>();
		TimeRangeValue? prevPoint = null;
		foreach (TimeRangeValue timeRangeValue in sorted)
		{
			if (prevPoint != null && prevPoint.EndTime == timeRangeValue.StartTime && prevPoint.Value == timeRangeValue.Value)
			{
				prevPoint.EndTime = timeRangeValue.EndTime;
				continue;
			}
			merged.Add(timeRangeValue);
			prevPoint = timeRangeValue;
		}

		return merged;
	}

	// Merge all continuous identical values, increasing the size of the first and leaving the last
	// This works better for line graphs since the end point will still be represented
	private static List<TimeRangeValue> MergeIdenticalMiddleValues(IEnumerable<TimeRangeValue> input)
	{
		var sorted = input.OrderBy(p => p.StartTime).ToList();

		// Merge continuous points with the same value together to improve storage speeds
		TimeRangeValue? firstValue = null;
		var merged = new List<TimeRangeValue>();
		foreach (TimeRangeValue timeRangeValue in sorted)
		{
			TimeRangeValue? previousValue = merged.LastOrDefault();
			if (previousValue != null)
			{
				// Todo: handle Tags
				if (previousValue.EndTime == timeRangeValue.StartTime && previousValue.Value == timeRangeValue.Value)
				{
					if (firstValue != null)
					{
						// Add the previous value's length onto the first value and remove the previous
						firstValue.EndTime = previousValue.EndTime;
						merged.RemoveAt(merged.Count - 1);
					}
					else
					{
						firstValue = previousValue;
					}
				}
				else
				{
					firstValue = null;
				}
			}

			merged.Add(timeRangeValue);
		}

		return merged;
	}

	private static void AddGap(DateTime startTime, DateTime endTime, TimeSpan periodDuration, List<TimeRangeValue> output)
	{
		var timeRangeValue = new TimeRangeValue
		{
			StartTime = startTime,
			EndTime = endTime,
			Value = double.NaN,
		};
		if (timeRangeValue.Duration >= periodDuration)
			output.Add(timeRangeValue);
	}

	// Add NaN points for each period duration between the start/end times
	/*private static DateTime FillGaps(DateTime startTime, DateTime endTime, int periodDuration, List<TimeRangeValue> output)
	{
		int maxGap = periodDuration * 2;

		while (true)
		{
			TimeSpan timeSpan = endTime.Subtract(startTime);
			if (timeSpan.TotalSeconds <= maxGap)
				break;

			DateTime expectedTime = startTime.AddSeconds(periodDuration);
			var insertedPoint = new TimeRangeValue()
			{
				StartTime = expectedTime.ToUniversalTime(),
				EndTime = expectedTime.ToUniversalTime().AddSeconds(periodDuration),
				Value = double.NaN,
			};

			output.Add(insertedPoint);
			startTime = expectedTime;
		}

		return startTime;
	}*/
}
