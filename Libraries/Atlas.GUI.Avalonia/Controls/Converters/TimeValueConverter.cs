using Atlas.Extensions;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Atlas.GUI.Avalonia
{
	public class TimeValueConverter : IValueConverter
	{
		public string Append { get; set; }
		private DateTime? dateTime;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return null;

			dateTime = value as DateTime?;
			if (dateTime == null)
				return "";

			return ((DateTime)dateTime).ToString("HH:mm:ss");
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return null;

			string text = value as string;
			if (text == null)
				return null;

			if (dateTime == null)
				return null;
			
			TimeSpan result;
			if (TimeSpan.TryParseExact(text, "g", CultureInfo.InvariantCulture, out result))
			{
				var date = (DateTime)dateTime;
				date.AddSeconds(result.TotalSeconds);
				return result;
			}

			//DateTimeOffset result;
			//if (DateTimeOffset.TryParseExact(text, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out result))
			//DateTime result;
			//if (DateTime.TryParseExact(text, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.NoCurrentDateDefault, out result))

			return dateTime;
		}
	}
}