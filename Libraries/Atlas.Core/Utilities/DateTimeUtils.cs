using System;
using System.Globalization;

namespace Atlas.Core
{
	public class DateTimeUtils
	{
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
			var epochTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			uint epochValue;
			if (text.Length == 10 && uint.TryParse(text, out epochValue))
			{
				dateTime = epochTime.AddSeconds(epochValue);
				return dateTime;
			}
			long epochValueMilliseconds;
			if (text.Length == 13 && long.TryParse(text, out epochValueMilliseconds))
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
	}
}
