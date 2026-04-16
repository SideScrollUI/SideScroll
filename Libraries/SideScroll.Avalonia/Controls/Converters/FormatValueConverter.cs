using Avalonia.Data.Converters;
using SideScroll.Extensions;
using System.Globalization;

namespace SideScroll.Avalonia.Controls.Converters;

/// <summary>Converts a value to a truncated, optionally formatted display string for read-only data-grid cells, with optional custom formatter support.</summary>
public class FormatValueConverter : IValueConverter
{
	/// <summary>Gets or sets the maximum number of characters shown before the value is truncated. Defaults to 1000.</summary>
	public int MaxLength { get; set; } = 1000;

	/// <summary>Gets or sets whether the value is passed through a JSON or other pretty-print formatter before display.</summary>
	public bool IsFormatted { get; set; }

	/// <summary>Gets or sets an optional custom formatter used to convert the value to a string.</summary>
	public ICustomFormatter? Formatter { get; set; }

	private object? _originalValue;

	private int _minDecimals;

	/// <summary>Converts a value to a truncated display string for a read-only data-grid cell.</summary>
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		_originalValue = value;
		if (value == null)
			return null;

		object? result = ChangeType(value, targetType, MaxLength, IsFormatted);
		return result;
	}

	// The DataGrid triggers this even if the binding is one way
	/// <summary>Returns the value unchanged; ConvertBack is not meaningful for this read-only display converter.</summary>
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		return _originalValue;
	}

	/// <summary>Converts <paramref name="value"/> to the target type, delegating to <see cref="ObjectToString"/> for string targets.</summary>
	public object? ChangeType(object? value, Type targetType, int maxLength, bool formatted)
	{
		if (value is null or DBNull)
			return null;

		if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
		{
			targetType = Nullable.GetUnderlyingType(targetType)!;
		}

		if (value is string text)
		{
			if (text.Length == 0)
				return null;
		}

		if (targetType == typeof(string))
		{
			if (Formatter != null)
				return Formatter.Format(null, value, null);

			return ObjectToString(value, maxLength, formatted);
		}

		try
		{
			return System.Convert.ChangeType(value, targetType);
		}
		catch
		{
			return null;
		}
	}

	/// <summary>Formats <paramref name="value"/> as a display string, applying type-specific formatting for <see cref="DateTime"/>, <see cref="TimeSpan"/>, and <see cref="double"/>.</summary>
	public string? ObjectToString(object? value, int maxLength, bool? formatted = null)
	{
		if (value is null) return null;

		if (value is DateTime dateTime)
		{
			return dateTime.Format();
		}

		if (value is DateTimeOffset dateTimeOffset)
		{
			return dateTimeOffset.UtcDateTime.Format();
		}

		formatted ??= IsFormatted;

		if (value is TimeSpan timeSpan)
		{
			if (formatted == true)
			{
				return timeSpan.FormattedDecimal();
			}
			else
			{
				return timeSpan.Trim(TimeSpan.FromMilliseconds(1)).FormattedShort();
			}
		}

		if (value is double d)
		{
			if (formatted == true)
			{
				return d.FormattedDecimal();
			}
			else
			{
				// Show the maximum decimal places found
				int optional = Math.Max(0, (Math.Abs(d) < 0.001 ? 6 : 3) - _minDecimals);

				var text = d.ToString("#,0." + new string('0', _minDecimals) + new string('#', optional));
				int periodIndex = text.IndexOf('.');
				if (periodIndex >= 0)
				{
					_minDecimals = Math.Max(_minDecimals, text.Length - periodIndex - 1);
				}
				return text;
			}
		}

		return value.Formatted(maxLength);
	}
}
