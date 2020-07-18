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
		public TimeSpan SummedDurations { get; set; }
		public int Count { get; set; }
		public List<Tag> AllTags { get; set; } = new List<Tag>();
		public List<Tag> Tags
		{
			get
			{
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
						lookup.Add(tag.Name, tag);
					}
				}
				return lookup.Values.ToList();
			}
		}

		public override string ToString() => Name ?? DateTimeUtils.FormatTimeRange(StartTime, EndTime) + " - " + Count;

		// Sum the provided datapoints using the specified period
		public static List<TimeRangePeriod> Periods(List<TimeRangeValue> dataPoints, DateTime startTime, DateTime endTime, TimeSpan periodDuration)
		{
			double periodSeconds = (int)periodDuration.TotalSeconds;
			double duration = Math.Ceiling(endTime.Subtract(startTime).TotalSeconds);
			int numPeriods = (int)Math.Ceiling((duration + 1) / periodSeconds);

			startTime = startTime.Trim();

			if (duration <= 1)
				return null;

			var timeRangeValues = new List<TimeRangePeriod>();

			for (int i = 0; i <= numPeriods; i++)
			{
				var bin = new TimeRangePeriod()
				{
					StartTime = startTime.AddSeconds(i * periodSeconds),
					EndTime = startTime.AddSeconds((i + 1) * periodSeconds),
				};
				timeRangeValues.Add(bin);
			}

			foreach (var timeRangeValue in dataPoints)
			{
				if (double.IsNaN(timeRangeValue.Value))
					continue;
				DateTime binTime = timeRangeValue.StartTime;
				if (binTime < startTime)
					binTime = startTime;

				for (; binTime <= timeRangeValue.EndTime && binTime <= endTime; binTime = binTime.AddSeconds(periodSeconds))
				{
					double offset = binTime.Subtract(startTime).TotalSeconds;
					int period = (int)(offset / periodSeconds);
					Debug.Assert(period < timeRangeValues.Count);
					var bin = timeRangeValues[period];

					DateTime binStartTime = timeRangeValue.StartTime.Max(bin.StartTime);
					DateTime binEndTime = timeRangeValue.EndTime.Min(bin.EndTime);

					bin.MinStartTime = bin.MinStartTime?.Min(binStartTime) ?? binStartTime;
					bin.MaxEndTime = bin.MaxEndTime?.Max(binEndTime) ?? binEndTime;
					bin.MinValue = Math.Min(bin.MinValue, timeRangeValue.Value);
					bin.MaxValue = Math.Max(bin.MaxValue, timeRangeValue.Value);

					TimeSpan binDuration = binEndTime.Subtract(binStartTime);

					bin.Sum += binDuration.TotalMinutes * timeRangeValue.Value;
					bin.SummedDurations += binDuration;
					bin.Count++;
					bin.AllTags.AddRange(timeRangeValue.Tags);
				}
			}

			foreach (var bin in timeRangeValues)
			{
				if (bin.SummedDurations.TotalMinutes == 0.0)
					continue;
				//double binMinutes = bin.Duration.TotalMinutes;
				bin.StartTime = bin.MinStartTime ?? bin.StartTime;
				bin.EndTime = bin.MaxEndTime ?? bin.EndTime;
			}
			return timeRangeValues;
		}

		public static List<TimeRangeValue> SumPeriods(List<TimeRangeValue> dataPoints, DateTime startTime, DateTime endTime, TimeSpan periodDuration)
		{
			var periods = Periods(dataPoints, startTime, endTime, periodDuration);
			var timeRangeValues = new List<TimeRangeValue>();
			foreach (var period in periods)
			{
				if (period.SummedDurations.TotalMinutes == 0.0)
					continue;
				double averageSum = period.Sum / period.SummedDurations.Min(period.Duration).TotalMinutes;
				double chartSum = averageSum * periodDuration.TotalMinutes;
				//double averageSum = period.Sum * (period.SummedDurations.TotalMinutes / period.Duration.TotalMinutes);
				timeRangeValues.Add(new TimeRangeValue(period.StartTime, period.EndTime, chartSum, period.Tags.ToArray()));
			}
			return timeRangeValues;
		}

		public static List<TimeRangeValue> MinPeriods(List<TimeRangeValue> dataPoints, DateTime startTime, DateTime endTime, TimeSpan periodDuration)
		{
			var periods = Periods(dataPoints, startTime, endTime, periodDuration);
			var timeRangeValues = new List<TimeRangeValue>();
			foreach (var period in periods)
			{
				if (period.SummedDurations.TotalMinutes == 0.0)
					continue;
				timeRangeValues.Add(new TimeRangeValue(period.StartTime, period.EndTime, period.MinValue, period.Tags.ToArray()));
			}
			return timeRangeValues;
		}

		public static List<TimeRangeValue> MaxPeriods(List<TimeRangeValue> dataPoints, DateTime startTime, DateTime endTime, TimeSpan periodDuration)
		{
			var periods = Periods(dataPoints, startTime, endTime, periodDuration);
			var timeRangeValues = new List<TimeRangeValue>();
			foreach (var period in periods)
			{
				if (period.SummedDurations.TotalMinutes == 0.0)
					continue;
				timeRangeValues.Add(new TimeRangeValue(period.StartTime, period.EndTime, period.MaxValue, period.Tags.ToArray()));
			}
			return timeRangeValues;
		}

		public static List<TimeRangeValue> PeriodCounts(List<TimeRangeValue> dataPoints, DateTime startTime, DateTime endTime, TimeSpan periodDuration)
		{
			var periods = Periods(dataPoints, startTime, endTime, periodDuration);
			if (periods == null)
				return null;
			var timeRangeValues = new List<TimeRangeValue>();
			foreach (var period in periods)
			{
				//if (period.Count == 0)
				//	continue;
				timeRangeValues.Add(new TimeRangeValue(period.StartTime, period.EndTime, period.Count, period.Tags.ToArray()));
			}
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
