using Avalonia.Media;
using SideScroll.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SideScroll.Avalonia.Themes;

public class ThemeSection : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	public void UpdateProperty(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}

[Params]
public class TabTheme : ThemeSection
{
	public override string ToString() => "Tab";

	[Header("Tab"), ResourceKey("TabBackgroundBrush")]
	public Color? Background { get; set; }

	[ResourceKey("TabBackgroundFocusedBrush")]
	public Color? BackgroundFocused { get; set; }

	[ResourceKey("TabBackgroundBorderBrush")]
	public Color? Border { get; set; }

	// Title
	[Header("Title"), ResourceKey("TitleBackgroundBrush", "SystemControlBackgroundBaseLowBrush")]
	public Color? TitleBackground { get; set; }

	[ResourceKey("TitleButtonBackgroundPointerOverBrush")]
	public Color? TitleButtonBackgroundPointerOver { get; set; }

	[ResourceKey("TitleForegroundBrush")]
	public Color? TitleForeground { get; set; }

	[ResourceKey("TitleBorderBrush")]
	public Color? TitleBorder { get; set; }

	// Splitter
	[Header("Splitter"), ResourceKey("TabSplitterBackgroundBrush")]
	public Color? SplitterBackground { get; set; }

	[ResourceKey("TabSplitterSize"), Range(6, 100)]
	public double? SplitterSize { get; set; }


	[Header("Header"), ResourceKey("TabHeaderForegroundBrush")]
	public Color? HeaderForeground { get; set; }

	[Header("Separators"), ResourceKey("TabSeparatorForegroundBrush")]
	public Color? SeparatorForeground { get; set; }

	[ResourceKey("TabSectionSeparatorBrush")]
	public Color? SectionSeparator { get; set; }

	// Context Menu
	[Header("Context Menu"), ResourceKey("MenuFlyoutPresenterBackground", "ColorViewContentBackgroundBrush")]
	public Color? ContextMenuBackground { get; set; }

	[ResourceKey("MenuFlyoutItemForeground")]
	public Color? ContextMenuForeground { get; set; }


	[Header("Progress Bar"), ResourceKey("TabProgressBarForegroundBrush")]
	public Color? ProgressBarForeground { get; set; }

	// Button
	[Header("Button"), ResourceKey("ThemeButtonBackgroundBrush")]
	public Color? ButtonBackground { get; set; }

	[ResourceKey("ThemeButtonBackgroundPointerOverBrush")]
	public Color? ButtonBackgroundPointerOver { get; set; }

	[ResourceKey("ThemeButtonBackgroundPressedBrush")]
	public Color? ButtonBackgroundPressed { get; set; }

	[ResourceKey("ThemeButtonForegroundBrush")]
	public Color? ButtonForeground { get; set; }

	[ResourceKey("ThemeButtonBorderBrush")]
	public Color? ButtonBorder { get; set; }
}

[Params]
public class FontTheme : ThemeSection
{
	public override string ToString() => "Font";

	public static IEnumerable<FontFamily>? FontFamilies { get; set; }
	public static IEnumerable<string>? FontFamilyNames => FontFamilies?.Select(f => f.Name);


	[Header("Font Family"), BindList(nameof(FontFamilyNames))]
	public string? FontFamily { get; set; }

	//[ResourceKey("ContentControlThemeFontFamily")]
	//public FontWeight? ContentFontWeight { get; set; } = FontWeight.Normal;

	[BindList(nameof(FontFamilyNames))]
	public string? MonospaceFontFamily { get; set; } = "Courier New";

	[ResourceKey("MonospaceFontWeight")]
	public FontWeight? MonospaceFontWeight { get; set; } = FontWeight.Normal;

	[Header("Font Size"), Range(10, 32), ResourceKey("TitleFontSize")]
	public double TitleFontSize { get; set; } = 16;

