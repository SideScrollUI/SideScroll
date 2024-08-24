using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using SideScroll.Attributes;
using SideScroll.Tabs.Lists;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace SideScroll.UI.Avalonia.Themes;

[AttributeUsage(AttributeTargets.Property)]
public class ResourceKeyAttribute(params string[] names) : Attribute
{
	public readonly string[] Names = names;
}

[Params]
public class AvaloniaThemeSettings : INotifyPropertyChanged
{
	[Required, StringLength(50)]
	public string? Name { get; set; }

	public static List<string> Variants =>
	[
		"Light",
		"Dark",
	];

	[ReadOnly(true)]
	public string? Variant { get; set; }

	[Inline]
	public FontTheme Font { get; set; } = new();
	[Inline]
	public TabTheme Tab { get; set; } = new();
	[Inline]
	public ToolbarTheme Toolbar { get; set; } = new();
	[Inline]
	public ToolTipTheme ToolTip { get; set; } = new();
	[Inline]
	public ScrollBarTheme ScrollBar { get; set; } = new();
	[Inline]
	public DataGridTheme DataGrid { get; set; } = new();
	[Inline]
	public ButtonTheme Button { get; set; } = new();
	[Inline]
	public TextControlTheme TextControl { get; set; } = new();
	[Inline]
	public TextAreaTheme TextArea { get; set; } = new();
	[Inline]
	public TextEditorTheme TextEditor { get; set; } = new();
	[Inline]
	public ChartTheme Chart { get; set; } = new();

	public event PropertyChangedEventHandler? PropertyChanged;

	public override string? ToString() => Name;

	public List<object> GetSections() =>
	[
		Font,
		Tab,
		Toolbar,
		ToolTip,
		ScrollBar,
		DataGrid,
		Button,
		TextControl,
		TextArea,
		TextEditor,
		Chart,
	];

	public IEnumerable<ListProperty> GetProperties() => ListProperty.Create(this);

