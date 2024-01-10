using Atlas.Extensions;
using System.Globalization;

namespace Atlas.Core;

public static class DateTimeUtils
{
	public static DateTime EpochTime => new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

	public static bool TryParseTimeSpan(string text, out TimeSpan timeSpan)
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

	public static bool TryParseDateTime(string text, out DateTime dateTime)
	{
		dateTime = default;

		if (text == null) return false;

		text = text.Trim();

		// Convert epoch 1569998557298
		if (text.Length == 10 && uint.TryParse(text, out uint epochValue))
		{
			dateTime = EpochTime.AddSeconds(epochValue);
			return true;
		}

		if (text.Length == 13 && long.TryParse(text, out long epochValueMilliseconds))
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

	public static string FormatTimeRange(DateTime startTime, DateTime endTime, bool withDuration = true)
	{
		string timeFormat = "H:mm:ss";
		if (startTime.Second == 0 && endTime.Second == 0)
		{
			timeFormat = "H:mm";
		}
		string startFormat = $"yyyy-M-d {timeFormat}";
		string endFormat = (startTime.Date == endTime.Date) ? timeFormat : startFormat;

		TimeSpan duration = endTime.Subtract(startTime);

		string text = startTime.ToString(startFormat) + " - " + endTime.ToString(endFormat);
		if (withDuration)
		{
			text += " - " + duration.FormattedDecimal();
		}
		return text;
	}
}
