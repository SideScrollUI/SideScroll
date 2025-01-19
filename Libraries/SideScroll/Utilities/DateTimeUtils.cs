using SideScroll.Extensions;
using SideScroll.Time;
using System.Globalization;

namespace SideScroll.Utilities;

public static class DateTimeUtils
{
	public static DateTime EpochTime { get; private set; } = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

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
