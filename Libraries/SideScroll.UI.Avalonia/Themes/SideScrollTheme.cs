using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace SideScroll.UI.Avalonia.Themes;

public static class SideScrollTheme
{
	public static SolidColorBrush TabBackground => GetBrush("TabBackgroundBrush");
	public static SolidColorBrush TabBackgroundFocused => GetBrush("TabBackgroundFocusedBrush");
	public static SolidColorBrush TabProgressBarForeground => GetBrush("TabProgressBarForegroundBrush");

	// Title
	public static SolidColorBrush TitleBackground => GetBrush("TitleBackgroundBrush");
	public static SolidColorBrush TitleForeground => GetBrush("TitleForegroundBrush");

	// Toolbar
	public static SolidColorBrush ToolbarBackground => GetBrush("ToolbarBackgroundBrush");
	public static SolidColorBrush ToolbarButtonBackgroundPointerOver => GetBrush("ToolbarButtonBackgroundPointerOverBrush");
	public static SolidColorBrush ToolbarSeparator => GetBrush("ToolbarSeparatorBrush");

	public static SolidColorBrush ToolbarLabelForeground => GetBrush("ToolbarLabelForegroundBrush");

	public static SolidColorBrush ToolbarTextBackground => GetBrush("ToolbarTextBackgroundBrush");
	public static SolidColorBrush ToolbarTextForeground => GetBrush("ToolbarTextForegroundBrush");
	public static SolidColorBrush ToolbarTextCaret => GetBrush("ToolbarTextCaretBrush");

	public static SolidColorBrush ToolTipForeground => GetBrush("ToolTipForeground");

	// Icon
	public static SolidColorBrush IconForeground => GetBrush("IconForegroundBrush");
	public static SolidColorBrush IconForegroundHighlight => GetBrush("IconForegroundHighlightBrush");
	public static SolidColorBrush IconAltForeground => GetBrush("IconAltForegroundBrush");
	public static SolidColorBrush IconAltForegroundHighlight => GetBrush("IconAltForegroundHighlightBrush");
	public static SolidColorBrush IconForegroundDisabled => GetBrush("IconForegroundDisabledBrush");

	// DataGrid
	public static SolidColorBrush DataGridRowHighlight => GetBrush("DataGridRowHighlightBrush");

	// DataGrid [StyleValue]
	public static SolidColorBrush DataGridHasLinksBackground => GetBrush("DataGridHasLinksBackgroundBrush");
	public static SolidColorBrush DataGridHasLinksForeground => GetBrush("DataGridHasLinksForegroundBrush");
	public static SolidColorBrush DataGridNoLinksBackground => GetBrush("DataGridNoLinksBackgroundBrush");
	public static SolidColorBrush DataGridStyledBorder => GetBrush("DataGridStyledBorderBrush");

	public static SolidColorBrush DataGridEditableBackground => GetBrush("DataGridEditableBackgroundBrush");

	// Button
	public static SolidColorBrush ButtonBackground => GetBrush("ThemeButtonBackgroundBrush");
	public static SolidColorBrush ButtonForeground => GetBrush("ThemeButtonForegroundBrush");

	public static SolidColorBrush LabelForeground => GetBrush("LabelForegroundBrush");
	//public static SolidColorBrush LabelHighlightForeground => GetBrush("LabelHighlightForegroundBrush");

	public static SolidColorBrush TextControlBackground => GetBrush("TextControlBackground");

	public static SolidColorBrush TextAreaBackground => GetBrush("TextAreaBackgroundBrush");
	public static SolidColorBrush TextReadOnlyForeground => GetBrush("TextReadOnlyForegroundBrush");
	public static SolidColorBrush TextReadOnlyBackground => GetBrush("TextReadOnlyBackgroundBrush");

	// Chart 
	public static SolidColorBrush ChartBackgroundSelected => GetBrush("ChartBackgroundSelectedBrush");
	public static SolidColorBrush ChartLabelForegroundHighlight => GetBrush("ChartLabelForegroundHighlightBrush");
	public static double ChartBackgroundSelectedAlpha => GetDouble("ChartBackgroundSelectedAlpha");

	// Text Editor
	public static SolidColorBrush TextEditorBackgroundBrush => GetBrush("TextEditorBackgroundBrush");
	public static SolidColorBrush TextEditorForegroundBrush => GetBrush("TextEditorForegroundBrush");
	public static SolidColorBrush LinkTextForegroundBrush => GetBrush("LinkTextForegroundBrush");