	[Range(10, 32), ResourceKey("HeaderFontSize")]
	public double HeaderFontSize { get; set; } = 18;

	[Range(10, 32), ResourceKey("DataGridFontSize")]
	public double DataGridFontSize { get; set; } = 15;

	[Range(10, 32), ResourceKey("ControlContentThemeFontSize")]
	public double ControlContentFontSize { get; set; } = 14;
}

[Params]
public class ToolbarTheme : ThemeSection
{
	public override string ToString() => "Toolbar";

	[Header("General"), ResourceKey("ToolbarBackgroundBrush")]
	public Color? Background { get; set; }

	[ResourceKey("ToolbarSeparatorBrush")]
	public Color? Separator { get; set; }

	[Header("Labels"), ResourceKey("ToolbarLabelForegroundBrush")]
	public Color? LabelForeground { get; set; }

	[ResourceKey("ToolbarHeaderLabelForegroundBrush")]
	public Color? HeaderLabelForeground { get; set; }

	[Header("Text"), ResourceKey("ToolbarTextBackgroundBrush")]
	public Color? TextBackground { get; set; }

	[ResourceKey("ToolbarTextForegroundBrush")]
	public Color? TextForeground { get; set; }

	[Header("Button"), ResourceKey("ToolbarButtonBackgroundPointerOverBrush")]
	public Color? ButtonBackgroundPointerOver { get; set; }

	[Range(0, 20), ResourceKey("ToolbarButtonCornerRadius")]
	public double? ButtonCornerRadius { get; set; }

	[Header("Icons"), ResourceKey("IconForegroundBrush")]
	public Color? IconForeground { get; set; }

	[ResourceKey("IconForegroundHighlightBrush")]
	public Color? IconForegroundHighlight { get; set; }

	[ResourceKey("IconForegroundDisabledBrush")]
	public Color? IconForegroundDisabled { get; set; }

	[Header("Icons - Alt"), ResourceKey("IconAltForegroundBrush")]
	public Color? IconAltForeground { get; set; }

	[ResourceKey("IconAltForegroundHighlightBrush")]
	public Color? IconAltForegroundHighlight { get; set; }

	[Header("Radio Button"), ResourceKey("RadioButtonForegroundPointerOver")]
	public Color? RadioButtonForegroundPointerOver { get; set; }
}

[Params]
public class ToolTipTheme : ThemeSection
{
	public override string ToString() => "Tool Tip";

	[ResourceKey("ToolTipBackground")]
	public Color? Background { get; set; }

	[ResourceKey("ToolTipForeground")]
	public Color? Foreground { get; set; }

	[ResourceKey("ToolTipBorderBrush")]
	public Color? Border { get; set; }

	[Range(10, 32), ResourceKey("ToolTipContentThemeFontSize")]
	public double FontSize { get; set; } = 14;
}

[Params]
public class ScrollBarTheme : ThemeSection
{
	public override string ToString() => "Scroll Bar";

	[Header("ScrollBar"), ResourceKey(
		"ThemeScrollBarBackgroundBrush",
		"ScrollBarTrackFill",
		"ScrollBarTrackFillPointerOver"
		)]
	public Color? Background { get; set; }

	[ResourceKey("ScrollBarShowingBorderBrush")]
	public Color? BorderBrush { get; set; }

	// Thumb
	[Header("Thumb"), ResourceKey("ThemeScrollBarThumbBrush")]
	public Color? Thumb { get; set; }

	[ResourceKey("ThemeScrollBarThumbPointerOverBrush")]
	public Color? ThumbPointerOver { get; set; }

	// Buttons
	[Header("Buttons"), ResourceKey("ScrollBarButtonBackground")]
	public Color? ButtonBackground { get; set; }

	[ResourceKey("ScrollBarButtonBackgroundPointerOver")]
	public Color? ButtonBackgroundPointerOver { get; set; }

	[ResourceKey("ScrollBarButtonBackgroundPressed")]
	public Color? ButtonBackgroundPressed { get; set; }


