using System;
using System.Collections.Generic;
using System.Linq;

namespace Atlas.Core
{
	public class TimeRangeValue
	{
		[XAxis]
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public TimeSpan Duration => EndTime.Subtract(StartTime);

		public string Name { get; set; }
		[YAxis]
		public double Value { get; set; }

		public override string ToString() => Name ?? DateTimeUtils.FormatTimeRange(StartTime, EndTime) + " - " + Value;

		public TimeRangeValue()
		{
		}

		public TimeRangeValue(DateTime startTime, DateTime endTime, double value = 0)
		{
			StartTime = startTime;
			EndTime = endTime;
			Value = value;
		}

		public List<TimeRangeValue> SumPeriods(List<TimeRangeValue> dataPoints, double periodDuration)
		{
			return TimeRangePeriod.SumPeriods(dataPoints, StartTime, EndTime, periodDuration);
		}

		private static int GetMinGap(List<TimeRangeValue> input, int periodDuration)
		{
			if (input.Count < 10)
				return periodDuration;

			int minDistance = 2 * periodDuration;
			DateTime? prevTime = null;
			foreach (TimeRangeValue point in input)
			{
				DateTime startTime = point.StartTime;
				if (prevTime != null)
				{
					int duration = Math.Abs((int)startTime.Subtract(prevTime.Value).TotalSeconds);
					minDistance = Math.Min(minDistance, duration);
				}

				prevTime = startTime;
			}
			return Math.Max(periodDuration, minDistance);
		}

		// Adds a single NaN point between all gaps greater than minGap so the chart will add gaps in lines
		public static List<TimeRangeValue> AddGaps(List<TimeRangeValue> input, int periodDuration)
		{
			var sorted = input.OrderBy(p => p.StartTime).ToList();
			int minGap = GetMinGap(sorted, periodDuration);

			DateTime? prevTime = null;
			var output = new List<TimeRangeValue>();
			foreach (TimeRangeValue point in sorted)
			{
				DateTime startTime = point.StartTime;
				double value = point.Value;
				if (prevTime != null)
				{
					DateTime expectedTime = prevTime.Value.AddSeconds(minGap);
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