	public void Update(AvaloniaThemeSettings newSettings)
	{
		using var newProperties = newSettings.GetProperties().GetEnumerator();
		foreach (ListProperty listProperty in GetProperties())
		{
			object? existingValue = listProperty.Value;
			newProperties.MoveNext();
			object? newValue = newProperties.Current.Value;
			if (newValue?.Equals(existingValue) == true) continue;

			listProperty.Value = newValue;

			if (listProperty.Object is ThemeSection themeSection)
			{
				themeSection.UpdateProperty(listProperty.Name!);
			}
			else
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(listProperty.Name));
			}
		}
	}

	public void LoadFromCurrent()
	{
		Font.FontFamily = SideScrollTheme.ContentControlThemeFontFamily.Name;
		Font.MonospaceFontFamily = SideScrollTheme.MonospaceFontFamily.Name;

		foreach (ListProperty listProperty in GetProperties())
		{
			if (listProperty.GetCustomAttribute<ResourceKeyAttribute>() is not ResourceKeyAttribute attribute) continue;

			if (listProperty.UnderlyingType == typeof(Color))
			{
				Color color = SideScrollTheme.GetBrush(attribute.Names.First()).Color;
				listProperty.Value = color;
			}
			else if (listProperty.UnderlyingType == typeof(double))
			{
				double value = SideScrollTheme.GetDouble(attribute.Names.First());
				listProperty.Value = value;
			}
		}
	}

	public bool HasNullValue()
	{
		return GetProperties()
			.Any(property => property.GetCustomAttribute<ResourceKeyAttribute>() != null && property.Value == null);
	}

	public void FillMissingValues()
	{
		var original = Application.Current!.RequestedThemeVariant;
		Application.Current.RequestedThemeVariant = GetVariant();

		foreach (ListProperty listProperty in GetProperties())
		{
			if (listProperty.GetCustomAttribute<ResourceKeyAttribute>() is not ResourceKeyAttribute attribute) continue;

			if (listProperty.Value != null) continue;

			if (listProperty.UnderlyingType == typeof(Color))
			{
				Color color = SideScrollTheme.GetBrush(attribute.Names.First()).Color;
				listProperty.Value = color;
			}
			else if (listProperty.UnderlyingType == typeof(double))
			{
				double value = SideScrollTheme.GetDouble(attribute.Names.First());
				listProperty.Value = value;
			}
		}
		Application.Current.RequestedThemeVariant = original;
	}

	public ResourceDictionary CreateDictionary()
	{
		var dictionary = new ResourceDictionary
		{
			["ContentControlThemeFontFamily"] = FontTheme.FontFamilies?.FirstOrDefault(f => f.Name == Font.FontFamily),
			["MonospaceFontFamily"] = FontTheme.FontFamilies?.FirstOrDefault(f => f.Name == Font.MonospaceFontFamily),

			["IconForegroundColor"] = Toolbar.IconForeground,
		};

		foreach (ListMember listProperty in GetProperties())
		{
			if (listProperty.GetCustomAttribute<ResourceKeyAttribute>() is not ResourceKeyAttribute attribute) continue;

			object? value = listProperty.Value;
			foreach (string name in attribute.Names)
			{
				if (value is Color color)
				{
					dictionary[name] = new SolidColorBrush(color);
				}
				else if (value is double d)
				{
					// Todo: Improve, Add generic attribute support to ListProperty.GetCustomAttribute()
					if (name.Contains("Thickness"))
					{
						dictionary[name] = new Thickness(d);
					}
					else
					{
						dictionary[name] = d;
					}
				}
				else if (value is null)
				{
					Debug.WriteLine($"Property {listProperty} is null");
				}
			}
		}

		return dictionary;
	}

	// Multiple Variants with the same name will give different results, so always use the actual ones
	public ThemeVariant GetVariant()
	{
		return Variant switch
		{
			"Light" => ThemeVariant.Light,
			"Dark" => ThemeVariant.Dark,
			_ => ThemeVariant.Default
		};
	}
}

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

	[Header("Background"), ResourceKey("TabBackgroundBrush")]
	public Color? Background { get; set; }

	[ResourceKey("TabBackgroundFocusedBrush")]
	public Color? BackgroundFocused { get; set; }

	// Title
	[Header("Title"), Separator, ResourceKey("TitleBackgroundBrush")]
	public Color? TitleBackground { get; set; }

	[ResourceKey("TitleForegroundBrush")]
	public Color? TitleForeground { get; set; }

	[ResourceKey("TitleBorderBrush")]
	public Color? TitleBorder { get; set; }

	// Splitter
	[Header("Splitter"), Separator, ResourceKey("TabSplitterBackgroundBrush")]
	public Color? SplitterBackground { get; set; }

	[ResourceKey("TabSplitterSize"), Range(6, 100)]
	public double? SplitterSize { get; set; }


	[Header("Header"), ResourceKey("TabHeaderForegroundBrush")]
	public Color? HeaderForeground { get; set; }

	[Header("Separator"), ResourceKey("TabSeparatorForegroundBrush")]
	public Color? SeparatorForeground { get; set; }

	
	[Header("Context Menu"), ResourceKey("MenuFlyoutPresenterBackground")]
	public Color? ContextMenuBackground { get; set; }

	[ResourceKey("MenuFlyoutItemForeground")]
	public Color? ContextMenuForeground { get; set; }


	[Header("Progress Bar"), ResourceKey("TabProgressBarForegroundBrush")]
	public Color? ProgressBarForeground { get; set; }

	// Button
	[Header("Button"), ResourceKey("ThemeButtonBackgroundBrush")]
	public Color? ButtonBackground { get; set; }

	[ResourceKey("ThemeButtonForegroundBrush")]
	public Color? ButtonForeground { get; set; }

	[ResourceKey("ThemeButtonBackgroundPointerOverBrush")]
	public Color? ButtonBackgroundPointerOver { get; set; }

	[ResourceKey("ThemeButtonBackgroundPressedBrush")]
	public Color? ButtonBackgroundPressed { get; set; }
}

[Params]
public class FontTheme : ThemeSection
{
	public override string ToString() => "Font";

	public static IEnumerable<FontFamily>? FontFamilies { get; set; }
	public static IEnumerable<string>? FontFamilyNames => FontFamilies?.Select(f => f.Name);

