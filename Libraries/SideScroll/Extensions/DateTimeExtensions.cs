using SideScroll.Time;

namespace SideScroll.Extensions;

public static class DateTimeExtensions
{
	public const string StringFormatUtcShort = "yyyy-M-d H:mm:ss.FFF";
	public const string StringFormatUtcLong = "yyyy-M-d H:mm:ss.FFFFFF";
	public const string StringFormatUtcId = "yyyy-MM-dd H:mm:ss.FFFFFF";

	public const string StringFormatShort = "yyyy-M-d h:mm:ss.FFF tt";
	public const string StringFormatLong = "yyyy-M-d h:mm:ss.FFFFFF tt";

	public static string? FormatShort(this DateTime dateTime)
	{
		dateTime = TimeZoneView.Current.Convert(dateTime);
		string format = dateTime.Kind == DateTimeKind.Utc ? StringFormatUtcShort : StringFormatShort;
		return dateTime.ToString(format);
	}

	public static string FormatLong(this DateTime dateTime)
	{
		dateTime = TimeZoneView.Current.Convert(dateTime);
		string format = dateTime.Kind == DateTimeKind.Utc ? StringFormatUtcLong : StringFormatLong;
		return dateTime.ToString(format);
	}

	public static string? FormatId(this DateTime dateTime)
	{
		dateTime = TimeZoneView.Current.Convert(dateTime);
		return dateTime.ToString(StringFormatUtcId);
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
		DateTime dateTime = dateTimeOffset.UtcDateTime;
		return new DateTimeOffset(dateTime.Trim(ticks));
	}

	public static DateTime Ceil(this DateTime dateTime, long ticks = TimeSpan.TicksPerSecond)
	{
		return new DateTime(dateTime.Ticks + ticks - 1, dateTime.Kind).Trim();
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
		return DateTime.UtcNow.Subtract(dateTime.ToUniversalTime()).Trim();
	}
}
