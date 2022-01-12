using System;
using System.Collections.Generic;
using System.Linq;

namespace Atlas.Extensions
{
	public static class TimeSpanExtensions
	{
		public class TimeUnit
		{
			public TimeSpan TimeSpan { get; set; }
			public string Name { get; set; }

			public TimeUnit(TimeSpan timeSpan, string name)
			{
				TimeSpan = timeSpan;
				Name = name;
			}
		}

		public static List<TimeUnit> TimeUnits { get; set; } = new List<TimeUnit>()
		{
			new TimeUnit(TimeSpan.FromDays(7), "Week"),
			new TimeUnit(TimeSpan.FromDays(1), "Day"),
			new TimeUnit(TimeSpan.FromHours(1), "Hour"),
			new TimeUnit(TimeSpan.FromMinutes(1), "Minute"),
			new TimeUnit(TimeSpan.FromSeconds(1), "Second"),
			new TimeUnit(TimeSpan.FromMilliseconds(1), "Millisecond"),
		};

		public static string FormattedDecimal(this TimeSpan timeSpan)
		{
			string format = "#,0.#";
			var absTimeSpan = new TimeSpan(Math.Abs(timeSpan.Ticks));
			foreach (TimeUnit timeUnit in TimeUnits)
			{
				if (absTimeSpan < timeUnit.TimeSpan)
					continue;

				double units = timeSpan.TotalSeconds / timeUnit.TimeSpan.TotalSeconds;
				string value = units.ToString(format) + " " + timeUnit.Name;

				if (absTimeSpan.TotalSeconds > timeUnit.TimeSpan.TotalSeconds)
					value += "s";

				return value;
			}
			return timeSpan.TotalSeconds + " Seconds";
		}

		public static List<TimeSpan> CommonTimeSpans { get; set; } = new List<TimeSpan>()
		{
			TimeSpan.FromSeconds(1),
			TimeSpan.FromSeconds(5),
			TimeSpan.FromSeconds(10),
			TimeSpan.FromSeconds(30),
			TimeSpan.FromMinutes(1),
			TimeSpan.FromMinutes(5),
			TimeSpan.FromMinutes(10),
			TimeSpan.FromMinutes(30),
			TimeSpan.FromHours(1),
			TimeSpan.FromHours(2),
			TimeSpan.FromHours(6),
			TimeSpan.FromHours(12),
			TimeSpan.FromDays(1),
			TimeSpan.FromDays(2),
			TimeSpan.FromDays(3),
			TimeSpan.FromDays(7),
			TimeSpan.FromDays(28),
		};

		public static TimeSpan PeriodDuration(this TimeSpan timeSpan, int numPeriods = 100)
		{
			TimeSpan maxPeriodDuration = TimeSpan.FromSeconds(timeSpan.TotalSeconds * 2 / numPeriods);
			foreach (TimeSpan periodMin in CommonTimeSpans.Reverse<TimeSpan>())
			{
				if (periodMin <= maxPeriodDuration)
					return periodMin;
			}
			return CommonTimeSpans.First();
		}

		public static TimeSpan Trim(this TimeSpan timeSpan, long ticks = TimeSpan.TicksPerSecond)
		{
			return new TimeSpan(timeSpan.Ticks - (timeSpan.Ticks % ticks));
		}

		public static TimeSpan Trim(this TimeSpan timeSpan, TimeSpan roundingInterval)
		{
			return Trim(timeSpan, roundingInterval.Ticks);
		}

		public static TimeSpan Ceil(this TimeSpan timeSpan, long ticks = TimeSpan.TicksPerSecond)
		{
			return new TimeSpan(ticks * ((timeSpan.Ticks + ticks - 1) / ticks));
		}

		public static TimeSpan Max(this TimeSpan first, TimeSpan second)
		{
			return new TimeSpan(Math.Max(first.Ticks, second.Ticks));
		}

		public static TimeSpan Min(this TimeSpan first, TimeSpan second)
		{
			return new TimeSpan(Math.Min(first.Ticks, second.Ticks));
		}
	}
}