	[Header("Font Family"), BindList(nameof(FontFamilyNames))]
	public string? FontFamily { get; set; }

	[BindList(nameof(FontFamilyNames))]
	public string? MonospaceFontFamily { get; set; } = "Courier New";

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

	[Header("ScrollBar"), ResourceKey("ThemeScrollBarBackgroundBrush")]
	public Color? Background { get; set; }

	[Header("Thumb"), ResourceKey("ThemeScrollBarThumbBrush", "ScrollBarThumbBackgroundColor")]
	public Color? Thumb { get; set; }

	[ResourceKey("ThemeScrollBarThumbPointerOverBrush")]
	public Color? ThumbPointerOver { get; set; }


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

	[ResourceKey("DataGridRowSelectedBackgroundOpacity", "DataGridRowSelectedUnfocusedBackgroundOpacity")]
	public double? RowBackgroundLowOpacity { get; set; }

	[ResourceKey("DataGridRowSelectedHoveredBackgroundOpacity", "DataGridRowSelectedHoveredUnfocusedBackgroundOpacity")]
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

	[ResourceKey("DataGridHasLinksForegroundBrush")]
	public Color? StyledHasLinksForeground { get; set; }

	[ResourceKey("DataGridNoLinksBackgroundBrush")]
	public Color? StyledNoLinksBackground { get; set; }

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
	[Header("Background"), ResourceKey("ButtonBackground")]
	public Color? Background { get; set; }

	[ResourceKey("ButtonBackgroundPointerOver")]
	public Color? BackgroundPointerOver { get; set; }

	[ResourceKey("ButtonBackgroundPressed")]
	public Color? BackgroundPressed { get; set; }

	// Foreground
	[Header("Foreground"), ResourceKey("ButtonForeground")]
	public Color? Foreground { get; set; }

	[ResourceKey("ButtonForegroundPointerOver")]
	public Color? ForegroundPointerOver { get; set; }

	[ResourceKey("ButtonForegroundPressed")]
	public Color? ForegroundPressed { get; set; }

	// Border
	[Header("Border"), ResourceKey("ButtonBorderBrush")]
	public Color? Border { get; set; }

	[ResourceKey("ButtonBorderBrushPointerOver")]
	public Color? BorderPointerOver { get; set; }

	[Range(0, 10), ResourceKey("ButtonBorderThemeThickness")]
	public double? BorderThickness { get; set; }
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
		"CalendarDatePickerBackground"
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
	public Color? TextControlForegroundHigh { get; set; }

	[ResourceKey("TextControlForegroundReadOnlyBrush")]
	public Color? TextControlForegroundReadOnly { get; set; }

	// Border
	[Header("Text Control - Border"), ResourceKey(
		"TextControlBorderBrush",
		"ComboBoxBorderBrush",
		"CalendarDatePickerBorderBrush"
		)]
	public Color? TextControlBorder { get; set; }

	[ResourceKey(
		"TextControlBorderBrushPointerOver",
		"ComboBoxBorderBrushPointerOver",
		"CheckBoxCheckBackgroundStrokeUncheckedPointerOver",
		"CalendarDatePickerBorderBrushPointerOver",
		"RadioButtonOuterEllipseStrokePointerOver",
		"ThemeBorderHighBrush" // Simple theme
		)]
	public Color? TextControlBorderPointerOver { get; set; }

	// Text Control - Selection
	[Header("Text Control - Selection"), ResourceKey("TextControlSelectionForegroundBrush")]
	public Color? TextControlSelectionForeground { get; set; }

	[ResourceKey("TextControlSelectionHighlightColor")]
	public Color? TextControlSelectionHighlight { get; set; }

	// ComboBox
	[Header("ComboBox"), ResourceKey("ComboBoxItemBackgroundSelected")]
	public Color? ComboBoxItemBackgroundSelected { get; set; }

	[ResourceKey("ComboBoxItemBackgroundPointerOver", "ComboBoxItemBackgroundSelectedPointerOver")]
	public Color? ComboBoxItemBackgroundPointerOver { get; set; }

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

	[ResourceKey("XmlHighlightTagBrush")]
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

	[ResourceKey("ChartBorderThickness")]
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
