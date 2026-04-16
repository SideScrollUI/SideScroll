using Avalonia.Data;
using Avalonia.Data.Converters;
using SideScroll.Time;
using SideScroll.Utilities;
using System.Globalization;

namespace SideScroll.Avalonia.Controls.Converters;

/// <summary>Converts between a <see cref="DateTime"/> or <see cref="TimeSpan"/> property value and a time-of-day string, with round-trip fidelity for <see cref="TabDateTimePicker"/>.</summary>
public class DateTimeValueConverter : IValueConverter
{
	protected string? PreviousTimeText { get; set; }
	protected DateTime? PreviousDateTime { get; set; }

	/// <summary>Converts a <see cref="DateTime"/> to a formatted time-of-day string for display in a date-time picker.</summary>
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

	/// <summary>Converts the edited time string back to a <see cref="DateTime"/>, merging with the stored date component.</summary>
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

	/// <summary>Parses the given time text and merges it with the stored date, returning a validation error string if the format is invalid.</summary>
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
			dateTime = TimeZoneView.Now.Date;
		}

		PreviousDateTime = dateTime.Date.Add(timeSpan);
		return PreviousDateTime;
	}
}
