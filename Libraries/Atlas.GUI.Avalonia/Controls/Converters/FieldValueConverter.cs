using Atlas.Extensions;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Atlas.GUI.Avalonia
{
	public class FieldValueConverter : IValueConverter
	{
		public string Append { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return null;

			//if (targetType == typeof(string))
			//	return value.ObjectToString();

			object result = ChangeType(value, targetType);
			//dynamic result = System.Convert.ChangeType(value, targetType);
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
			if (value is string)
			{
				string text = (string)value;
				if (text.Length == 0)
					return null;
			}

			if (value is DateTime)
			{
				DateTime dateTime = (DateTime)value;
				if (dateTime != null)
				{
					string text = dateTime.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.FFFFFF");
					return text;
				}
			}

			if (targetType == typeof(string))
				return value.ObjectToString();
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
}