	public static SolidColorBrush JsonHighlightPunctuationBrush => GetBrush("JsonHighlightPunctuationBrush");
	public static SolidColorBrush JsonHighlightFieldNameBrush => GetBrush("JsonHighlightFieldNameBrush");
	public static SolidColorBrush JsonHighlightStringBrush => GetBrush("JsonHighlightStringBrush");
	public static SolidColorBrush JsonHighlightNumberBrush => GetBrush("JsonHighlightNumberBrush");
	public static SolidColorBrush JsonHighlightBoolBrush => GetBrush("JsonHighlightBoolBrush");
	public static SolidColorBrush JsonHighlightNullBrush => GetBrush("JsonHighlightNullBrush");

	public static SolidColorBrush XmlHighlightCommentBrush => GetBrush("XmlHighlightCommentBrush");
	public static SolidColorBrush XmlHighlightCDataBrush => GetBrush("XmlHighlightCDataBrush");
	public static SolidColorBrush XmlHighlightDocTypeBrush => GetBrush("XmlHighlightDocTypeBrush");
	public static SolidColorBrush XmlHighlightDeclarationBrush => GetBrush("XmlHighlightDeclarationBrush");
	public static SolidColorBrush XmlHighlightTagBrush => GetBrush("XmlHighlightTagBrush");
	public static SolidColorBrush XmlHighlightAttributeNameBrush => GetBrush("XmlHighlightAttributeNameBrush");
	public static SolidColorBrush XmlHighlightAttributeValueBrush => GetBrush("XmlHighlightAttributeValueBrush");
	public static SolidColorBrush XmlHighlightEntityBrush => GetBrush("XmlHighlightEntityBrush");
	public static SolidColorBrush XmlHighlightBrokenEntityBrush => GetBrush("XmlHighlightBrokenEntityBrush");

	// Sizes
	public static double TabSplitterSize => GetDouble("TabSplitterSize");
	public static double TitleFontSize => GetDouble("TitleFontSize");
	public static double DataGridFontSize => GetDouble("DataGridFontSize");

	// Fonts
	public static FontFamily ContentControlThemeFontFamily => GetFontFamily("ContentControlThemeFontFamily");
	public static FontFamily MonospaceFontFamily => GetFontFamily("MonospaceFontFamily");

	public static ThemeVariant ThemeVariant => Application.Current!.ActualThemeVariant;

	public static Color GetColor(string colorName)
	{
		if (Application.Current!.Resources.ThemeDictionaries.TryGetValue(ThemeVariant, out IThemeVariantProvider? provider))
		{
			if (provider.TryGetResource(colorName, null, out object? providerObject))
			{
				return (Color)providerObject!;
			}
		}

		if (Application.Current.TryGetResource(colorName, ThemeVariant, out object? obj))
		{
			return (Color)obj!;
		}

		throw new Exception($"Color not found: {colorName}");
	}

	public static SolidColorBrush GetBrush(string brushName)
	{
		if (Application.Current!.Resources.ThemeDictionaries.TryGetValue(ThemeVariant, out IThemeVariantProvider? provider))
		{
			if (provider.TryGetResource(brushName, ThemeVariant, out object? providerObject))
			{
				return (SolidColorBrush)providerObject!;
			}
		}

		if (Application.Current.TryGetResource(brushName, ThemeVariant, out object? obj))
		{
			return (SolidColorBrush)obj!;
		}

		throw new Exception($"Brush not found: {brushName}");
	}

	public static double GetDouble(string name)
	{
		if (Application.Current!.Resources.ThemeDictionaries.TryGetValue(ThemeVariant, out IThemeVariantProvider? provider))
		{
			if (provider.TryGetResource(name, ThemeVariant, out object? providerObject))
			{
				return (double)providerObject!;
			}
		}

		if (Application.Current.TryGetResource(name, ThemeVariant, out object? value))
		{
			return (double)value!;
		}

		throw new Exception($"Double not found: {name}");
	}

	public static FontFamily GetFontFamily(string name)
	{
		if (Application.Current!.TryGetResource(name, ThemeVariant, out object? value))
		{
			return (FontFamily)value!;
		}

		throw new Exception($"FontFamily not found: {name}");
	}
}
