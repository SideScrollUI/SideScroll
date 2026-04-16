using Avalonia.Data.Converters;
using Avalonia.Media;
using SideScroll.Attributes;
using SideScroll.Avalonia.Themes;
using SideScroll.Tabs;
using System.Collections;
using System.Reflection;

namespace SideScroll.Avalonia.Controls.DataGrids;

/// <summary>Converts a data-grid cell value to a foreground brush, using theme colors to distinguish cells that have navigable links from plain-value cells.</summary>
public class ValueToForegroundBrushConverter(PropertyInfo propertyInfo) : IValueConverter
{
	/// <summary>Gets the property whose value (or link metadata) determines the cell foreground color.</summary>
	public PropertyInfo PropertyInfo => propertyInfo;

	/// <summary>Holds the theme brushes used for different cell text states.</summary>
	public sealed class BrushColors
	{
		/// <summary>Gets the foreground brush for cells whose value has navigable links.</summary>
		public ISolidColorBrush HasLinks => SideScrollTheme.DataGridHasLinksForeground;

		/// <summary>Gets the foreground brush for cells whose value has no navigable links.</summary>
		public ISolidColorBrush NoLinks => SideScrollTheme.ToolbarTextForeground;
	}
	public static BrushColors StyleBrushes { get; set; } = new();

	/// <summary>Gets or sets whether editable cells use a distinct foreground brush.</summary>
	public bool Editable { get; set; }

	/// <summary>Returns the appropriate foreground brush for the given cell value.</summary>
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

	/// <summary>Not supported; always throws <see cref="NotSupportedException"/>.</summary>
	public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}
