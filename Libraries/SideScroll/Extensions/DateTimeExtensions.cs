using SideScroll.Time;

namespace SideScroll.Extensions;

/// <summary>
/// Specifies the precision level for time formatting
/// </summary>
public enum TimeFormatType
{
	/// <summary>
	/// Format to minute precision
	/// </summary>
	Minute,

	/// <summary>
	/// Format to second precision
	/// </summary>
	Second,

	/// <summary>
	/// Format to millisecond precision
	/// </summary>
	Millisecond,

	/// <summary>
	/// Format to microsecond precision
	/// </summary>
	Microsecond,
}

/// <summary>
/// Contains format strings for different time precision levels
/// </summary>
public class TimeFormats
{
	/// <summary>
	/// Default minute format string (short time pattern)
	/// </summary>
	public const string DefaultMinute = "t";

	/// <summary>
	/// Default second format string (long time pattern)
	/// </summary>
	public const string DefaultSecond = "T";

	/// <summary>
	/// Default millisecond format string
	/// </summary>
	public const string DefaultMillisecond = "h:mm:ss.FFF tt"; // todo: 'T' doesn't include milliseconds which we want

	/// <summary>
	/// Default microsecond format string
	/// </summary>
	public const string DefaultMicrosecond = "h:mm:ss.FFFFFF tt";

	/// <summary>
	/// Default UTC minute format string (24-hour format)
	/// </summary>
	public const string DefaultUtcMinute = "H:mm";

	/// <summary>
	/// Default UTC second format string (24-hour format)
	/// </summary>
	public const string DefaultUtcSecond = "H:mm:ss";

	/// <summary>
	/// Default UTC millisecond format string (24-hour format)
	/// </summary>
	public const string DefaultUtcMillisecond = "H:mm:ss.FFF";

	/// <summary>
	/// Default UTC microsecond format string (24-hour format)
	/// </summary>
	public const string DefaultUtcMicrosecond = "H:mm:ss.FFFFFF";

	/// <summary>
	/// Gets or sets the minute format string
	/// </summary>
	public string Minute { get; set; } = DefaultMinute;

	/// <summary>
	/// Gets or sets the second format string
	/// </summary>
	public string Second { get; set; } = DefaultSecond;

	/// <summary>
	/// Gets or sets the millisecond format string
	/// </summary>
	public string Millisecond { get; set; } = DefaultMillisecond;

	/// <summary>
	/// Gets or sets the microsecond format string
	/// </summary>
	public string Microsecond { get; set; } = DefaultMicrosecond;

	/// <summary>
	/// Gets a TimeFormats instance configured with UTC format strings
	/// </summary>
	public static TimeFormats DefaultUtc => new()
	{
		Minute = DefaultUtcMinute,
		Second = DefaultUtcSecond,
		Millisecond = DefaultUtcMillisecond,
		Microsecond = DefaultUtcMicrosecond,
	};
}

/// <summary>
/// Contains standard DateTime format strings
/// </summary>
public static class DateTimeFormats
{
	/// <summary>
	/// Standard date format string (yyyy-M-d)
	/// </summary>
	public const string DateFormat = "yyyy-M-d";

	/// <summary>
	/// Identifier format string with microsecond precision (yyyy-MM-dd H:mm:ss.FFFFFFF)
	/// </summary>
	public const string Id = "yyyy-MM-dd H:mm:ss.FFFFFFF";
}

/// <summary>
/// Extension methods for DateTime and DateTimeOffset manipulation and formatting
/// </summary>
public static class DateTimeExtensions
{
	/// <summary>
	/// Gets or sets the default date format string
	/// </summary>
	public static string DateFormat { get; set; } = DateTimeFormats.DateFormat;

	/// <summary>
	/// Gets or sets the time format strings for local time
	/// </summary>
	public static TimeFormats TimeFormats { get; set; } = new();

