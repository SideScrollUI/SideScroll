using Atlas.Extensions;
using System;
using System.Globalization;

namespace Atlas.Core
{
	public class DateTimeUtils
	{
		public static DateTime EpochTime => new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

		public static TimeSpan? ConvertTextToTimeSpan(string text)
		{
			TimeSpan timeSpan;
			if (TimeSpan.TryParseExact(text, @"h\:m\:s", CultureInfo.InvariantCulture, out timeSpan))
				return timeSpan;

			if (TimeSpan.TryParseExact(text, @"h\:m", CultureInfo.InvariantCulture, out timeSpan))
				return timeSpan;

			return null;
		}

		public static DateTime? ConvertTextToDateTime(string text)
		{
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

		public static string FormatTimeRange(DateTime startTime, DateTime endTime)
		{
			string startFormat = "yyyy-M-d H:mm:ss";
			string endFormat = (startTime.Date == endTime.Date) ? "H:mm:ss" : startFormat;
			TimeSpan duration = endTime.Subtract(startTime);
			return startTime.ToString(startFormat) + " - " + endTime.ToString(endFormat) + " - " + duration.FormattedDecimal();
		}
	}
}
