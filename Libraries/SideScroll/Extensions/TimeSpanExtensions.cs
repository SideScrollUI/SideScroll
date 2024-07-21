using System.Text;

namespace SideScroll.Extensions;

public static class TimeSpanExtensions
{
	public record TimeUnit(TimeSpan TimeSpan, string Name);

	public static List<TimeUnit> TimeUnits { get; set; } =
	[
		new(TimeSpan.FromDays(365.25), "Year"),
		new(TimeSpan.FromDays(7), "Week"),
		new(TimeSpan.FromDays(1), "Day"),
		new(TimeSpan.FromHours(1), "Hour"),
		new(TimeSpan.FromMinutes(1), "Minute"),
		new(TimeSpan.FromSeconds(1), "Second"),
		new(TimeSpan.FromMilliseconds(1), "Millisecond"),
	];

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
			{
				value += "s";
			}

			return value;
		}
		return timeSpan.TotalSeconds + " Seconds";
	}

	// Only show required units, with 3 optional decimal places
	public static string FormattedShort(this TimeSpan timeSpan)
	{
		StringBuilder sb = new();

		if ((int)timeSpan.TotalDays > 0)
		{
			sb.Append((int)timeSpan.TotalDays);
			sb.Append(':');
		}

		if ((int)timeSpan.TotalHours > 0)
		{
			sb.Append(timeSpan.Hours);
			sb.Append(':');
			if (timeSpan.Minutes < 10)
			{
				sb.Append('0');
			}
		}

		if ((int)timeSpan.TotalMinutes > 0)
		{
			sb.Append(timeSpan.Minutes);
			sb.Append(':');
			if (timeSpan.Seconds < 10)
			{
				sb.Append('0');
			}
		}

		sb.Append(timeSpan.Seconds);

		int millis = timeSpan.Milliseconds;
		if (millis > 0)
		{
			sb.Append('.');
			sb.Append(millis.ToString("D3").TrimEnd('0'));
		}

		return sb.ToString();
	}

	public static List<TimeSpan> CommonTimeSpans { get; set; } =
	[
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
	];

	public static TimeSpan PeriodDuration(this TimeSpan timeSpan, int numPeriods = 100)
	{
		TimeSpan maxPeriodDuration = timeSpan.Multiply(2.0 / numPeriods);
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
