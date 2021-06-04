using Atlas.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Atlas.Core
{
	public class TimeRangePeriod : ITags
	{
		public DateTime StartTime { get; set; }
		public DateTime? MinStartTime { get; set; }

		public DateTime EndTime { get; set; }
		public DateTime? MaxEndTime { get; set; }

		public TimeSpan Duration => EndTime.Subtract(StartTime);

		public string Name { get; set; }

		public double MinValue { get; set; } = double.MaxValue;
		public double MaxValue { get; set; } = double.MinValue;

		public double Sum { get; set; }
		public double SummedSecondValues { get; set; } // Total of all values each second
		public TimeSpan SummedDurations { get; set; }

		public int Count { get; set; }

		public List<Tag> AllTags { get; set; } = new List<Tag>();

		public List<Tag> Tags
		{
			get
			{
				// Concatenate tag values with the same tag name
				var lookup = new Dictionary<string, Tag>();
				foreach (Tag tag in AllTags)
				{
					if (lookup.TryGetValue(tag.Name, out Tag tagBin))
					{
						if (tagBin.Value is string text && tag.Value is string tagText)
						{
							if (!text.Contains(tagText))
								tagBin.Value += ", " + tagText;
						}
					}
					else
					{
						lookup.Add(tag.Name, new Tag(tag));
					}
				}
				return lookup.Values.ToList();
			}
		}

		public override string ToString() => Name ?? DateTimeUtils.FormatTimeRange(StartTime, EndTime) + " - " + Count;

		// Sum the provided datapoints using the specified period
		public static List<TimeRangePeriod> Periods(List<TimeRangeValue> timeRangeValues, TimeWindow timeWindow, TimeSpan periodDuration)
		{
			double windowSeconds = Math.Ceiling(timeWindow.Duration.TotalSeconds);
			double periodSeconds = (int)periodDuration.TotalSeconds;
			if (windowSeconds < 1 || periodSeconds < 1)
				return null;

			int numPeriods = (int)Math.Ceiling((windowSeconds + 1) / periodSeconds);

			DateTime startTime = timeWindow.StartTime.Trim();

			var timeRangePeriods = new List<TimeRangePeriod>();

			for (int i = 0; i <= numPeriods; i++)
			{
				var bin = new TimeRangePeriod()
				{
					StartTime = startTime.AddSeconds(i * periodSeconds),
					EndTime = startTime.AddSeconds((i + 1) * periodSeconds),
				};
				timeRangePeriods.Add(bin);
			}

			foreach (var timeRangeValue in timeRangeValues)
			{
				if (double.IsNaN(timeRangeValue.Value))
					continue;

				if (timeRangeValue.StartTime == timeRangeValue.EndTime)
					timeRangeValue.EndTime = timeRangeValue.StartTime.Add(periodDuration);

				DateTime valueStartTime = timeRangeValue.StartTime.Max(startTime);
				DateTime valueEndTime = timeRangeValue.EndTime.Min(timeWindow.EndTime);

				for (DateTime valueBinStartTime = valueStartTime; valueBinStartTime < valueEndTime;)
				{
					double offset = valueBinStartTime.Subtract(startTime).TotalSeconds;
					int period = (int)(offset / periodSeconds);
					Debug.Assert(period >= 0 && period < timeRangePeriods.Count);
					TimeRangePeriod bin = timeRangePeriods[period];

					DateTime binStartTime = valueStartTime.Max(bin.StartTime);
					DateTime binEndTime = valueEndTime.Min(bin.EndTime);

					bin.MinStartTime = bin.MinStartTime?.Min(binStartTime) ?? binStartTime;
					bin.MaxEndTime = bin.MaxEndTime?.Max(binEndTime) ?? binEndTime;

					bin.MinValue = Math.Min(bin.MinValue, timeRangeValue.Value);
					bin.MaxValue = Math.Max(bin.MaxValue, timeRangeValue.Value);

					TimeSpan binDuration = binEndTime.Subtract(binStartTime);

					double totalSeconds = binDuration.Min(timeRangeValue.Duration).TotalSeconds;
					bin.Sum += binDuration.TotalSeconds / timeRangeValue.Duration.TotalSeconds * timeRangeValue.Value;
					bin.SummedDurations += binDuration;
					bin.SummedSecondValues += totalSeconds * timeRangeValue.Value;
					bin.Count++;
					bin.AllTags.AddRange(timeRangeValue.Tags);
					valueBinStartTime += binDuration;
				}
			}

			foreach (var bin in timeRangePeriods)
			{
				if (bin.SummedDurations.TotalSeconds == 0.0)
					continue;

				bin.StartTime = bin.MinStartTime ?? bin.StartTime;
				bin.EndTime = bin.MaxEndTime ?? bin.EndTime;
			}
			return timeRangePeriods;
		}

		public static double TotalAverage(List<TimeRangeValue> dataPoints, TimeWindow timeWindow, TimeSpan periodDuration)
		{
			var periods = Periods(dataPoints, timeWindow, periodDuration);
			if (periods == null)
				return 0;

			double totalSum = 0;
			TimeSpan totalDuration;
			foreach (var period in periods)
			{
				totalDuration = totalDuration.Add(period.SummedDurations);
				totalSum += period.SummedSecondValues;
			}
			if (totalDuration.TotalSeconds == 0.0)
				return 0;

			return totalSum / totalDuration.TotalSeconds;
		}

		public static double TotalSum(List<TimeRangeValue> dataPoints, TimeWindow timeWindow, TimeSpan periodDuration)
		{
			var periods = Periods(dataPoints, timeWindow, periodDuration);
			if (periods == null)
				return 0;

			double total = 0;
			foreach (var period in periods)
			{
				total += period.Sum;
			}
			return total;
		}

		public static double TotalMinimum(List<TimeRangeValue> dataPoints, TimeWindow timeWindow)
		{
			double min = 0;
			foreach (var period in dataPoints)
			{
				if (double.IsNaN(period.Value))
					continue;

				if (period.EndTime > timeWindow.StartTime && period.StartTime < timeWindow.EndTime)
					min = Math.Min(min, period.Value);
			}
			return min;
		}

		public static double TotalMaximum(List<TimeRangeValue> dataPoints, TimeWindow timeWindow)
		{
			double max = 0;
			foreach (var period in dataPoints)
			{
				if (double.IsNaN(period.Value))
					continue;

				if (period.EndTime > timeWindow.StartTime && period.StartTime < timeWindow.EndTime)
					max = Math.Max(max, period.Value);
			}
			return max;
		}

		public static List<TimeRangeValue> PeriodAverages(List<TimeRangeValue> dataPoints, TimeWindow timeWindow, TimeSpan periodDuration)
		{
			var periods = Periods(dataPoints, timeWindow, periodDuration);
			if (periods == null)
				return null;

			var timeRangeValues = new List<TimeRangeValue>();
			foreach (var period in periods)
			{
				if (period.SummedDurations.TotalSeconds == 0.0)
					continue;

				double average = period.SummedSecondValues / period.SummedDurations.Min(period.Duration).TotalSeconds;
				timeRangeValues.Add(new TimeRangeValue(period.StartTime, period.EndTime, average, period.Tags));
			}
			return timeRangeValues;
		}

		public static List<TimeRangeValue> PeriodSums(List<TimeRangeValue> dataPoints, TimeWindow timeWindow, TimeSpan periodDuration)
		{
			var periods = Periods(dataPoints, timeWindow, periodDuration);
			if (periods == null)
				return null;

			var timeRangeValues = new List<TimeRangeValue>();
			foreach (var period in periods)
			{
				if (period.SummedDurations.TotalSeconds == 0.0)
					continue;

				timeRangeValues.Add(new TimeRangeValue(period.StartTime, period.EndTime, period.Sum, period.Tags));
			}
			return timeRangeValues;
		}

		public static List<TimeRangeValue> PeriodMins(List<TimeRangeValue> dataPoints, TimeWindow timeWindow, TimeSpan periodDuration)
		{
			var periods = Periods(dataPoints, timeWindow, periodDuration);
			if (periods == null)
				return null;

			var timeRangeValues = new List<TimeRangeValue>();
			foreach (var period in periods)
			{
				if (period.SummedDurations.TotalSeconds == 0.0)
					continue;

				timeRangeValues.Add(new TimeRangeValue(period.StartTime, period.EndTime, period.MinValue, period.Tags));
			}
			return timeRangeValues;
		}

		public static List<TimeRangeValue> PeriodMaxes(List<TimeRangeValue> dataPoints, TimeWindow timeWindow, TimeSpan periodDuration)
		{
			var periods = Periods(dataPoints, timeWindow, periodDuration);
			if (periods == null)
				return null;

			var timeRangeValues = new List<TimeRangeValue>();
			foreach (var period in periods)
			{
				if (period.SummedDurations.TotalSeconds == 0.0)
					continue;

				timeRangeValues.Add(new TimeRangeValue(period.StartTime, period.EndTime, period.MaxValue, period.Tags));
			}
			return timeRangeValues;
		}

		public static List<TimeRangeValue> PeriodCounts(List<TimeRangeValue> dataPoints, DateTime startTime, DateTime endTime, int minPeriods, int maxPeriods)
		{
			return PeriodCounts(dataPoints, new TimeWindow(startTime, endTime), minPeriods, maxPeriods);
		}

		public static List<TimeRangeValue> PeriodCounts(List<TimeRangeValue> dataPoints, TimeWindow timeWindow, int minPeriods, int maxPeriods)
		{
			double durationSeconds = Math.Ceiling(timeWindow.Duration.TotalSeconds);
			int numPeriods = Math.Max(minPeriods, Math.Min(maxPeriods, (int)durationSeconds));
			double periodDuration = Math.Ceiling(durationSeconds / numPeriods);

			return PeriodCounts(dataPoints, timeWindow, TimeSpan.FromSeconds(periodDuration));
		}

		public static List<TimeRangeValue> PeriodCounts(List<TimeRangeValue> dataPoints, TimeWindow timeWindow, TimeSpan periodDuration, bool addGaps = false)
		{
			var periods = Periods(dataPoints, timeWindow, periodDuration);
			if (periods == null)
				return null;

			var timeRangeValues = new List<TimeRangeValue>();
			foreach (var period in periods)
			{
				if (addGaps && period.Count == 0)
					continue;

				timeRangeValues.Add(new TimeRangeValue(period.StartTime, period.EndTime, period.Count, period.Tags));
			}

			if (addGaps)
				return TimeRangeValue.AddGaps(timeRangeValues, timeWindow.StartTime, timeWindow.EndTime, periodDuration);

			return timeRangeValues;
		}

		/*public List<TimeRangeValue> SumPeriods(List<TimeRangeValue> dataPoints)
		{
			double duration = Math.Ceiling(EndTime.Subtract(StartTime).TotalSeconds);
			int numPeriods = Math.Max(5, Math.Min(200, (int)duration));
			double periodDuration = Math.Ceiling(duration / numPeriods);

			return SumPeriods(dataPoints, periodDuration);
		}*/
	}
}