	[Separator, ResourceKey("ScrollBarButtonArrowForeground")]
	public Color? ButtonArrowForeground { get; set; }

	// Doesn't work
	/*[ResourceKey("ScrollBarButtonArrowForegroundPointerOver")]
	public Color? ButtonArrowForegroundPointerOver { get; set; }*/
}

[Params]
public class DataGridTheme : ThemeSection
{
	public override string ToString() => "Data Grid";

	// Column Header
	[Header("Column Header"), ResourceKey("DataGridColumnHeaderBackgroundBrush")]
	public Color? ColumnHeaderBackground { get; set; }

	[ResourceKey("DataGridColumnHeaderBackgroundPointerOverBrush")]
	public Color? ColumnHeaderBackgroundPointerOver { get; set; }

	[ResourceKey("DataGridColumnHeaderForegroundBrush")]
	public Color? ColumnHeaderForeground { get; set; }

	[ResourceKey("DataGridColumnHeaderForegroundPointerOverBrush")]
	public Color? ColumnHeaderForegroundPointerOver { get; set; }

	[ResourceKey("DataGridHeaderSeparatorBrush")]
	public Color? ColumnHeaderSeparator { get; set; }

	// Row
	[Header("Row"), ResourceKey("DataGridRowBackgroundBrush")]
	public Color? RowBackground { get; set; }

	[ResourceKey("DataGridRowHighlightBrush")]
	public Color? RowBackgroundHighlight { get; set; }

	[ResourceKey("DataGridRowSelectedBackgroundOpacity", "DataGridRowSelectedUnfocusedBackgroundOpacity"), Range(0.0, 1.0)]
	public double? RowBackgroundLowOpacity { get; set; }

	[ResourceKey("DataGridRowSelectedHoveredBackgroundOpacity", "DataGridRowSelectedHoveredUnfocusedBackgroundOpacity"), Range(0.0, 1.0)]
	public double? RowBackgroundMediumOpacity { get; set; }

	// Cell
	[Header("Cell"), ResourceKey("DataGridCellForegroundBrush")]
	public Color? CellForeground { get; set; }

	[ResourceKey("DataGridCellForegroundPointerOverBrush")]
	public Color? CellForegroundPointerOver { get; set; }

	[ResourceKey("DataGridCellForegroundSelectedBrush")]
	public Color? CellForegroundSelected { get; set; }

	[ResourceKey("DataGridCellBorderBrush")]
	public Color? CellBorder { get; set; }

	// Cell - Focus
	[Header("Cell - Focus"), ResourceKey("DataGridCellFocusVisualPrimaryBrush")]
	public Color? CellFocusVisualPrimary { get; set; }

	[ResourceKey("DataGridCellFocusVisualSecondaryBrush")]
	public Color? CellFocusVisualSecondary { get; set; }

	// [StyleValue] attribute

	[Header("Styled"), ResourceKey("DataGridHasLinksBackgroundBrush")]
	public Color? StyledHasLinksBackground { get; set; }

	[ResourceKey("DataGridNoLinksBackgroundBrush")]
	public Color? StyledNoLinksBackground { get; set; }

	[ResourceKey("DataGridHasLinksForegroundBrush")]
	public Color? StyledHasLinksForeground { get; set; }

	[ResourceKey("DataGridStyledBorderBrush")]
	public Color? StyledBorder { get; set; }

	[Header("Border"), ResourceKey("DataGridBorderBrush")]
	public Color? Border { get; set; }
}

// Button, including TabControlTextButton
[Params]
public class ButtonTheme : ThemeSection
{
	public override string ToString() => "Button";

	// Background
	[Header("Background"), ResourceKey("ButtonBackground", "ToggleButtonBackground")]
	public Color? Background { get; set; }

	[ResourceKey("ButtonBackgroundPointerOver", "ToggleButtonBackgroundPointerOver")]
	public Color? BackgroundPointerOver { get; set; }

