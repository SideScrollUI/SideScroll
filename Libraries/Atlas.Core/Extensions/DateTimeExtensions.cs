using System;
using System.Collections.Generic;

namespace Atlas.Extensions
{
	public static class DateTimeExtensions
	{
		public static DateTime Trim(this DateTime date, long ticks = TimeSpan.TicksPerSecond)
		{
			return new DateTime(date.Ticks - (date.Ticks % ticks), date.Kind);
		}

		public static DateTime Trim(this DateTime dateTime, TimeSpan timeSpan)
		{
			return Trim(dateTime, timeSpan.Ticks);
		}

		public static TimeSpan Trim(this TimeSpan timeSpan, long ticks = TimeSpan.TicksPerSecond)
		{
			return new TimeSpan(timeSpan.Ticks - (timeSpan.Ticks % ticks));
		}

		public static TimeSpan Trim(this TimeSpan timeSpan, TimeSpan roundingInterval)
		{
			return Trim(timeSpan, roundingInterval.Ticks);
		}

		public static DateTimeOffset Trim(this DateTimeOffset dateTimeOffset, long ticks)
		{
			DateTime dateTime = dateTimeOffset.UtcDateTime;
			return new DateTimeOffset(dateTime.Trim(ticks));
		}

		public static DateTime Max(this DateTime first, DateTime second)
		{
			return new DateTime(Math.Max(first.Ticks, second.Ticks), DateTimeKind.Utc);
		}

		public static DateTime Min(this DateTime first, DateTime second)
		{
			return new DateTime(Math.Min(first.Ticks, second.Ticks), DateTimeKind.Utc);
		}

		public static TimeSpan Age(this DateTime dateTime)
		{
			return DateTime.UtcNow.Subtract(dateTime).Trim();
		}

		public static TimeSpan Max(this TimeSpan first, TimeSpan second)
		{
			return new TimeSpan(Math.Max(first.Ticks, second.Ticks));
		}

		public static TimeSpan Min(this TimeSpan first, TimeSpan second)
		{
			return new TimeSpan(Math.Min(first.Ticks, second.Ticks));
		}

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

		public static List<TimeUnit> TimeUnits = new List<TimeUnit>()
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
			foreach (TimeUnit timeUnit in TimeUnits)
			{
				if (timeSpan < timeUnit.TimeSpan)
					continue;

				double units = timeSpan.TotalSeconds / timeUnit.TimeSpan.TotalSeconds;
				string value = units.ToString(format) + " " + timeUnit.Name;

				if (timeSpan.TotalSeconds > timeUnit.TimeSpan.TotalSeconds)
					value += "s";
				
				return value;
			}
			return timeSpan.TotalSeconds + " Seconds";
		}
	}
}
