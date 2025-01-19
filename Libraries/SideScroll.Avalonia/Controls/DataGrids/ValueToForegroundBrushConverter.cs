using Avalonia.Data.Converters;
using Avalonia.Media;
using SideScroll.Attributes;
using SideScroll.Avalonia.Themes;
using SideScroll.Tabs;
using System.Collections;
using System.Reflection;

namespace SideScroll.Avalonia.Controls.DataGrids;

public class ValueToForegroundBrushConverter(PropertyInfo propertyInfo) : IValueConverter
{
	public PropertyInfo PropertyInfo => propertyInfo;

	public sealed class BrushColors
	{
		public ISolidColorBrush HasLinks => SideScrollTheme.DataGridHasLinksForeground; //Theme.TitleForeground;
		public ISolidColorBrush NoLinks => SideScrollTheme.ToolbarTextForeground; // Should this be different?
		// public ISolidColorBrush Editable { get; set; } = SideScrollTheme.Editable;
	}
	internal static BrushColors StyleBrushes { get; set; } = new();

	public bool Editable { get; set; }

	public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
	{
		try
		{
			if (value is DictionaryEntry ||
				PropertyInfo.IsDefined(typeof(StyleValueAttribute)))
			{
				bool hasLinks = TabUtils.ObjectHasLinks(value, true);
				if (hasLinks)
					return StyleBrushes.HasLinks;
				else
					return StyleBrushes.NoLinks;
			}
		}
		catch (InvalidCastException)
		{
		}

		return Brushes.Black;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}