	[ResourceKey("ButtonBackgroundPressed", "ToggleButtonBackgroundPressed")]
	public Color? BackgroundPressed { get; set; }

	// Foreground
	[Header("Foreground"), ResourceKey("ButtonForeground", "ToggleButtonForeground")]
	public Color? Foreground { get; set; }

	[ResourceKey("ButtonForegroundPointerOver", "ToggleButtonForegroundPointerOver")]
	public Color? ForegroundPointerOver { get; set; }

	[ResourceKey("ButtonForegroundPressed", "ToggleButtonForegroundPressed")]
	public Color? ForegroundPressed { get; set; }

	// Border
	[Header("Border"), ResourceKey("ButtonBorderBrush", "ToggleButtonBorderBrush")]
	public Color? Border { get; set; }

	[ResourceKey("ButtonBorderBrushPointerOver", "ToggleButtonBorderBrushPointerOver")]
	public Color? BorderPointerOver { get; set; }

	[ResourceKey("ButtonBorderBrushPressed", "ToggleButtonBorderBrushPressed")]
	public Color? BorderPressed { get; set; }

	[Range(0, 10), ResourceKey("ButtonBorderThemeThickness")]
	public double? BorderThickness { get; set; }

	[Range(0, 20), ResourceKey("ButtonCornerRadius")]
	public double? CornerRadius { get; set; }
}

[Params]
public class TextControlTheme : ThemeSection
{
	public override string ToString() => "Text Control";

	[Header("Labels"), ResourceKey("LabelForegroundBrush")]
	public Color? LabelForeground { get; set; }

	// Background
	[Header("Text Control - Background"), ResourceKey(
		"TextControlBackground",
		"ComboBoxBackground",
		"CalendarDatePickerBackground",
		"CalendarDatePickerBackgroundPointerOver"
		)]
	public Color? TextControlBackground { get; set; }

	[ResourceKey("TextControlBackgroundReadOnlyBrush")]
	public Color? TextControlBackgroundReadOnly { get; set; }

	// Foreground
	[Header("Text Control - Foreground"), ResourceKey(
		"TextControlForeground",
		"ComboBoxForeground",
		"CalendarDatePickerForeground",
		"RadioButtonForeground",
		"CheckBoxCheckGlyphForegroundIndeterminate"
		)]
	public Color? TextControlForeground { get; set; }

	[ResourceKey(
		"TextControlForegroundFocused",
		"TextControlForegroundPointerOver",
		"ComboBoxForegroundFocused",
		//"ComboBoxForegroundPointerOver",
		"ComboBoxPlaceHolderForegroundFocusedPressed",
		"CalendarDatePickerBorderBrushPointerOver",
		"RadioButtonOuterEllipseStrokePressed"
		)]
	public Color? TextControlForegroundHighlight { get; set; }

	[ResourceKey("TextControlForegroundReadOnlyBrush")]
	public Color? TextControlForegroundReadOnly { get; set; }

	// Border
	[Header("Text Control - Border"), ResourceKey(
		"TextControlBorderBrush",
		"ComboBoxBorderBrush",
		"CalendarDatePickerBorderBrush",
		"CheckBoxCheckBackgroundStrokeUnchecked"
		)]
	public Color? TextControlBorder { get; set; }

	[ResourceKey(
		"TextControlBorderBrushPointerOver",
		"ComboBoxBorderBrushPointerOver",
		"ComboBoxBorderBrushPressed",
		"CheckBoxCheckBackgroundStrokeUncheckedPointerOver",
		"CalendarDatePickerBorderBrushPointerOver",
		"RadioButtonOuterEllipseStrokePointerOver",
		"ThemeBorderHighBrush" // Simple theme
		)]
	public Color? TextControlBorderPointerOver { get; set; }

