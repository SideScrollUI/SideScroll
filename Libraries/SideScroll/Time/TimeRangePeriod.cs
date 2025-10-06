using SideScroll.Utilities;
using SideScroll.Extensions;
using System.Diagnostics;

namespace SideScroll.Time;

public class TimeRangePeriod : ITags
{
	public DateTime StartTime { get; set; }
	public DateTime? MinStartTime { get; set; }

	public DateTime EndTime { get; set; }
	public DateTime? MaxEndTime { get; set; }

	public TimeSpan Duration => EndTime.Subtract(StartTime);

	public string? Name { get; set; }

	public double MinValue { get; set; } = double.MaxValue;
	public double MaxValue { get; set; } = double.MinValue;

	public double Sum { get; set; }
	public double SummedSecondValues { get; set; } // Total of all values each second
	public TimeSpan SummedDurations { get; set; }

	public int Count { get; set; }

	public List<Tag> AllTags { get; set; } = [];

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

	// Sum the provided datapoints using the specified period
	public static List<TimeRangePeriod>? Periods(IEnumerable<TimeRangeValue> timeRangeValues, TimeWindow timeWindow, TimeSpan periodDuration, bool trimPeriods = true)
	{
		var periodTimeWindow = new TimeWindow(timeWindow.StartTime, timeWindow.EndTime.Add(periodDuration));

		double windowSeconds = Math.Ceiling(periodTimeWindow.Duration.TotalSeconds);
		double periodSeconds = (int)periodDuration.TotalSeconds;
		if (windowSeconds < 1 || periodSeconds < 1)
			return null;

		int numPeriods = (int)(windowSeconds / periodSeconds);

		DateTime minStartTime = periodTimeWindow.StartTime.Trim();
		DateTime maxEndTime = periodTimeWindow.EndTime;

		List<TimeRangePeriod> timeRangePeriods = [];

		for (int i = 0; i <= numPeriods; i++)
		{
			TimeRangePeriod period = new()
			{
				StartTime = minStartTime.AddSeconds(i * periodSeconds),
				EndTime = minStartTime.AddSeconds((i + 1) * periodSeconds),
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
				double offset = valueBinStartTime.Subtract(minStartTime).TotalSeconds;
				int periodIndex = (int)(offset / periodSeconds);
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
					double totalSeconds = binDuration.Min(timeRangeValue.Duration).TotalSeconds;
					//bin.Sum += binDuration.TotalSeconds / timeRangeValue.Duration.TotalSeconds * timeRangeValue.Value;
					period.Sum += binDuration.TotalSeconds / totalSeconds * timeRangeValue.Value;
					period.SummedDurations += binDuration;
					period.SummedSecondValues += totalSeconds * timeRangeValue.Value;
					valueBinStartTime += binDuration;
				}
				else
				{
					period.Sum += timeRangeValue.Value;
					period.SummedDurations += periodDuration;
					period.SummedSecondValues += periodDuration.TotalSeconds * timeRangeValue.Value;
					break;
				}
			}
		}

