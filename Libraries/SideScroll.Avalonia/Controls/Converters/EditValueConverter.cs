using Avalonia.Data.Converters;
using SideScroll.Extensions;
using System.Globalization;

namespace SideScroll.Avalonia.Controls.Converters;

/// <summary>Converts a property value to a display string and converts the edited string back to the target type for two-way data binding in editable controls.</summary>
public class EditValueConverter : IValueConverter
{
	/// <summary>Converts a value to a string for display in an editable control.</summary>
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value == null)
			return null;

		object? result = ChangeType(value, targetType);
		return result;
	}

	/// <summary>Converts the edited string back to the target type.</summary>
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value == null)
			return null;

		object? result = ChangeType(value, targetType);
		return result;
	}

	/// <summary>Converts <paramref name="value"/> to <paramref name="targetType"/>, handling <see cref="DateTime"/> and formatted types.</summary>
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
			return dateTime.Format(TimeFormatType.Microsecond);
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