	[Range(0, 10), ResourceKey("TextControlBorderThemeThickness",
		"TextControlBorderThemeThicknessFocused",
		"CalendarDatePickerBorderThemeThickness",
		"ComboBoxBorderThemeThickness",
		"CheckBoxBorderThemeThickness"
		)]
	public double? BorderThickness { get; set; }

	[Range(0, 20), ResourceKey("ControlCornerRadius")]
	public double? CornerRadius { get; set; }

	// Text Control - Selection
	[Header("Text Control - Selection"), ResourceKey("TextControlSelectionForegroundBrush")]
	public Color? TextControlSelectionForeground { get; set; }

	[ResourceKey("TextControlSelectionHighlightColor")]
	public Color? TextControlSelectionHighlight { get; set; }

	// ComboBox
	[Header("ComboBox"), ResourceKey("ComboBoxDropDownBackground")]
	public Color? ComboBoxDropDownBackground { get; set; }

	[ResourceKey("ComboBoxItemBackgroundSelected")]
	public Color? ComboBoxItemBackgroundSelected { get; set; }

	[ResourceKey("ComboBoxItemBackgroundPointerOver",
		"ComboBoxItemBackgroundSelectedPointerOver",
		"ComboBoxBackgroundPressed"
		)]
	public Color? ComboBoxItemBackgroundPointerOver { get; set; }

	[ResourceKey("ComboBoxItemForegroundSelected")]
	public Color? ComboBoxItemForegroundSelected { get; set; }

	// Calendar View / Date Time Picker
	[Header("Calendar View"), ResourceKey("CalendarViewBackground")]
	public Color? CalendarViewBackground { get; set; }

	[ResourceKey("CalendarViewBorderBrush")]
	public Color? CalendarViewItemBackground { get; set; }

	[ResourceKey("CalendarViewOutOfScopeBackground")]
	public Color? CalendarViewOutOfScopeBackground { get; set; }

	[ResourceKey("CalendarViewHoverBorderBrush")]
	public Color? CalendarViewHoverBorder { get; set; }

	[ResourceKey("CalendarViewCalendarItemForeground",
		"CalendarViewTodayForeground",
		"CalendarViewOutOfScopeForeground",
		"CalendarViewBlackoutForeground",
		"CalendarViewWeekDayForegroundDisabled")]
	public Color? CalendarViewForeground { get; set; }

	// Errors
	[Header("Errors"), ResourceKey("SystemControlErrorTextForegroundBrush")]
	public Color? ErrorTextForeground { get; set; }
}

[Params]
public class TextAreaTheme : ThemeSection
{
	public override string ToString() => "Text Area";

	[ResourceKey("TextAreaBackgroundBrush")]
	public Color? Background { get; set; }

	[ResourceKey("TextAreaForegroundBrush")]
	public Color? Foreground { get; set; }

	[ResourceKey("TextAreaBorderBrush")]
	public Color? Border { get; set; }
}

[Params]
public class TextEditorTheme : ThemeSection
{
	public override string ToString() => "Text Editor";

	[Header("Text"), ResourceKey("TextEditorBackgroundBrush")]
	public Color? Background { get; set; }

	[ResourceKey("TextEditorForegroundBrush")]
	public Color? Foreground { get; set; }

	[ResourceKey("TextAreaSelectionBrush")] // AvaloniaEdit name
	public Color? SelectedBackground { get; set; }

	[ResourceKey("LinkTextForegroundBrush")]
	public Color? LinkForeground { get; set; }

	// Json

	[Header("Json"), ResourceKey("JsonHighlightPunctuationBrush")]
	public Color? JsonPunctuation { get; set; }

	[ResourceKey("JsonHighlightFieldNameBrush")]
	public Color? JsonFieldName { get; set; }

	[Separator, ResourceKey("JsonHighlightStringBrush")]
	public Color? JsonString { get; set; }

	[ResourceKey("JsonHighlightNumberBrush")]
	public Color? JsonNumber { get; set; }

