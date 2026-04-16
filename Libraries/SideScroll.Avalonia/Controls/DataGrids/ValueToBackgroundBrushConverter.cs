using Avalonia.Data.Converters;
using Avalonia.Media;
using SideScroll.Attributes;
using SideScroll.Avalonia.Themes;
using SideScroll.Tabs;
using SideScroll.Tabs.Lists;
using System.Collections;
using System.Reflection;

namespace SideScroll.Avalonia.Controls.DataGrids;

/// <summary>Converts a data-grid cell value to a background brush, using theme colors to indicate whether the cell has linked data, is editable, or is a plain value.</summary>
public class ValueToBackgroundBrushConverter(PropertyInfo propertyInfo) : IValueConverter
{
	/// <summary>Gets the property whose value (or link metadata) determines the cell background color.</summary>
	public PropertyInfo PropertyInfo => propertyInfo;

	/// <summary>Holds the theme brushes used for different cell states.</summary>
	public sealed class BrushColors
	{
		/// <summary>Gets the background brush for cells whose value has navigable links.</summary>
		public ISolidColorBrush HasLinks => SideScrollTheme.DataGridHasLinksBackground;

		/// <summary>Gets the background brush for cells whose value has no navigable links.</summary>
		public ISolidColorBrush NoLinks => SideScrollTheme.DataGridNoLinksBackground;

		/// <summary>Gets or sets the background brush for editable cells.</summary>
		public ISolidColorBrush Editable { get; set; } = SideScrollTheme.DataGridEditableBackground;
	}
	public static BrushColors StyleBrushes { get; set; } = new();

	/// <summary>Gets or sets whether editable cells receive a distinct background brush.</summary>
	public bool Editable { get; set; }

	/// <summary>Returns the appropriate background brush for the given cell value.</summary>
	public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
	{
		try
		{
			if (value is DictionaryEntry || PropertyInfo.IsDefined(typeof(StyleValueAttribute)))
			{
				bool hasLinks = TabUtils.ObjectHasLinks(value, true);
				if (hasLinks)
				{
					return StyleBrushes.HasLinks; // null?
				}
				else if (Editable && value is ListMember listMember && listMember.IsEditable)
				{
					return StyleBrushes.Editable;
				}
				else
				{
					return StyleBrushes.NoLinks;
				}
			}
		}
		catch (InvalidCastException)
		{
		}

		return null;
	}

	/// <summary>Not supported; always throws <see cref="NotSupportedException"/>.</summary>
	public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}