	/// <summary>
	/// Gets or sets the time format strings for UTC time
	/// </summary>
	public static TimeFormats TimeFormatsUtc { get; set; } = TimeFormats.DefaultUtc;

	/// <summary>
	/// Gets or sets the DateTime identifier format string
	/// </summary>
	public static string DateTimeFormatId { get; set; } = DateTimeFormats.Id;

	/// <summary>
	/// Gets or sets the default time format type to use
	/// </summary>
	public static TimeFormatType DefaultFormatType { get; set; }

	/// <summary>
	/// Formats a DateTime using the specified format type (Minute, Second, Millisecond, or Microsecond precision)
	/// </summary>
	public static string? Format(this DateTime dateTime, TimeFormatType? formatType = null)
	{
		dateTime = TimeZoneView.Current.Convert(dateTime);
		formatType ??= DefaultFormatType;

		var timeFormats = dateTime.Kind == DateTimeKind.Utc ? TimeFormatsUtc : TimeFormats;

		string? timeFormat = formatType switch
		{
			TimeFormatType.Minute => timeFormats.Minute,
			TimeFormatType.Second => timeFormats.Second,
			TimeFormatType.Millisecond => timeFormats.Millisecond,
			TimeFormatType.Microsecond => timeFormats.Microsecond,
			_ => null,
		};

		if (timeFormat == null) return null;

		return dateTime.ToString(DateFormat) + ' ' + dateTime.ToString(timeFormat);
	}

	/// <summary>
	/// Formats a DateTime as a unique identifier string in UTC with microsecond precision (format: "yyyy-MM-dd H:mm:ss.FFFFFFF")
	/// </summary>
	public static string FormatId(this DateTime dateTime)
	{
		dateTime = TimeZoneView.Utc.ConvertTimeToUtc(dateTime);
		return dateTime.ToString(DateTimeFormatId);
	}

	/// <summary>
	/// Trims a DateTime to the specified tick precision, removing fractional ticks (default: seconds)
	/// </summary>
	public static DateTime Trim(this DateTime dateTime, long ticks = TimeSpan.TicksPerSecond)
	{
		return new DateTime(dateTime.Ticks - (dateTime.Ticks % ticks), dateTime.Kind);
	}

	/// <summary>
	/// Trims a DateTime to the specified TimeSpan precision, removing fractional parts
	/// </summary>
	public static DateTime Trim(this DateTime dateTime, TimeSpan timeSpan)
	{
		return Trim(dateTime, timeSpan.Ticks);
	}

	/// <summary>
	/// Trims a DateTimeOffset to the specified tick precision, removing fractional ticks
	/// </summary>
	public static DateTimeOffset Trim(this DateTimeOffset dateTimeOffset, long ticks)
	{
		DateTime dateTime = dateTimeOffset.UtcDateTime; // DateTime defaults to Unspecified
		return new DateTimeOffset(dateTime.Trim(ticks));
	}

	/// <summary>
	/// Rounds a DateTime up to the next tick interval (default: seconds)
	/// </summary>
	public static DateTime Ceil(this DateTime dateTime, long ticks = TimeSpan.TicksPerSecond)
	{
		return new DateTime(dateTime.Ticks + ticks - 1, dateTime.Kind).Trim();
	}

	/// <summary>
	/// Returns the later of two DateTime values
	/// </summary>
	public static DateTime Max(this DateTime first, DateTime second)
	{
		return new DateTime(Math.Max(first.Ticks, second.Ticks), first.Kind);
	}

	/// <summary>
	/// Returns the earlier of two DateTime values
	/// </summary>
	public static DateTime Min(this DateTime first, DateTime second)
	{
		return new DateTime(Math.Min(first.Ticks, second.Ticks), first.Kind);
	}

	/// <summary>
	/// Calculates the time elapsed since the specified DateTime (trimmed to seconds)
	/// </summary>
	public static TimeSpan Age(this DateTime dateTime)
	{
		return DateTime.UtcNow.Subtract(dateTime.ToUniversalTime()).Trim();
	}
}
