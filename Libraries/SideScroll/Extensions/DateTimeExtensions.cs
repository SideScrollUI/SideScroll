using SideScroll.Time;

namespace SideScroll.Extensions;

public enum TimeFormatType
{
	Minute,
	Second,
	Millisecond,
	Microsecond,
}

public class TimeFormats
{
	public const string DefaultMinute = "t";
	public const string DefaultSecond = "T";
	public const string DefaultMillisecond = "h:mm:ss.FFF tt"; // todo: 'T' doesn't include milliseconds which we want
	public const string DefaultMicrosecond = "h:mm:ss.FFFFFF tt";

	public const string DefaultUtcMinute = "H:mm";
	public const string DefaultUtcSecond = "H:mm:ss";
	public const string DefaultUtcMillisecond = "H:mm:ss.FFF";
	public const string DefaultUtcMicrosecond = "H:mm:ss.FFFFFF";

	public string Minute { get; set; } = DefaultMinute;
	public string Second { get; set; } = DefaultSecond;
	public string Millisecond { get; set; } = DefaultMillisecond;
	public string Microsecond { get; set; } = DefaultMicrosecond;

	public static TimeFormats DefaultUtc => new()
	{
		Minute = DefaultUtcMinute,
		Second = DefaultUtcSecond,
		Millisecond = DefaultUtcMillisecond,
		Microsecond = DefaultUtcMicrosecond,
	};
}

public static class DateTimeFormats
{
	public const string DateFormat = "yyyy-M-d";

	public const string Id = "yyyy-MM-dd H:mm:ss.FFFFFFFF";
}

public static class DateTimeExtensions
{
	public static string DateFormat { get; set; } = DateTimeFormats.DateFormat;

	public static TimeFormats TimeFormats { get; set; } = new();
	public static TimeFormats TimeFormatsUtc { get; set; } = TimeFormats.DefaultUtc;

	public static string DateTimeFormatId { get; set; } = DateTimeFormats.Id;

	public static TimeFormatType DefaultFormatType { get; set; }

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

	public static string? FormatId(this DateTime dateTime)
	{
		dateTime = TimeZoneView.Utc.ConvertTimeToUtc(dateTime);
		return dateTime.ToString(DateTimeFormatId);
	}

	public static DateTime Trim(this DateTime dateTime, long ticks = TimeSpan.TicksPerSecond)
	{
		return new DateTime(dateTime.Ticks - (dateTime.Ticks % ticks), dateTime.Kind);
	}

	public static DateTime Trim(this DateTime dateTime, TimeSpan timeSpan)
	{
		return Trim(dateTime, timeSpan.Ticks);
	}

	public static DateTimeOffset Trim(this DateTimeOffset dateTimeOffset, long ticks)
	{
		DateTime dateTime = dateTimeOffset.DateTime;
		return new DateTimeOffset(dateTime.Trim(ticks));
	}

	public static DateTime Ceil(this DateTime dateTime, long ticks = TimeSpan.TicksPerSecond)
	{
		return new DateTime(dateTime.Ticks + ticks - 1, dateTime.Kind).Trim();
	}

	public static DateTime Max(this DateTime first, DateTime second)
	{
		return new DateTime(Math.Max(first.Ticks, second.Ticks), first.Kind);
	}

	public static DateTime Min(this DateTime first, DateTime second)
	{
		return new DateTime(Math.Min(first.Ticks, second.Ticks), first.Kind);
	}

	public static TimeSpan Age(this DateTime dateTime)
	{
		return DateTime.UtcNow.Subtract(dateTime.ToUniversalTime()).Trim();
	}
}
