using SideScroll.Utilities;
using SideScroll.Extensions;
using System.Diagnostics;

namespace SideScroll.Time;

/// <summary>
/// Represents a time period with aggregated values and statistics from multiple TimeRangeValue entries
/// </summary>
public class TimeRangePeriod : ITags
{
	/// <summary>
	/// Gets or sets the start time of this period
	/// </summary>
	public DateTime StartTime { get; set; }
	
	/// <summary>
	/// Gets or sets the minimum start time of values that contributed to this period
	/// </summary>
	public DateTime? MinStartTime { get; set; }

	/// <summary>
	/// Gets or sets the end time of this period
	/// </summary>
	public DateTime EndTime { get; set; }
	
	/// <summary>
	/// Gets or sets the maximum end time of values that contributed to this period
	/// </summary>
	public DateTime? MaxEndTime { get; set; }

	/// <summary>
	/// Gets the duration of this period
	/// </summary>
	public TimeSpan Duration => EndTime.Subtract(StartTime);

	/// <summary>
	/// Gets or sets the name of this period
	/// </summary>
	public string? Name { get; set; }

	/// <summary>
	/// Gets or sets the minimum value encountered in this period
	/// </summary>
	public double MinValue { get; set; } = double.MaxValue;
	
	/// <summary>
	/// Gets or sets the maximum value encountered in this period
	/// </summary>
	public double MaxValue { get; set; } = double.MinValue;

	/// <summary>
	/// Gets or sets the sum of all values in this period
	/// </summary>
	public double Sum { get; set; }
	
	/// <summary>
	/// Gets or sets the total of all values weighted by seconds
	/// </summary>
	public double SummedSecondValues { get; set; }
	
	/// <summary>
	/// Gets or sets the total duration covered by all values in this period
	/// </summary>
	public TimeSpan SummedDurations { get; set; }

	/// <summary>
	/// Gets or sets the count of values in this period
	/// </summary>
	public int Count { get; set; }

	/// <summary>
	/// Gets or sets all tags from all values in this period
	/// </summary>
	public List<Tag> AllTags { get; set; } = [];

	/// <summary>
	/// Gets the consolidated tags with concatenated values for duplicate tag names
	/// </summary>
	public List<Tag> Tags
	{
		get
		{
			// Concatenate tag values with the same tag name
			Dictionary<string, Tag> lookup = [];
			foreach (Tag tag in AllTags)
			{
				if (lookup.TryGetValue(tag.Name!, out Tag? tagBin))
				{
					if (tagBin.Value is string text && tag.Value is string tagText)
					{
						if (!text.Contains(tagText))
						{
							tagBin.Value += ", " + tagText;
						}
					}
				}
				else
				{
					lookup.Add(tag.Name!, new Tag(tag));
				}
			}
			return lookup.Values.ToList();
		}
	}

	public override string ToString() => Name ?? DateTimeUtils.FormatTimeRange(StartTime, EndTime) + " - " + Count;

