using Atlas.Core;
using Atlas.Tabs;
using Atlas.UI.Avalonia.Themes;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Collections;
using System.Reflection;

namespace Atlas.UI.Avalonia;

public class ValueToBrushConverter : IValueConverter
{
	public PropertyInfo PropertyInfo;

	public ValueToBrushConverter(PropertyInfo propertyInfo)
	{
		PropertyInfo = propertyInfo;
	}

	public sealed class BrushColors
	{
		public ISolidColorBrush HasLinks => AtlasTheme.HasLinksBackground;
		public ISolidColorBrush NoLinks => AtlasTheme.NoLinksBackground;
		public ISolidColorBrush Editable { get; set; } = AtlasTheme.Editable;
	}
	internal static BrushColors StyleBrushes { get; set; } = new();

	public bool Editable { get; set; }

	public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
	{
		try
		{
			if (PropertyInfo.IsDefined(typeof(StyleLabelAttribute)))
				return AtlasTheme.ButtonBackground;

			if (value is DictionaryEntry || PropertyInfo.IsDefined(typeof(StyleValueAttribute)))
			{
				bool hasLinks = TabUtils.ObjectHasLinks(value, true);
				if (hasLinks)
					return StyleBrushes.HasLinks; // null?
				else if (Editable && value is ListMember listMember && listMember.Editable)
					return StyleBrushes.Editable;
				else
					return StyleBrushes.NoLinks;
			}
		}
		catch (InvalidCastException)
		{
		}

		return null;
		//	return Brushes.White; // checkbox column requires a valid value
		//else
		//	return EditableBrush;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}


public class ValueToForegroundBrushConverter : IValueConverter
{
	public PropertyInfo PropertyInfo;

	public ValueToForegroundBrushConverter(PropertyInfo propertyInfo)
	{
		PropertyInfo = propertyInfo;
	}

	public sealed class BrushColors
	{
		public ISolidColorBrush HasLinks => AtlasTheme.ToolbarTextForeground; //Theme.TitleForeground;
		public ISolidColorBrush NoLinks => AtlasTheme.ToolbarTextForeground; // Should this be different?
		public ISolidColorBrush Editable { get; set; } = AtlasTheme.Editable;
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

/*
Used by DataGridPropertyTextColumn
Need to hook this into Cell.OnPaint for hover?
*/
