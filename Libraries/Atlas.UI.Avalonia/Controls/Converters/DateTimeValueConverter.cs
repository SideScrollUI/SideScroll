using Atlas.Core;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Atlas.UI.Avalonia;

public class DateTimeValueConverter : IValueConverter
{
	public DateTime? previousDateTime;

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is TimeSpan timeSpan)
		{
			var date = ((DateTime)previousDateTime).Date;
			previousDateTime = date.AddSeconds(timeSpan.TotalSeconds);
			return previousDateTime;
		}

		previousDateTime = value as DateTime?;

		if (targetType == typeof(string))
		{
			if (previousDateTime == null)
				return "";

			return ((DateTime)previousDateTime).ToString("H:mm:ss");
		}
		else
		{
			return previousDateTime;
		}
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value == null)
			return previousDateTime;

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

		return previousDateTime;
	}

	private void SetDate(DateTime dateTime)
	{
		if (previousDateTime == null)
		{
			previousDateTime = dateTime;
			return;
		}

		// use the same Kind as the original
		dateTime = DateTime.SpecifyKind(dateTime, ((DateTime)previousDateTime).Kind);

		var timeSpan = ((DateTime)previousDateTime).TimeOfDay;
		dateTime = dateTime.Date + timeSpan;
		previousDateTime = dateTime;
	}

	public void SetTime(string timeText)
	{
		// use a single 'h' so a leading zero isn't required
		TimeSpan? timeSpan = DateTimeUtils.ConvertTextToTimeSpan(timeText);
		if (timeSpan != null)
		{
			if (previousDateTime != null)
			{
				var date = ((DateTime)previousDateTime).Date;
				previousDateTime = date.AddSeconds(timeSpan.Value.TotalSeconds);
			}
			else
			{
				var date = DateTime.UtcNow.Date;
				previousDateTime = date.AddSeconds(timeSpan.Value.TotalSeconds);
			}
		}
	}
}
