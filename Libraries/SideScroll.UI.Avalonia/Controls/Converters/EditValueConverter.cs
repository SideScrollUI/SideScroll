using Avalonia.Data.Converters;
using SideScroll.Extensions;
using System.Globalization;

namespace SideScroll.UI.Avalonia.Controls.Converters;

public class EditValueConverter : IValueConverter
{
	// public string Append { get; set; }

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value == null)
			return null;

		object? result = ChangeType(value, targetType);
		return result;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value == null)
			return null;

		object? result = ChangeType(value, targetType);
		return result;
	}

	public static object? ChangeType(object? value, Type targetType)
	{
		if (value == null)
			return null;

		if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
		{
			targetType = Nullable.GetUnderlyingType(targetType)!;
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
			return dateTime.ToString("yyyy-M-d H:mm:ss.FFFFFF");
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
