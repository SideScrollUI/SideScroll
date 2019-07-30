using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Atlas.GUI.Avalonia
{
	public class DateTimeValueConverter : IValueConverter
	{
		public DateTime? originalDateTime;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			originalDateTime = value as DateTime?;

			if (targetType == typeof(string))
			{
				if (originalDateTime == null)
					return "";

				return ((DateTime)originalDateTime).ToString("HH:mm:ss");
			}
			else
			{
				return originalDateTime;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return originalDateTime;

			if (targetType != typeof(DateTime) && targetType != typeof(DateTime?))
				throw new Exception("invalid conversion");

			if (value is string text)
			{
				SetTime(text.Trim());
			}
			else if (value is DateTime dateTime)
			{
				SetDate(dateTime);
			}

			return originalDateTime;
		}

		private void SetDate(DateTime dateTime)
		{
			if (originalDateTime == null)
			{
				originalDateTime = dateTime;
				return;
			}

			// use the same Kind as the original
			dateTime = DateTime.SpecifyKind(dateTime, ((DateTime)originalDateTime).Kind);

			var timeSpan = ((DateTime)originalDateTime).TimeOfDay;
			dateTime = dateTime.Date + timeSpan;
			originalDateTime = dateTime;
		}

		public void SetTime(string timeText)
		{
			TimeSpan timeSpan;
			if (TimeSpan.TryParseExact(timeText, @"hh\:mm\:ss", CultureInfo.InvariantCulture, out timeSpan))
			{
				var date = ((DateTime)originalDateTime).Date;
				originalDateTime = date.AddSeconds(timeSpan.TotalSeconds);
				//return timeSpan;
			}

			//DateTimeOffset result;
			//if (DateTimeOffset.TryParseExact(text, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out result))
			//DateTime result;
			//if (DateTime.TryParseExact(text, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.NoCurrentDateDefault, out result))
		}
	}
}