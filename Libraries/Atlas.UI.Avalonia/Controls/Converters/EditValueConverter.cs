using Atlas.Core;
using Atlas.Extensions;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Atlas.UI.Avalonia;

public class EditValueConverter : IValueConverter
{
	public string Append { get; set; }

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value == null)
			return null;

		object result = ChangeType(value, targetType);
		return result;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value == null)
			return null;

		object result = ChangeType(value, targetType);
		return result;
	}

	public static object ChangeType(object value, Type targetType)
	{
		if (targetType.IsGenericType && targetType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
		{
			if (value == null)
				return null;

			targetType = Nullable.GetUnderlyingType(targetType);
		}

		if (value.GetType() == targetType)
			return value;

		if (value is string text)
		{
			if (text.Length == 0)
				return null;
		}

		if (value is DateTime dateTime)
		{
			return dateTime.ToString("yyyy-M-d H:mm:ss.ffffff");
		}

		if (value.GetType().IsPrimitive == false && targetType == typeof(string))
			return value.Formatted();

		try
		{
			return System.Convert.ChangeType(value, targetType);
		}
		catch
		{
			return null;
		}
	}
}
