using Atlas.Core;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Atlas.UI.Avalonia;

public class DateTimeValueConverter : IValueConverter
{
	public DateTime? PreviousDateTime;

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is TimeSpan timeSpan)
		{
			var date = ((DateTime)PreviousDateTime).Date;
			PreviousDateTime = date.AddSeconds(timeSpan.TotalSeconds);
			return PreviousDateTime;
		}

		PreviousDateTime = value as DateTime?;

		if (targetType == typeof(string))
		{
			if (PreviousDateTime == null)
				return "";

			return ((DateTime)PreviousDateTime).ToString("H:mm:ss");
		}
		else
		{
			return PreviousDateTime;
		}
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value == null)
			return PreviousDateTime;

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

		return PreviousDateTime;
	}

	private void SetDate(DateTime dateTime)
	{
		if (PreviousDateTime == null)
		{
			PreviousDateTime = dateTime;
			return;
		}

		// use the same Kind as the original
		dateTime = DateTime.SpecifyKind(dateTime, ((DateTime)PreviousDateTime).Kind);

		var timeSpan = ((DateTime)PreviousDateTime).TimeOfDay;
		dateTime = dateTime.Date + timeSpan;
		PreviousDateTime = dateTime;
	}

	public void SetTime(string timeText)
	{
		// use a single 'h' so a leading zero isn't required
		TimeSpan? timeSpan = DateTimeUtils.ConvertTextToTimeSpan(timeText);
		if (timeSpan != null)
		{
			if (PreviousDateTime != null)
			{
				var date = ((DateTime)PreviousDateTime).Date;
				PreviousDateTime = date.AddSeconds(timeSpan.Value.TotalSeconds);
			}
			else
			{
				var date = DateTime.UtcNow.Date;
				PreviousDateTime = date.AddSeconds(timeSpan.Value.TotalSeconds);
			}
		}
	}
}
