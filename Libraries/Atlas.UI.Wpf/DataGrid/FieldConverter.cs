using Atlas.Extensions;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Atlas.UI.Wpf
{
	[ValueConversion(typeof(string), typeof(string))]
	public class FieldValueConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value.Formatted();
		}
		
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new InvalidOperationException("FormatConverter can only be used OneWay");
		}
	}
}
