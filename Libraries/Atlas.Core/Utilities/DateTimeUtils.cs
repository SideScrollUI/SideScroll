using Atlas.Extensions;
using System.Globalization;

namespace Atlas.Core;

public static class DateTimeUtils
{
	public static DateTime EpochTime => new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

	public static TimeSpan? ConvertTextToTimeSpan(string text)
	{
		if (text == null)
			return null;

		if (TimeSpan.TryParseExact(text, @"h\:m\:s\.FFFFFFF", CultureInfo.InvariantCulture, out TimeSpan timeSpan))
			return timeSpan;

		if (TimeSpan.TryParseExact(text, @"h\:m\:s", CultureInfo.InvariantCulture, out timeSpan))
			return timeSpan;

		if (TimeSpan.TryParseExact(text, @"h\:m", CultureInfo.InvariantCulture, out timeSpan))
			return timeSpan;

		return null;
	}

	public static DateTime? ConvertTextToDateTime(string text)
	{
		if (text == null)
			return null;

		DateTime dateTime;

		// convert epoch 1569998557298
		var epochTime = EpochTime;
		if (text.Length == 10 && uint.TryParse(text, out uint epochValue))
		{
			dateTime = epochTime.AddSeconds(epochValue);
			return dateTime;
		}

		if (text.Length == 13 && long.TryParse(text, out long epochValueMilliseconds))
		{
			dateTime = epochTime.AddMilliseconds(epochValueMilliseconds);
			return dateTime;
		}

		if (DateTime.TryParse(text, out dateTime)
			//|| DateTime.TryParseExact(text, "dd/MMM/yyyy:HH:mm:ss zzz", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out dateTime) // July 25 05:08:00
			|| DateTime.TryParseExact(text, "dd/MMM/yyyy:HH:mm:ss zzz", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out dateTime) // 18/Jul/2019:11:47:45 +0000
			|| DateTime.TryParseExact(text, "dd/MMM/yyyy:HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out dateTime)) // 18/Jul/2019:11:47:45
		{
			if (dateTime.Kind == DateTimeKind.Unspecified)
				dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
			else if (dateTime.Kind == DateTimeKind.Local)
				dateTime = dateTime.ToUniversalTime();
			return dateTime;
		}

		return null;
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
			text += " - " + duration.FormattedDecimal();
		return text;
	}
}