	/// <summary>
	/// Aggregates time range values into periods of specified duration within a time window
	/// </summary>
	/// <param name="trimPeriods">Whether to trim period boundaries to actual data boundaries</param>
	/// <returns>A list of time periods with aggregated statistics, or null if parameters are invalid</returns>
	public static List<TimeRangePeriod>? Periods(IEnumerable<TimeRangeValue> timeRangeValues, TimeWindow timeWindow, TimeSpan periodDuration, bool trimPeriods = true)
	{
		var periodTimeWindow = new TimeWindow(timeWindow.StartTime, timeWindow.EndTime.Add(periodDuration));

		long windowTicks = periodTimeWindow.Duration.Ticks;
		long periodTicks = periodDuration.Ticks;
		if (windowTicks < 1 || periodTicks < 1)
			return null;

		int numPeriods = (int)(windowTicks / periodTicks);

		DateTime minStartTime = periodTimeWindow.StartTime;//.Trim(periodDuration);
		DateTime maxEndTime = periodTimeWindow.EndTime;

		List<TimeRangePeriod> timeRangePeriods = [];

		for (int i = 0; i <= numPeriods; i++)
		{
			TimeRangePeriod period = new()
			{
				StartTime = minStartTime.AddTicks(i * periodTicks),
				EndTime = minStartTime.AddTicks((i + 1) * periodTicks),
			};
			timeRangePeriods.Add(period);
		}

		foreach (TimeRangeValue timeRangeValue in timeRangeValues)
		{
			if (double.IsNaN(timeRangeValue.Value))
				continue;

			if (timeRangeValue.EndTime < minStartTime || timeRangeValue.StartTime > maxEndTime)
				continue;

			bool hasDuration = timeRangeValue.EndTime > timeRangeValue.StartTime;

			DateTime valueStartTime = timeRangeValue.StartTime.Max(minStartTime);
			DateTime valueEndTime = timeRangeValue.EndTime.Min(maxEndTime);

			for (DateTime valueBinStartTime = valueStartTime; valueBinStartTime < valueEndTime || !hasDuration;)
			{
				long offset = valueBinStartTime.Subtract(minStartTime).Ticks;
				int periodIndex = (int)(offset / periodTicks);
				Debug.Assert(periodIndex >= 0 && periodIndex < timeRangePeriods.Count);
				TimeRangePeriod period = timeRangePeriods[periodIndex];

				DateTime binStartTime = valueStartTime.Max(period.StartTime);
				DateTime binEndTime = valueEndTime.Min(period.EndTime);

				period.MinStartTime = period.MinStartTime?.Min(binStartTime) ?? binStartTime;
				period.MaxEndTime = period.MaxEndTime?.Max(binEndTime) ?? binEndTime;

				period.MinValue = Math.Min(period.MinValue, timeRangeValue.Value);
				period.MaxValue = Math.Max(period.MaxValue, timeRangeValue.Value);

				TimeSpan binDuration = binEndTime.Subtract(binStartTime);
				period.Count++;
				period.AllTags.AddRange(timeRangeValue.Tags);

				if (hasDuration)
				{
					long totalTicks = binDuration.Min(timeRangeValue.Duration).Ticks;
					double weight = (double)binDuration.Ticks / totalTicks;
					period.Sum += weight * timeRangeValue.Value;
					period.SummedDurations += binDuration;
					period.SummedSecondValues += totalTicks * timeRangeValue.Value;
					valueBinStartTime += binDuration;
				}
				else
				{
					period.Sum += timeRangeValue.Value;
					period.SummedDurations += periodDuration;
					period.SummedSecondValues += periodDuration.Ticks * timeRangeValue.Value;
					break;
				}
			}
		}

		if (trimPeriods)
		{
			foreach (TimeRangePeriod period in timeRangePeriods)
			{
				if (period.SummedDurations.Ticks == 0)
					continue;

				period.StartTime = period.MinStartTime ?? period.StartTime;
				period.EndTime = period.MaxEndTime ?? period.EndTime;
			}
		}
		return timeRangePeriods;
	}

	/// <summary>
	/// Calculates the total average value across all periods
	/// </summary>
	public static double TotalAverage(IEnumerable<TimeRangeValue> timeRangeValues, TimeWindow timeWindow, TimeSpan periodDuration)
	{
		var periods = Periods(timeRangeValues, timeWindow, periodDuration);
		if (periods == null)
			return 0;

		double totalSum = 0;
		TimeSpan totalDuration = TimeSpan.Zero;
		foreach (TimeRangePeriod period in periods)
		{
			totalDuration = totalDuration.Add(period.SummedDurations);
			totalSum += period.SummedSecondValues;
		}
		if (totalDuration.Ticks == 0)
			return 0;

		totalDuration = totalDuration.Max(timeWindow.Duration);

		return totalSum / totalDuration.Ticks;
	}

	/// <summary>
	/// Calculates the total sum of all values across all periods
	/// </summary>
	public static double TotalSum(IEnumerable<TimeRangeValue> timeRangeValues, TimeWindow timeWindow, TimeSpan periodDuration)
	{
		var periods = Periods(timeRangeValues, timeWindow, periodDuration);
		if (periods == null)
			return 0;

		double total = periods.Sum(p => p.Sum);
		return total;
	}

	/// <summary>
	/// Calculates the total count of values across all periods
	/// </summary>
	public static int TotalCounts(IEnumerable<TimeRangeValue> timeRangeValues, TimeWindow timeWindow, TimeSpan periodDuration)
	{
		var periods = Periods(timeRangeValues, timeWindow, periodDuration);
		if (periods == null)
			return 0;

		int total = periods.Sum(p => p.Count);
		return total;
	}

	/// <summary>
	/// Finds the minimum value across all time range values within the time window
	/// </summary>
	public static double TotalMinimum(IEnumerable<TimeRangeValue> timeRangeValues, TimeWindow timeWindow)
	{
		double min = timeRangeValues
			.Where(period => !double.IsNaN(period.Value))
			.Where(period => period.EndTime > timeWindow.StartTime && period.StartTime < timeWindow.EndTime)
			.DefaultIfEmpty(new TimeRangeValue())
			.Min(period => period.Value);
		return min;
	}

	/// <summary>
	/// Finds the maximum value across all time range values within the time window
	/// </summary>
	public static double TotalMaximum(IEnumerable<TimeRangeValue> timeRangeValues, TimeWindow timeWindow)
	{
		double max = timeRangeValues
			.Where(period => !double.IsNaN(period.Value))
			.Where(period => period.EndTime > timeWindow.StartTime && period.StartTime < timeWindow.EndTime)
			.DefaultIfEmpty(new TimeRangeValue())
			.Max(period => period.Value);
		return max;
	}

