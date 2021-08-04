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

		public static DateTimeOffset Trim(this DateTimeOffset dateTimeOffset, long ticks)
		{
			DateTime dateTime = dateTimeOffset.UtcDateTime;
			return new DateTimeOffset(dateTime.Trim(ticks));
		}

		public static DateTime Ceil(this DateTime date, long ticks = TimeSpan.TicksPerSecond)
		{
			return new DateTime(date.Ticks + ticks - 1, date.Kind).Trim();
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
	}
}
