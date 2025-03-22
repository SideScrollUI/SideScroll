using Avalonia.Data.Converters;
using SideScroll.Attributes;
using SideScroll.Extensions;
using SideScroll.Tabs.Lists;
using System.Globalization;
using System.Reflection;

namespace SideScroll.Avalonia.Controls.Converters;


public class PropertyTextConverter<T> : FormatValueConverter
{
	public PropertyInfo PropertyInfo { get; init; }

	public FormatValueConverter FormatConverter { get; set; } = new();

	public PropertyTextConverter(PropertyInfo propertyInfo)
	{
		PropertyInfo = propertyInfo;

		var maxHeightAttribute = propertyInfo.GetCustomAttribute<MaxHeightAttribute>();
		if (maxHeightAttribute != null && typeof(IListItem).IsAssignableFrom(PropertyInfo.PropertyType))
		{
			int MaxDesiredHeight = maxHeightAttribute.MaxHeight;
			FormatConverter.MaxLength = MaxDesiredHeight * 10;
		}
		FormatConverter.IsFormatted = (propertyInfo.GetCustomAttribute<FormattedAttribute>() != null);

		var formatterAttribute = propertyInfo.GetCustomAttribute<FormatterAttribute>();
		if (formatterAttribute != null)
		{
			FormatConverter.Formatter = (ICustomFormatter)Activator.CreateInstance(formatterAttribute.Type)!;
		}
	}

	public string? GetText<TModel>(TModel model) where TModel : class
	{
		object? value = PropertyInfo.GetValue(model);
		return (string)Convert(value, typeof(string), null, CultureInfo.CurrentCulture)!;
	}

	public T? GetValue<TModel>(TModel model) where TModel : class
	{
		object? value = PropertyInfo.GetValue(model);
		return (T)Convert(value, typeof(T), null, CultureInfo.CurrentCulture)!;
	}
}


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
