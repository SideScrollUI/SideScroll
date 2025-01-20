using Avalonia.Data.Converters;
using Avalonia.Media;
using SideScroll.Attributes;
using SideScroll.Avalonia.Themes;
using SideScroll.Tabs;
using SideScroll.Tabs.Lists;
using System.Collections;
using System.Reflection;

namespace SideScroll.Avalonia.Controls.DataGrids;

public class ValueToBackgroundBrushConverter(PropertyInfo propertyInfo) : IValueConverter
{
	public PropertyInfo PropertyInfo => propertyInfo;

	public sealed class BrushColors
	{
		public ISolidColorBrush HasLinks => SideScrollTheme.DataGridHasLinksBackground;
		public ISolidColorBrush NoLinks => SideScrollTheme.DataGridNoLinksBackground;
		public ISolidColorBrush Editable { get; set; } = SideScrollTheme.DataGridEditableBackground;
	}
	internal static BrushColors StyleBrushes { get; set; } = new();

	public bool Editable { get; set; }

	public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
	{
		try
		{
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

/*
Used by DataGridPropertyTextColumn
Need to hook this into Cell.OnPaint for hover?
*/