	/// <summary>
	/// Calculates the average value for each period and returns them as time range values
	/// </summary>
	public static List<TimeRangeValue>? PeriodAverages(IEnumerable<TimeRangeValue> timeRangeValues, TimeWindow timeWindow, TimeSpan periodDuration)
	{
		var periods = Periods(timeRangeValues, timeWindow, periodDuration);

		return periods?
			.Where(period => period.SummedDurations.Ticks > 0)
			.Select(period =>
			{
				double average = period.SummedSecondValues / period.SummedDurations.Min(period.Duration).Ticks;
				return new TimeRangeValue(period.StartTime, period.EndTime, average, period.Tags);
			})
			.ToList();
	}

	/// <summary>
	/// Calculates the sum of values for each period and returns them as time range values
	/// </summary>
	public static List<TimeRangeValue>? PeriodSums(IEnumerable<TimeRangeValue> timeRangeValues, TimeWindow timeWindow, TimeSpan periodDuration)
	{
		var periods = Periods(timeRangeValues, timeWindow, periodDuration);

		return periods?
			.Where(period => period.SummedDurations.Ticks > 0)
			.Select(period => new TimeRangeValue(period.StartTime, period.EndTime, period.Sum, period.Tags))
			.ToList();
	}

	/// <summary>
	/// Finds the minimum value for each period and returns them as time range values
	/// </summary>
	public static List<TimeRangeValue>? PeriodMins(IEnumerable<TimeRangeValue> timeRangeValues, TimeWindow timeWindow, TimeSpan periodDuration)
	{
		var periods = Periods(timeRangeValues, timeWindow, periodDuration);

		return periods?
			.Where(period => period.SummedDurations.Ticks > 0)
			.Select(period => new TimeRangeValue(period.StartTime, period.EndTime, period.MinValue, period.Tags))
			.ToList();
	}

	/// <summary>
	/// Finds the maximum value for each period and returns them as time range values
	/// </summary>
	public static List<TimeRangeValue>? PeriodMaxes(IEnumerable<TimeRangeValue> timeRangeValues, TimeWindow timeWindow, TimeSpan periodDuration)
	{
		var periods = Periods(timeRangeValues, timeWindow, periodDuration);

		return periods?
			.Where(period => period.SummedDurations.Ticks > 0)
			.Select(period => new TimeRangeValue(period.StartTime, period.EndTime, period.MaxValue, period.Tags))
			.ToList();
	}

	/// <summary>
	/// Calculates the count of values for each period with automatic period count determination
	/// </summary>
	public static List<TimeRangeValue>? PeriodCounts(IEnumerable<TimeRangeValue> timeRangeValues, DateTime startTime, DateTime endTime, int minPeriods, int maxPeriods)
	{
		return PeriodCounts(timeRangeValues, new TimeWindow(startTime, endTime), minPeriods, maxPeriods);
	}

	/// <summary>
	/// Calculates the count of values for each period with automatic period count determination
	/// </summary>
	public static List<TimeRangeValue>? PeriodCounts(IEnumerable<TimeRangeValue> timeRangeValues, TimeWindow timeWindow, int minPeriods, int maxPeriods)
	{
		long durationTicks = timeWindow.Duration.Ticks;
		int numPeriods = Math.Clamp((int)(durationTicks / TimeSpan.TicksPerMillisecond), minPeriods, maxPeriods);
		long periodTicks = (durationTicks + numPeriods - 1) / numPeriods; // Ceiling division

		return PeriodCounts(timeRangeValues, timeWindow, TimeSpan.FromTicks(periodTicks));
	}

	/// <summary>
	/// Calculates the count of values for each period and returns them as time range values
	/// </summary>
	/// <param name="addGaps">Whether to add NaN gaps between periods with no data</param>
	public static List<TimeRangeValue>? PeriodCounts(IEnumerable<TimeRangeValue> timeRangeValues, TimeWindow timeWindow, TimeSpan periodDuration, bool addGaps = false)
	{
		var periods = Periods(timeRangeValues, timeWindow, periodDuration, false);
		if (periods == null)
			return null;

		// Exclude double.IsNaN?
		// double.IsNaN(period.Value))
		List<TimeRangeValue> periodCounts = periods
			.Where(period => period.SummedDurations.Ticks > 0)
			.Select(period => new TimeRangeValue(period.StartTime, period.EndTime, period.Count, period.Tags))
			.ToList();

		if (addGaps)
		{
			return TimeRangeValue.AddGaps(periodCounts, timeWindow.StartTime, timeWindow.EndTime, periodDuration);
		}

		return periodCounts;
	}
}
