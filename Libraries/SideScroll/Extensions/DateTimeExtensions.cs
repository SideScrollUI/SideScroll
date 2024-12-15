using SideScroll.Time;

namespace SideScroll.Extensions;

public enum TimeFormatType
{
	Minute,
	Second,
	Millisecond,
	Microsecond,
}

public static class DateTimeFormats
{
	public const string DateFormat = "yyyy-M-d";

	public const string TimeMinute = "t";
	public const string TimeSecond = "T"; // todo: 'T' doesn't include milliseconds which we want
	public const string TimeMillisecond = "h:mm:ss.FFF tt"; // todo: 'T' doesn't include milliseconds which we want
	public const string TimeMicrosecond = "h:mm:ss.FFFFFF tt";

	public const string TimeUtcMinute = "H:mm";
	public const string TimeUtcSecond = "H:mm:ss";
	public const string TimeUtcMillisecond = "H:mm:ss.FFF";
	public const string TimeUtcMicrosecond = "H:mm:ss.FFFFFF";

	public const string Id = "yyyy-MM-dd H:mm:ss.FFFFFFF";
}

public static class DateTimeExtensions
{
	public static string DateFormat { get; set; } = DateTimeFormats.DateFormat;

	public static string TimeFormatMinute { get; set; } = DateTimeFormats.TimeMinute;
	public static string TimeFormatSecond { get; set; } = DateTimeFormats.TimeSecond;
	public static string TimeFormatMillisecond { get; set; } = DateTimeFormats.TimeMillisecond;
	public static string TimeFormatMicrosecond { get; set; } = DateTimeFormats.TimeMicrosecond;

	public static string TimeFormatUtcMinute { get; set; } = DateTimeFormats.TimeUtcMinute;
	public static string TimeFormatUtcSecond { get; set; } = DateTimeFormats.TimeUtcSecond;
	public static string TimeFormatUtcMillisecond { get; set; } = DateTimeFormats.TimeUtcMillisecond;
	public static string TimeFormatUtcMicrosecond { get; set; } = DateTimeFormats.TimeUtcMicrosecond;

	public static string DateTimeFormatId { get; set; } = DateTimeFormats.Id;

	public static TimeFormatType DefaultFormatType { get; set; }

	public static string? Format(this DateTime dateTime, TimeFormatType? formatType = null)
	{
		dateTime = TimeZoneView.Current.Convert(dateTime);
		formatType ??= DefaultFormatType;

		string? timeFormat = formatType switch
		{
			TimeFormatType.Minute => dateTime.Kind == DateTimeKind.Utc ? TimeFormatUtcMinute : TimeFormatMinute,
			TimeFormatType.Second => dateTime.Kind == DateTimeKind.Utc ? TimeFormatUtcSecond : TimeFormatSecond,
			TimeFormatType.Millisecond => dateTime.Kind == DateTimeKind.Utc ? TimeFormatUtcMillisecond : TimeFormatMillisecond,
			TimeFormatType.Microsecond => dateTime.Kind == DateTimeKind.Utc ? TimeFormatUtcMicrosecond : TimeFormatMicrosecond,
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
