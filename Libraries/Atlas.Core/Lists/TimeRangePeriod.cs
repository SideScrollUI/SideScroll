using Atlas.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Atlas.Core
{
	public class TimeRangePeriod
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

		public override string ToString() => Name ?? DateTimeUtils.FormatTimeRange(StartTime, EndTime) + " - " + Count;

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

			foreach (var dataPoint in dataPoints)
			{
				if (double.IsNaN(dataPoint.Value))
					continue;
				DateTime binTime = dataPoint.StartTime;
				if (binTime < startTime)
					binTime = startTime;

				for (; binTime < dataPoint.EndTime && binTime < endTime; binTime = binTime.AddSeconds(periodSeconds))
				{
					double offset = binTime.Subtract(startTime).TotalSeconds;
					int period = (int)(offset / periodSeconds);
					Debug.Assert(period < timeRangeValues.Count);
					var bin = timeRangeValues[period];

					DateTime binStartTime = dataPoint.StartTime.Max(bin.StartTime);
					DateTime binEndTime = dataPoint.EndTime.Min(bin.EndTime);

					bin.MinStartTime = bin.MinStartTime?.Min(binStartTime) ?? binStartTime;
					bin.MaxEndTime = bin.MaxEndTime?.Max(binEndTime) ?? binEndTime;
					bin.MinValue = Math.Min(bin.MinValue, dataPoint.Value);
					bin.MaxValue = Math.Max(bin.MaxValue, dataPoint.Value);

					TimeSpan binDuration = binEndTime.Subtract(binStartTime);

					bin.Sum += binDuration.TotalMinutes * dataPoint.Value;
					bin.SummedDurations += binDuration;
				}
			}

			foreach (var bin in timeRangeValues)
			{
				if (bin.SummedDurations.TotalMinutes == 0.0)
					continue;
				double binMinutes = bin.Duration.TotalMinutes;
				bin.StartTime = bin.MinStartTime ?? bin.StartTime;
				bin.EndTime = bin.MaxEndTime ?? bin.EndTime;
			}
			return timeRangeValues;
		}

		public static List<TimeRangeValue> SumPeriods(List<TimeRangeValue> dataPoints, DateTime startTime, DateTime endTime, TimeSpan periodDuration)
		{
			var periods = Periods(dataPoints, startTime, endTime, periodDuration);
			var timeRangeValues = new List<TimeRangeValue>();
			foreach (var bin in periods)
			{
				if (bin.SummedDurations.TotalMinutes == 0.0)
					continue;
				double binMinutes = bin.Duration.TotalMinutes;
				double averageSum = bin.Sum * binMinutes / bin.SummedDurations.Min(bin.Duration).TotalMinutes;
				timeRangeValues.Add(new TimeRangeValue(bin.StartTime, bin.EndTime, averageSum));
			}
			return timeRangeValues;
		}

		public static List<TimeRangeValue> MinPeriods(List<TimeRangeValue> dataPoints, DateTime startTime, DateTime endTime, TimeSpan periodDuration)
		{
			var periods = Periods(dataPoints, startTime, endTime, periodDuration);
			var timeRangeValues = new List<TimeRangeValue>();
			foreach (var bin in periods)
			{
				if (bin.SummedDurations.TotalMinutes == 0.0)
					continue;
				timeRangeValues.Add(new TimeRangeValue(bin.StartTime, bin.EndTime, bin.MinValue));
			}
			return timeRangeValues;
		}

		public static List<TimeRangeValue> MaxPeriods(List<TimeRangeValue> dataPoints, DateTime startTime, DateTime endTime, TimeSpan periodDuration)
		{
			var periods = Periods(dataPoints, startTime, endTime, periodDuration);
			var timeRangeValues = new List<TimeRangeValue>();
			foreach (var bin in periods)
			{
				if (bin.SummedDurations.TotalMinutes == 0.0)
					continue;
				timeRangeValues.Add(new TimeRangeValue(bin.StartTime, bin.EndTime, bin.MaxValue));
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