	[ResourceKey("JsonHighlightBoolBrush")]
	public Color? JsonBool { get; set; }

	[ResourceKey("JsonHighlightNullBrush")]
	public Color? JsonNull { get; set; }

	// Xml

	[Header("Xml"), ResourceKey("XmlHighlightCommentBrush")]
	public Color? XmlComment { get; set; }

	[ResourceKey("XmlHighlightCDataBrush")]
	public Color? XmlCData { get; set; }

	[ResourceKey("XmlHighlightDocTypeBrush")]
	public Color? XmlDocType { get; set; }

	[ResourceKey("XmlHighlightDeclarationBrush")]
	public Color? XmlDeclaration { get; set; }

	[Separator, ResourceKey("XmlHighlightTagBrush")]
	public Color? XmlTag { get; set; }

	[ResourceKey("XmlHighlightAttributeNameBrush")]
	public Color? XmlAttributeName { get; set; }

	[ResourceKey("XmlHighlightAttributeValueBrush")]
	public Color? XmlAttributeValue { get; set; }

	[ResourceKey("XmlHighlightEntityBrush")]
	public Color? XmlEntity { get; set; }

	// Color and formatting doesn't work
	//[ResourceKey("XmlHighlightBrokenEntityBrush")]
	//public Color? XmlBrokenEntity { get; set; }

	//[Header("Border"), ResourceKey("TextEditorBorderBrush")]
	//public Color? Border { get; set; }
}

[Params]
public class ChartTheme : ThemeSection
{
	public override string ToString() => "Chart";

	[Header("Chart"), ResourceKey("ChartBackgroundBrush")]
	public Color? Background { get; set; }

	[ResourceKey("ChartLabelForegroundBrush")]
	public Color? LabelForeground { get; set; }

	[ResourceKey("ChartLabelForegroundHighlightBrush")]
	public Color? LabelForegroundHighlight { get; set; }

	[Header("Lines"), ResourceKey("ChartGridLinesBrush")]
	public Color? GridLines { get; set; }

	[ResourceKey("ChartNowLineBrush")]
	public Color? NowLine { get; set; }

	[Header("Tool Tip"), ResourceKey("ChartToolTipBackgroundBrush")]
	public Color? ToolTipBackground { get; set; }

	[ResourceKey("ChartToolTipForegroundBrush")]
	public Color? ToolTipForeground { get; set; }

	[Header("Border"), ResourceKey("ChartBorderBrush")]
	public Color? Border { get; set; }

	[Range(0, 10), ResourceKey("ChartBorderThickness")]
	public double? BorderThickness { get; set; }

	[ResourceKey("ChartLegendIconBorderBrush")]
	public Color? LegendIconBorder { get; set; }

	[Inline]
	public ChartColorsTheme Colors { get; set; } = new();
}

public class ChartColorsTheme : ThemeSection
{
	public override string ToString() => "Chart Colors";

	[Header("Series Colors"), ResourceKey("ChartSeries1Brush")]
	public Color? Series1 { get; set; }

	[ResourceKey("ChartSeries2Brush")]
	public Color? Series2 { get; set; }

	[ResourceKey("ChartSeries3Brush")]
	public Color? Series3 { get; set; }

	[ResourceKey("ChartSeries4Brush")]
	public Color? Series4 { get; set; }

	[ResourceKey("ChartSeries5Brush")]
	public Color? Series5 { get; set; }

	[ResourceKey("ChartSeries6Brush")]
	public Color? Series6 { get; set; }

	[ResourceKey("ChartSeries7Brush")]
	public Color? Series7 { get; set; }

	[ResourceKey("ChartSeries8Brush")]
	public Color? Series8 { get; set; }

	[ResourceKey("ChartSeries9Brush")]
	public Color? Series9 { get; set; }

	[ResourceKey("ChartSeries10Brush")]
	public Color? Series10 { get; set; }
}
