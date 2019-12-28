using Atlas.Core;
using Atlas.Extensions;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Atlas.GUI.Avalonia
{
	public class FormatValueConverter : IValueConverter
	{
		// add a map to store original mappings?
		//public Dictionary<object, object> { get; set; }
		public bool ConvertBackEnabled { get; set; } = true;
		private object originalValue;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			originalValue = value;
			if (value == null)
				return null;

			//if (targetType == typeof(string))
			//	return value.ObjectToString();

			object result = ChangeType(value, targetType);
			//dynamic result = System.Convert.ChangeType(value, targetType);
			return result;
		}

		// The DataGrid triggers this even if the binding is one way
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return originalValue;
			// once a string, keep it as a string for copying to the DataGrid ClipBoard
			//return value;
			/*if (value == null)
				return null;

			object result = ChangeType(value, targetType);

			return result;*/
		}

		public static object ChangeType(object value, Type targetType)
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
					return timeSpan.ToString("g");
				//return timeSpan.ToString(@"s\.fff"); // doesn't display minutes or above

				return value.ObjectToString();
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