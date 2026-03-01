using System.Text;

namespace SideScroll.Extensions;

/// <summary>
/// Extension methods for formatting and manipulating TimeSpan values
/// </summary>
public static class TimeSpanExtensions
{
	/// <summary>
	/// Represents a time unit with its TimeSpan value and display name
	/// </summary>
	public record TimeUnit(TimeSpan TimeSpan, string Name);

	/// <summary>
	/// List of common time units for formatting (Year, Week, Day, Hour, Minute, Second, Millisecond)
	/// </summary>
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

	/// <summary>
	/// Formats a TimeSpan as a decimal number with the largest appropriate time unit (e.g., "3.5 Hours")
	/// </summary>
	public static string FormattedDecimal(this TimeSpan timeSpan)
	{
		string format = "#,0.#";
		var absTimeSpan = new TimeSpan(Math.Abs(timeSpan.Ticks));
		foreach (TimeUnit timeUnit in TimeUnits)
		{
			if (absTimeSpan < timeUnit.TimeSpan)
				continue;

			double units = timeSpan / timeUnit.TimeSpan;
			string value = units.ToString(format) + " " + timeUnit.Name;

			if (absTimeSpan > timeUnit.TimeSpan)
			{
				value += "s";
			}

			return value;
		}
		return timeSpan.TotalSeconds + " Seconds";
	}

	/// <summary>
	/// Formats a TimeSpan in a compact format showing only required units with up to 3 optional decimal places. Format: [d:]h:mm:ss[.fff]
	/// - Shows days only if > 0 (e.g., "2:1:23:45" for 2 days 1 hour)
	/// - Shows hours only if > 0 (e.g., "1:23:45" for 1 hour)
	/// - Shows minutes only if > 0, with zero-padded seconds (e.g., "2:30" for 2 minutes 30 seconds)
	/// - Always shows seconds (e.g., "45" for 45 seconds)
	/// - Shows milliseconds only if > 0, with trailing zeros trimmed (e.g., "1:23:45.12" or "45.5")
	/// </summary>
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

	/// <summary>
	/// List of commonly used TimeSpan durations for UI selection
	/// </summary>
	public static List<TimeSpan> CommonTimeSpans { get; set; } =
	[
		TimeSpan.FromMilliseconds(1),
		TimeSpan.FromMilliseconds(5),
		TimeSpan.FromMilliseconds(10),
		TimeSpan.FromMilliseconds(50),
		TimeSpan.FromMilliseconds(100),
		TimeSpan.FromMilliseconds(500),
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

	/// <summary>
	/// Calculates an appropriate period duration for dividing a TimeSpan into the specified number of periods
	/// </summary>
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

	/// <summary>
	/// Trims a TimeSpan to the specified tick precision, removing fractional ticks (default: seconds)
	/// </summary>
	public static TimeSpan Trim(this TimeSpan timeSpan, long ticks = TimeSpan.TicksPerSecond)
	{
		return new TimeSpan(timeSpan.Ticks - (timeSpan.Ticks % ticks));
	}

	/// <summary>
	/// Trims a TimeSpan to the specified rounding interval precision
	/// </summary>
	public static TimeSpan Trim(this TimeSpan timeSpan, TimeSpan roundingInterval)
	{
		return Trim(timeSpan, roundingInterval.Ticks);
	}

	/// <summary>
	/// Rounds a TimeSpan up to the next tick interval (default: seconds)
	/// </summary>
	public static TimeSpan Ceil(this TimeSpan timeSpan, long ticks = TimeSpan.TicksPerSecond)
	{
		return new TimeSpan(ticks * ((timeSpan.Ticks + ticks - 1) / ticks));
	}

	/// <summary>
	/// Returns the longer of two TimeSpan values
	/// </summary>
	public static TimeSpan Max(this TimeSpan first, TimeSpan second)
	{
		return new TimeSpan(Math.Max(first.Ticks, second.Ticks));
	}

	/// <summary>
	/// Returns the shorter of two TimeSpan values
	/// </summary>
	public static TimeSpan Min(this TimeSpan first, TimeSpan second)
	{
		return new TimeSpan(Math.Min(first.Ticks, second.Ticks));
	}
}
