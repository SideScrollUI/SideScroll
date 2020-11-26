using Atlas.Core;
using Atlas.Extensions;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Atlas.UI.Avalonia
{
	public class FormatValueConverter : IValueConverter
	{
		// add a map to store original mappings?
		//public Dictionary<object, object> { get; set; }
		public bool ConvertBackEnabled { get; set; } = true;
		public int MaxLength { get; set; } = 1000;

		private object _originalValue;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			_originalValue = value;
			if (value == null)
				return null;

			object result = ChangeType(value, targetType, MaxLength);
			return result;
		}

		// The DataGrid triggers this even if the binding is one way
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return _originalValue;
		}

		public static object ChangeType(object value, Type targetType, int maxLength)
		{
			if (targetType.IsGenericType && targetType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
			{
				if (value == null)
					return null;

				targetType = Nullable.GetUnderlyingType(targetType);
			}

			if (value is string text)
			{
				if (text.Length == 0)
					return null;
			}

			if (targetType == typeof(string))
			{
				if (value is DateTime dateTime)
					return dateTime.ToUniversalTime().ToString("yyyy-M-d H:mm:ss.FFF");

				if (value is DateTimeOffset dateTimeOffset)
					return dateTimeOffset.UtcDateTime.ToString("yyyy-M-d H:mm:ss.FFF");

				if (value is TimeSpan timeSpan)
				{
					if (timeSpan.TotalSeconds < 1)
						return timeSpan.Trim(TimeSpan.FromMilliseconds(1)).ToString("g");
					else
						return timeSpan.FormattedDecimal();
				}

				//return timeSpan.ToString(@"s\.fff"); // doesn't display minutes or above

				return value.Formatted(maxLength);
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
	}
}