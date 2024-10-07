using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace SideScroll.UI.Avalonia.Themes;

public static class SideScrollTheme
{
	public static SolidColorBrush TabBackground => GetBrush("TabBackgroundBrush");
	public static SolidColorBrush TabBackgroundBorder => GetBrush("TabBackgroundBorderBrush");
	public static SolidColorBrush TabBackgroundFocused => GetBrush("TabBackgroundFocusedBrush");
	public static SolidColorBrush TabProgressBarForeground => GetBrush("TabProgressBarForegroundBrush");

	// Title
	public static SolidColorBrush TitleBackground => GetBrush("TitleBackgroundBrush");
	public static SolidColorBrush TitleButtonBackgroundPointerOver => GetBrush("TitleButtonBackgroundPointerOverBrush");
	public static SolidColorBrush TitleForeground => GetBrush("TitleForegroundBrush");

	// Toolbar
	public static SolidColorBrush ToolbarBackground => GetBrush("ToolbarBackgroundBrush");
	public static SolidColorBrush ToolbarButtonBackgroundPointerOver => GetBrush("ToolbarButtonBackgroundPointerOverBrush");
	public static SolidColorBrush ToolbarSeparator => GetBrush("ToolbarSeparatorBrush");

	public static SolidColorBrush ToolbarLabelForeground => GetBrush("ToolbarLabelForegroundBrush");

	public static SolidColorBrush ToolbarTextBackground => GetBrush("ToolbarTextBackgroundBrush");
	public static SolidColorBrush ToolbarTextForeground => GetBrush("ToolbarTextForegroundBrush");
	public static SolidColorBrush ToolbarTextCaret => GetBrush("ToolbarTextCaretBrush");

	// ToolTip
	public static SolidColorBrush ToolTipBackground => GetBrush("ToolTipBackground");
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
	public static SolidColorBrush TextReadOnlyForeground => GetBrush("TextControlForegroundReadOnlyBrush");
	public static SolidColorBrush TextReadOnlyBackground => GetBrush("TextControlBackgroundReadOnlyBrush");

	// TextArea 
	public static SolidColorBrush TextAreaBackground => GetBrush("TextAreaBackgroundBrush");
	public static SolidColorBrush TextAreaForeground => GetBrush("TextAreaForegroundBrush");

	// Chart
	public static SolidColorBrush ChartBackgroundSelected => GetBrush("ChartBackgroundSelectedBrush");
	public static double ChartBackgroundSelectedAlpha => GetDouble("ChartBackgroundSelectedAlpha");

	public static SolidColorBrush ChartLabelForeground => GetBrush("ChartLabelForegroundBrush");
	public static SolidColorBrush ChartLabelForegroundHighlight => GetBrush("ChartLabelForegroundHighlightBrush");

	public static SolidColorBrush ChartGridLines => GetBrush("ChartGridLinesBrush");
	public static SolidColorBrush ChartNowLine => GetBrush("ChartNowLineBrush");

	public static SolidColorBrush ChartLegendIconBorder => GetBrush("ChartLegendIconBorderBrush");

	public static SolidColorBrush ChartToolTipBackground => GetBrush("ChartToolTipBackgroundBrush");
	public static SolidColorBrush ChartToolTipForeground => GetBrush("ChartToolTipForegroundBrush");

	public static SolidColorBrush ChartSeries(int index) => GetBrush($"ChartSeries{index}Brush");

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
	public static FontWeight MonospaceFontWeight => GetFontWeight("MonospaceFontWeight");

	public static ThemeVariant ThemeVariant => Application.Current!.ActualThemeVariant;

	public static FontFamily SourceCodeProFont => GetFontFamily("SourceCodeProFont");

	public static object GetResource(string name)
	{
		if (Application.Current!.Resources.ThemeDictionaries.TryGetValue(ThemeVariant, out IThemeVariantProvider? provider))
		{
			if (provider.TryGetResource(name, null, out object? providerObject))
			{
				return providerObject!;
			}
		}

		if (Application.Current.TryGetResource(name, ThemeVariant, out object? obj))
		{
			return obj!;
		}

		throw new Exception($"Resource not found: {name}");
	}

	public static Color GetColor(string colorName)
	{
		return (Color)GetResource(colorName);
	}

	public static SolidColorBrush GetBrush(string brushName)
	{
		return (SolidColorBrush)GetResource(brushName);
	}

	public static double GetDouble(string name)
	{
		object obj = GetResource(name);
		if (obj is Thickness thickness)
		{
			return thickness.Left;
		}
		else if (obj is CornerRadius cornerRadius)
		{
			return cornerRadius.BottomLeft;
		}
		else
		{
			return (double)GetResource(name);
		}
	}

	public static FontFamily GetFontFamily(string name)
	{
		if (Application.Current!.TryGetResource(name, ThemeVariant, out object? value))
		{
			return (FontFamily)value!;
		}

		throw new Exception($"FontFamily not found: {name}");
	}

	public static FontWeight GetFontWeight(string name)
	{
		if (Application.Current!.TryGetResource(name, ThemeVariant, out object? value))
		{
			return (FontWeight)value!;
		}

		throw new Exception($"FontWeight not found: {name}");
	}

	public static Thickness GetThickness(string name)
	{
		if (Application.Current!.TryGetResource(name, ThemeVariant, out object? value))
		{
			return (Thickness)value!;
		}

		throw new Exception($"Thickness not found: {name}");
	}
}
