using SideScroll.Extensions;
using SideScroll.Time;
using System.Globalization;

namespace SideScroll.Utilities;

/// <summary>
/// Provides utilities for working with DateTime and TimeSpan values
/// </summary>
public static class DateTimeUtils
{
	/// <summary>
	/// Gets the Unix epoch time (January 1, 1970, 00:00:00 UTC)
	/// </summary>
	public static DateTime EpochTime { get; private set; } = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

	/// <summary>
	/// Attempts to parse a string into a TimeSpan value
	/// </summary>
	/// <param name="text">The text to parse</param>
	/// <param name="timeSpan">The resulting TimeSpan if parsing succeeds</param>
	/// <returns>True if parsing succeeded; otherwise, false</returns>
	/// <remarks>
	/// Supports the following formats:
	/// <list type="bullet">
	/// <item><description>h:m:s.FFFFFFF - Hours, minutes, seconds with fractional seconds (e.g., "1:30:45.1234567")</description></item>
	/// <item><description>h:m:s - Hours, minutes, seconds (e.g., "1:30:45")</description></item>
	/// <item><description>h:m - Hours and minutes (e.g., "1:30")</description></item>
	/// </list>
	/// </remarks>
	public static bool TryParseTimeSpan(string? text, out TimeSpan timeSpan)
	{
		timeSpan = default;

		if (text == null) return false;

		text = text.Trim();

		if (TimeSpan.TryParseExact(text, @"h\:m\:s\.FFFFFFF", CultureInfo.InvariantCulture, out timeSpan))
		{
			return true;
		}

		if (TimeSpan.TryParseExact(text, @"h\:m\:s", CultureInfo.InvariantCulture, out timeSpan))
		{
			return true;
		}

		if (TimeSpan.TryParseExact(text, @"h\:m", CultureInfo.InvariantCulture, out timeSpan))
		{
			return true;
		}

		return false;
	}

	/// <summary>
	/// Attempts to parse a string into a DateTime value, supporting various formats including Unix epoch timestamps
	/// </summary>
	/// <param name="text">The text to parse</param>
	/// <param name="dateTime">The resulting DateTime if parsing succeeds (always returned in UTC)</param>
	/// <returns>True if parsing succeeded; otherwise, false</returns>
	/// <remarks>
	/// Supports the following formats:
	/// <list type="bullet">
	/// <item><description>Unix epoch (10 digits) - Seconds since January 1, 1970 (e.g., "1569998557")</description></item>
	/// <item><description>Unix epoch milliseconds (13 digits) - Milliseconds since January 1, 1970 (e.g., "1569998557298")</description></item>
	/// <item><description>Standard DateTime formats - Any format recognized by DateTime.TryParse</description></item>
	/// <item><description>dd/MMM/yyyy:HH:mm:ss zzz - Apache log format with timezone (e.g., "18/Jul/2019:11:47:45 +0000")</description></item>
	/// <item><description>dd/MMM/yyyy:HH:mm:ss - Apache log format without timezone (e.g., "18/Jul/2019:11:47:45")</description></item>
	/// </list>
	/// All results are converted to UTC.
	/// </remarks>
	public static bool TryParseDateTime(string? text, out DateTime dateTime)
	{
		dateTime = default;

		if (text == null) return false;

		text = text.Trim();

		// Convert epoch 1569998557298
		string numString = text.Replace(",", "");
		if (numString.Length == 10 && uint.TryParse(numString, out uint epochValue))
		{
			dateTime = EpochTime.AddSeconds(epochValue);
			return true;
		}

		if (numString.Length == 13 && long.TryParse(numString, out long epochValueMilliseconds))
		{
			dateTime = EpochTime.AddMilliseconds(epochValueMilliseconds);
			return true;
		}

		if (DateTime.TryParse(text, out dateTime)
			//|| DateTime.TryParseExact(text, "dd/MMM/yyyy:HH:mm:ss zzz", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out dateTime) // July 25 05:08:00
			|| DateTime.TryParseExact(text, "dd/MMM/yyyy:HH:mm:ss zzz", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out dateTime) // 18/Jul/2019:11:47:45 +0000
			|| DateTime.TryParseExact(text, "dd/MMM/yyyy:HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out dateTime)) // 18/Jul/2019:11:47:45
		{
			if (dateTime.Kind == DateTimeKind.Unspecified)
			{
				dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
			}
			else if (dateTime.Kind == DateTimeKind.Local)
			{
				dateTime = dateTime.ToUniversalTime();
			}
			return true;
		}

		return false;
	}

	/// <summary>
	/// Formats a time range into a human-readable string representation
	/// </summary>
	/// <param name="startTime">The start time of the range</param>
	/// <param name="endTime">The end time of the range</param>
	/// <param name="withDuration">Whether to include the duration in the output (default is true)</param>
	/// <returns>A formatted string representing the time range</returns>
	/// <remarks>
	/// The format adjusts based on whether the dates span multiple days and whether seconds are present.
	/// Example outputs:
	/// <list type="bullet">
	/// <item><description>Same day with seconds: "2023-10-18 14:30:45 - 15:45:30 - 1h 14m 45s"</description></item>
	/// <item><description>Same day without seconds: "2023-10-18 2:30 PM - 3:45 PM - 1h 15m"</description></item>
	/// <item><description>Different days: "2023-10-18 14:30:45 - 2023-10-19 15:45:30 - 1d 1h 14m 45s"</description></item>
	/// </list>
	/// Times are displayed in the current time zone.
	/// </remarks>
	public static string FormatTimeRange(DateTime startTime, DateTime endTime, bool withDuration = true)
	{
		startTime = TimeZoneView.Current.Convert(startTime);
		endTime = TimeZoneView.Current.Convert(endTime);

		string dateFormat = "yyyy-M-d";
		string timeFormat = "T";
		if (startTime.Second == 0 && endTime.Second == 0)
		{
			timeFormat = "t";
		}

		string text = startTime.ToString(dateFormat) + ' ' + startTime.ToString(timeFormat) + " -";
		if (startTime.Date != endTime.Date)
		{
			text += ' ' + endTime.ToString(dateFormat);
		}
		text += ' ' + endTime.ToString(timeFormat);

		if (withDuration)
		{
			TimeSpan duration = endTime.Subtract(startTime);
			text += " - " + duration.FormattedDecimal();
		}
		return text;
	}
}
