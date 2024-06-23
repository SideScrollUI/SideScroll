using Avalonia.Data;
using Avalonia.Data.Converters;
using SideScroll.Utilities;
using System.Globalization;

namespace SideScroll.UI.Avalonia.Controls.Converters;

public class DateTimeValueConverter : IValueConverter
{
	public string? PreviousTimeText;
	public DateTime? PreviousDateTime;

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		// Debug.WriteLine($"Convert {value} of Type {value?.GetType()}, Target: {targetType}");

		PreviousDateTime = value as DateTime?;

		if (targetType == typeof(string))
		{
			if (value is DateTime dateTime)
			{
				if (PreviousTimeText != null &&
					DateTimeUtils.TryParseTimeSpan(PreviousTimeText, out TimeSpan prevTimeSpan)
					&& dateTime.TimeOfDay == prevTimeSpan)
				{
					// Debug.WriteLine($"Convert returned PreviousTimeText: {PreviousTimeText}");
					return PreviousTimeText;
				}
			}
			return PreviousDateTime?.ToString("H:mm:ss") ?? "";
		}
		else if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
		{
			return PreviousDateTime;
		}
		else
		{
			return null;
		}
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		// Debug.WriteLine($"ConvertBack {value} of Type {value?.GetType()}, Target: {targetType}");

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

		if (PreviousDateTime is DateTime prevDateTime)
		{
			// use the same Kind as the original
			dateTime = DateTime.SpecifyKind(dateTime, prevDateTime.Kind);

			var timeSpan = prevDateTime.TimeOfDay;
			dateTime = dateTime.Date + timeSpan;
			PreviousDateTime = dateTime;
		}
		else
		{
			PreviousDateTime = dateTime;
		}
	}

	public object? SetTime(string timeText)
	{
		// Debug.WriteLine($"SetTime {timeText}");

		PreviousTimeText = timeText;

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
