using Avalonia.Data.Converters;
using SideScroll.Extensions;
using System.Globalization;

namespace SideScroll.Avalonia.Controls.Converters;

public class FormatValueConverter : IValueConverter
{
	public int MaxLength { get; set; } = 1000;

	// add a map to store original mappings?
	//public Dictionary<object, object> { get; set; }

	// public bool ConvertBackEnabled { get; set; } = true;
	public bool IsFormatted { get; set; }

	public ICustomFormatter? Formatter { get; set; }

	private object? _originalValue;

	private int _minDecimals;

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		_originalValue = value;
		if (value == null)
			return null;

		object? result = ChangeType(value, targetType, MaxLength, IsFormatted);
		return result;
	}

	// The DataGrid triggers this even if the binding is one way
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		return _originalValue;
	}

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

	public string? ObjectToString(object value, int maxLength, bool formatted)
	{
		if (value is DateTime dateTime)
		{
			return dateTime.Format();
		}

		if (value is DateTimeOffset dateTimeOffset)
		{
			return dateTimeOffset.UtcDateTime.Format();
		}

		if (value is TimeSpan timeSpan)
		{
			if (formatted)
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
			if (formatted)
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