		if (trimPeriods)
		{
			foreach (TimeRangePeriod period in timeRangePeriods)
			{
				if (period.SummedDurations.TotalSeconds == 0.0)
					continue;

				period.StartTime = period.MinStartTime ?? period.StartTime;
				period.EndTime = period.MaxEndTime ?? period.EndTime;
			}
		}
		return timeRangePeriods;
	}

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
		if (totalDuration.TotalSeconds == 0.0)
			return 0;

		totalDuration = totalDuration.Max(timeWindow.Duration);

		return totalSum / totalDuration.TotalSeconds;
	}

	public static double TotalSum(IEnumerable<TimeRangeValue> timeRangeValues, TimeWindow timeWindow, TimeSpan periodDuration)
	{
		var periods = Periods(timeRangeValues, timeWindow, periodDuration);
		if (periods == null)
			return 0;

		double total = periods.Sum(p => p.Sum);
		return total;
	}

	public static int TotalCounts(IEnumerable<TimeRangeValue> timeRangeValues, TimeWindow timeWindow, TimeSpan periodDuration)
	{
		var periods = Periods(timeRangeValues, timeWindow, periodDuration);
		if (periods == null)
			return 0;

		int total = periods.Sum(p => p.Count);
		return total;
	}

	public static double TotalMinimum(IEnumerable<TimeRangeValue> timeRangeValues, TimeWindow timeWindow)
	{
		double min = timeRangeValues
			.Where(period => !double.IsNaN(period.Value))
			.Where(period => period.EndTime > timeWindow.StartTime && period.StartTime < timeWindow.EndTime)
			.DefaultIfEmpty(new TimeRangeValue())
			.Min(period => period.Value);
		return min;
	}

	public static double TotalMaximum(IEnumerable<TimeRangeValue> timeRangeValues, TimeWindow timeWindow)
	{
		double max = timeRangeValues
			.Where(period => !double.IsNaN(period.Value))
			.Where(period => period.EndTime > timeWindow.StartTime && period.StartTime < timeWindow.EndTime)
			.DefaultIfEmpty(new TimeRangeValue())
			.Max(period => period.Value);
		return max;
	}

	public static List<TimeRangeValue>? PeriodAverages(IEnumerable<TimeRangeValue> timeRangeValues, TimeWindow timeWindow, TimeSpan periodDuration)
	{
		var periods = Periods(timeRangeValues, timeWindow, periodDuration);

		return periods?
			.Where(period => period.SummedDurations.TotalSeconds > 0.0)
			.Select(period =>
			{
				double average = period.SummedSecondValues / period.SummedDurations.Min(period.Duration).TotalSeconds;
				return new TimeRangeValue(period.StartTime, period.EndTime, average, period.Tags);
			})
			.ToList();
	}

	public static List<TimeRangeValue>? PeriodSums(IEnumerable<TimeRangeValue> timeRangeValues, TimeWindow timeWindow, TimeSpan periodDuration)
	{
		var periods = Periods(timeRangeValues, timeWindow, periodDuration);

		return periods?
			.Where(period => period.SummedDurations.TotalSeconds > 0.0)
			.Select(period => new TimeRangeValue(period.StartTime, period.EndTime, period.Sum, period.Tags))
			.ToList();
	}

	public static List<TimeRangeValue>? PeriodMins(IEnumerable<TimeRangeValue> timeRangeValues, TimeWindow timeWindow, TimeSpan periodDuration)
	{
		var periods = Periods(timeRangeValues, timeWindow, periodDuration);

		return periods?
			.Where(period => period.SummedDurations.TotalSeconds > 0.0)
			.Select(period => new TimeRangeValue(period.StartTime, period.EndTime, period.MinValue, period.Tags))
			.ToList();
	}

	public static List<TimeRangeValue>? PeriodMaxes(IEnumerable<TimeRangeValue> timeRangeValues, TimeWindow timeWindow, TimeSpan periodDuration)
	{
		var periods = Periods(timeRangeValues, timeWindow, periodDuration);

		return periods?
			.Where(period => period.SummedDurations.TotalSeconds > 0.0)
			.Select(period => new TimeRangeValue(period.StartTime, period.EndTime, period.MaxValue, period.Tags))
			.ToList();
	}

	public static List<TimeRangeValue>? PeriodCounts(IEnumerable<TimeRangeValue> timeRangeValues, DateTime startTime, DateTime endTime, int minPeriods, int maxPeriods)
	{
		return PeriodCounts(timeRangeValues, new TimeWindow(startTime, endTime), minPeriods, maxPeriods);
	}

	public static List<TimeRangeValue>? PeriodCounts(IEnumerable<TimeRangeValue> timeRangeValues, TimeWindow timeWindow, int minPeriods, int maxPeriods)
	{
		double durationSeconds = Math.Ceiling(timeWindow.Duration.TotalSeconds);
		int numPeriods = Math.Clamp((int)durationSeconds, minPeriods, maxPeriods);
		double periodDuration = Math.Ceiling(durationSeconds / numPeriods);

		return PeriodCounts(timeRangeValues, timeWindow, TimeSpan.FromSeconds(periodDuration));
	}

	public static List<TimeRangeValue>? PeriodCounts(IEnumerable<TimeRangeValue> timeRangeValues, TimeWindow timeWindow, TimeSpan periodDuration, bool addGaps = false)
	{
		var periods = Periods(timeRangeValues, timeWindow, periodDuration, false);
		if (periods == null)
			return null;

		// Exclude double.IsNaN?
		// double.IsNaN(period.Value))
		List<TimeRangeValue> periodCounts = periods
			.Where(period => period.SummedDurations.TotalSeconds > 0.0)
			.Select(period => new TimeRangeValue(period.StartTime, period.EndTime, period.Count, period.Tags))
			.ToList();

		if (addGaps)
		{
			return TimeRangeValue.AddGaps(periodCounts, timeWindow.StartTime, timeWindow.EndTime, periodDuration);
		}

		return periodCounts;
	}
}
