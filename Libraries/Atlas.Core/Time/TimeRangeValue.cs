using Atlas.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Atlas.Core
{
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

		public string Name { get; set; }
		[YAxis]
		public double Value { get; set; }
		//[Tags]
		public List<Tag> Tags { get; set; } = new List<Tag>();
		public string Description => string.Join(", ", Tags);

		public override string ToString() => Name ?? DateTimeUtils.FormatTimeRange(StartTime, EndTime) + " - " + Value;

		public TimeRangeValue()
		{
		}

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

		public List<TimeRangeValue> SumPeriods(List<TimeRangeValue> timeRangeValues, TimeSpan periodDuration)
		{
			return TimeRangePeriod.SumPeriods(timeRangeValues, StartTime, EndTime, periodDuration);
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
		public static List<TimeRangeValue> AddGaps(List<TimeRangeValue> input, TimeSpan periodDuration)
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
						var insertedPoint = new TimeRangeValue()
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

		public static List<TimeRangeValue> FillGaps(List<TimeRangeValue> input, DateTime startTime, DateTime endTime, int periodDuration)
		{
			var output = new List<TimeRangeValue>();
			if (input.Count == 0)
			{
				FillGaps(startTime, endTime, periodDuration, output);
				return output;
			}
			var sorted = input.OrderBy(p => p.StartTime).ToList();

			DateTime prevTime = startTime;
			foreach (TimeRangeValue point in sorted)
			{
				prevTime = FillGaps(prevTime, point.StartTime, periodDuration, output);
				output.Add(point);
				prevTime = point.EndTime;
			}
			prevTime = FillGaps(prevTime, endTime, periodDuration, output);
			return output;
		}

		// Add NaN points for each period duration between the start/end times
		private static DateTime FillGaps(DateTime startTime, DateTime endTime, int periodDuration, List<TimeRangeValue> output)
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
		}
	}
}
