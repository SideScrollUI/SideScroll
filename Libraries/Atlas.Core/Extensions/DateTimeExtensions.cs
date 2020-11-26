using System;

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

		public static string FormattedDecimal(this TimeSpan timeSpan)
		{
			string format = "#,0.#";
			if (timeSpan.TotalDays > 7)
				return (timeSpan.TotalDays / 7).ToString(format) + " Weeks";
			else if (timeSpan.TotalDays == 7)
				return (timeSpan.TotalDays / 7).ToString(format) + " Week";
			else if (timeSpan.TotalDays > 1)
				return timeSpan.TotalDays.ToString(format) + " Days";
			else if (timeSpan.TotalDays == 1)
				return timeSpan.TotalDays.ToString(format) + " Day";
			else if (timeSpan.TotalHours > 1)
				return timeSpan.TotalHours.ToString(format) + " Hours";
			else if (timeSpan.TotalHours == 1)
				return timeSpan.TotalHours.ToString(format) + " Hour";
			else if (timeSpan.TotalMinutes > 1)
				return timeSpan.TotalMinutes.ToString(format) + " Minutes";
			else if (timeSpan.TotalMinutes == 1)
				return timeSpan.TotalMinutes.ToString(format) + " Minute";
			else
				return timeSpan.TotalSeconds + " Seconds";
		}
	}
}
