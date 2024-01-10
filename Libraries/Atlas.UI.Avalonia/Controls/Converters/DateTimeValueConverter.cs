using Atlas.Core;
using Avalonia.Data;
using Avalonia.Data.Converters;
using System.Globalization;

namespace Atlas.UI.Avalonia;

public class DateTimeValueConverter : IValueConverter
{
	public DateTime? PreviousDateTime;

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		// Debug.WriteLine($"Convert {value}, Target: {targetType}");

		if (value is TimeSpan timeSpan)
		{
			var date = ((DateTime)PreviousDateTime!).Date;
			PreviousDateTime = date.AddSeconds(timeSpan.TotalSeconds);
			return PreviousDateTime;
		}

		PreviousDateTime = value as DateTime?;

		if (targetType == typeof(string))
		{
			if (PreviousDateTime is DateTime dateTime)
			{
				return dateTime.ToString("H:mm:ss");
			}

			return "";
		}
		else
		{
			return PreviousDateTime;
		}
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		// Debug.WriteLine($"ConvertBack {value}, Target: {targetType}");

		if (value == null) return PreviousDateTime;

		if (targetType != typeof(DateTime) && targetType != typeof(DateTime?))
		{
			return new BindingNotification(new DataValidationException("TargetType must be DateTime"), BindingErrorType.DataValidationError);
		}

		if (value is DateTime dateTime)
		{
			SetDate(dateTime);
		}
		else if (value is string text)
		{
			return SetTime(text);
		}

		return PreviousDateTime;
	}

	private void SetDate(DateTime dateTime)
	{
		// Debug.WriteLine($"SetDate {dateTime}");

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

	public object? SetTime(string timeText)
	{
		// Debug.WriteLine($"SetTime {timeText}");

		if (!DateTimeUtils.TryParseTimeSpan(timeText, out TimeSpan timeSpan))
		{
			// This doesn't always clear correctly after fixing a validation if the DateTime matches the previous valid value
			// So don't show an error message for now to make it a little less confusing
			return new BindingNotification(new DataValidationException(""), BindingErrorType.DataValidationError);
		}

		if (PreviousDateTime is not DateTime dateTime)
		{
			dateTime = DateTime.UtcNow.Date;
		}

		PreviousDateTime = dateTime.Date.AddSeconds(timeSpan.TotalSeconds);
		return PreviousDateTime;
	}
}
