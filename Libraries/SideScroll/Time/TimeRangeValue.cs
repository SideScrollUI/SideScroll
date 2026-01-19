using SideScroll.Utilities;
using SideScroll.Extensions;
using SideScroll.Attributes;

namespace SideScroll.Time;

/// <summary>
/// Interface for objects that contain tags
/// </summary>
public interface ITags
{
	/// <summary>
	/// Gets the list of tags
	/// </summary>
	List<Tag> Tags { get; }
}

/// <summary>
/// Represents a time range with an associated numeric value and tags
/// </summary>
public class TimeRangeValue : ITags
{
	/// <summary>
	/// Gets or sets the start time of this value's range
	/// </summary>
	[XAxis]
	public DateTime StartTime { get; set; }
	
	/// <summary>
	/// Gets or sets the end time of this value's range
	/// </summary>
	public DateTime EndTime { get; set; }

	/// <summary>
	/// Gets the duration between start and end time
	/// </summary>
	public TimeSpan Duration => EndTime.Subtract(StartTime);

	/// <summary>
	/// Gets the formatted time range text without duration
	/// </summary>
	public string TimeText => DateTimeUtils.FormatTimeRange(StartTime, EndTime, false);

	/// <summary>
	/// Gets or sets the numeric value associated with this time range
	/// </summary>
	[YAxis]
	public double Value { get; set; }

	/// <summary>
	/// Gets or sets the tags associated with this time range value
	/// </summary>
	// [Tags]
	public List<Tag> Tags { get; set; } = [];

	/// <summary>
	/// Gets a comma-separated description of all tags
	/// </summary>
	public string Description => string.Join(", ", Tags);

	/// <summary>
	/// Gets a TimeWindow representing this time range
	/// </summary>
	public TimeWindow TimeWindow => new(StartTime, EndTime);

	public override string ToString() => DateTimeUtils.FormatTimeRange(StartTime, EndTime) + " - " + Value;

	/// <summary>
	/// Initializes a new instance of the TimeRangeValue class
	/// </summary>
	public TimeRangeValue() { }

	/// <summary>
	/// Initializes a new instance with a single point in time
	/// </summary>
	public TimeRangeValue(DateTime startTime)
	{
		StartTime = startTime;
		EndTime = startTime;
	}

	/// <summary>
	/// Initializes a new instance with a time range, value, and tags
	/// </summary>
	public TimeRangeValue(DateTime startTime, DateTime endTime, double value, params Tag[] tags)
	{
		StartTime = startTime;
		EndTime = endTime;
		Value = value;
		Tags = tags.ToList();
	}

	/// <summary>
	/// Initializes a new instance with a time range, value, and tag list
	/// </summary>
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

		TimeSpan minDistance = 2 * periodDuration;
		DateTime? prevTime = null;
		foreach (TimeRangeValue point in input)
		{
			DateTime startTime = point.StartTime;
			if (prevTime != null)
			{
				TimeSpan duration = startTime.Subtract(prevTime.Value);
				duration = TimeSpan.FromTicks(Math.Abs(duration.Ticks));
				minDistance = minDistance.Min(duration);
			}

			prevTime = startTime;
		}
		return periodDuration.Max(minDistance);
	}

	/// <summary>
	/// Fills gaps with NaN and merges consecutive identical values for efficient charting
	/// </summary>
	/// <remarks>
	/// Inserts NaN values between gaps greater than the minimum detected gap so charts will display line breaks
	/// </remarks>
	public static List<TimeRangeValue> FillAndMerge(IEnumerable<TimeRangeValue> input, TimeSpan periodDuration)
	{
		var sorted = input.OrderBy(p => p.StartTime).ToList();
		TimeSpan minGap = GetMinGap(sorted, periodDuration);

		DateTime? prevTime = null;
		List<TimeRangeValue> output = [];
		foreach (TimeRangeValue point in sorted)
		{
			DateTime startTime = point.StartTime;
			if (prevTime != null)
			{
				DateTime expectedTime = prevTime.Value.Add(minGap);
				if (expectedTime < startTime)
				{
					TimeRangeValue insertedPoint = new()
					{
						StartTime = expectedTime.ToUniversalTime(),
						EndTime = startTime.ToUniversalTime(),
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

	/// <summary>
	/// Fills gaps with NaN and merges consecutive identical values within a specified time range for efficient charting
	/// </summary>
	/// <remarks>
	/// Fills the entire time window from startTime to endTime with NaN gaps and merges consecutive identical middle values
	/// </remarks>
	public static List<TimeRangeValue> FillAndMerge(List<TimeRangeValue> input, DateTime startTime, DateTime endTime, TimeSpan periodDuration)
	{
		List<TimeRangeValue> output = [];
		if (input.Count == 0)
		{
			AddGap(startTime, endTime, periodDuration, output);
			return output;
		}

		List<TimeRangeValue> merged = MergeIdenticalMiddleValues(input);

		//bool hasDuration = merged.First().Duration.Ticks > 0;
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
		var sorted = input.OrderBy(p => p.StartTime);

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
		var sorted = input.OrderBy(p => p.StartTime);

		// Merge continuous points with the same value together to improve storage speeds
		TimeRangeValue? firstValue = null;
		List<TimeRangeValue> merged = [];
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
		TimeRangeValue timeRangeValue = new()
		{
			StartTime = startTime,
			EndTime = endTime,
			Value = double.NaN,
		};
		if (timeRangeValue.Duration >= periodDuration)
		{
			output.Add(timeRangeValue);
		}
	}
}
