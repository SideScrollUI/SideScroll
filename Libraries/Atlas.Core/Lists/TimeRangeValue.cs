using Atlas.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Atlas.Core
{
	public class TimeRangeValue
	{
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public TimeSpan Duration => EndTime.Subtract(StartTime);

		public string Name { get; set; }
		public double Value { get; set; }

		public override string ToString() => Name;

		public TimeRangeValue()
		{
		}

		public TimeRangeValue(DateTime startTime, DateTime endTime, double value = 0)
		{
			StartTime = startTime;
			EndTime = endTime;
			Value = value;
		}

		public List<TimeRangeValue> SumPeriods(List<TimeRangeValue> dataPoints)//, TimeSpan periodTimeSpan = null)
		{
			//if (periodTimeSpan == null)
			double duration = Math.Ceiling(EndTime.Subtract(StartTime).TotalSeconds);
			int numPeriods = Math.Max(5, Math.Min(200, (int)duration));
			double periodDuration = Math.Ceiling(duration / numPeriods);
			DateTime startTime = StartTime.Trim(TimeSpan.FromSeconds(1));

			if (duration <= 1)
				return null;

			var timeRangeValues = new List<TimeRangeValue>();

			for (int i = 0; i <= numPeriods; i++)
			{
				var bin = new TimeRangeValue()
				{
					StartTime = startTime.AddSeconds(i * periodDuration),
					EndTime = startTime.AddSeconds(i * periodDuration),
				};
				timeRangeValues.Add(bin);
			}

			foreach (var dataPoint in dataPoints)
			{
				DateTime binTime = dataPoint.StartTime;
				if (binTime < startTime)
					binTime = startTime;

				for (; binTime < dataPoint.EndTime && binTime < EndTime; binTime = binTime.AddSeconds(periodDuration))
				{
					double offset = binTime.Subtract(startTime).TotalSeconds;
					DateTime binEndTime = binTime.AddSeconds(periodDuration);
					DateTime binStarted = binTime.Max(startTime);
					DateTime binEnded = binEndTime.Min(EndTime).Min(dataPoint.EndTime);

					TimeSpan binDuration = binEnded.Subtract(binStarted);
					int period = (int)(offset / periodDuration);
					Debug.Assert(period < timeRangeValues.Count);

					timeRangeValues[period].Value += binDuration.TotalMinutes;
				}
			}
			return timeRangeValues;
		}
	}
}
