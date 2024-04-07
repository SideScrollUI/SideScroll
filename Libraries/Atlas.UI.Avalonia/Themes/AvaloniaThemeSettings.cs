using Atlas.Core;
using Atlas.Tabs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Atlas.UI.Avalonia.Themes;

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
	public DataGridTheme DataGrid { get; set; } = new();
	[Inline]
	public ButtonTheme Button { get; set; } = new();
	[Inline]
	public TextControlTheme TextControl { get; set; } = new();
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
		DataGrid,
		Button,
		TextControl,
		TextEditor,
		Chart,
	];

	public IEnumerable<ListProperty> GetProperties() => ListProperty.Create(this);

	public void Update(AvaloniaThemeSettings newSettings)
	{
		var newProperties = newSettings.GetProperties().GetEnumerator();
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
		Font.FontFamily = AtlasTheme.ContentControlThemeFontFamily.Name;
		Font.MonospaceFontFamily = AtlasTheme.MonospaceFontFamily.Name;

		foreach (ListProperty listProperty in GetProperties())
		{
			if (listProperty.GetCustomAttribute<ResourceKeyAttribute>() is not ResourceKeyAttribute attribute) continue;

			if (listProperty.UnderlyingType == typeof(Color))
			{
				Color color = AtlasTheme.GetBrush(attribute.Names.First()).Color;
				listProperty.Value = color;
			}
			else if (listProperty.UnderlyingType == typeof(double))
			{
				double value = AtlasTheme.GetDouble(attribute.Names.First());
				listProperty.Value = value;
			}
		}
	}

	public bool HasNullValue()
	{
		Application.Current!.RequestedThemeVariant = GetVariant();

		return GetProperties()
			.Any(property => property.GetCustomAttribute<ResourceKeyAttribute>() != null && property.Value == null);
	}

	public void FillMissingValues()
	{
		var original = Application.Current!.RequestedThemeVariant;
		Application.Current!.RequestedThemeVariant = GetVariant();

		foreach (ListProperty listProperty in GetProperties())
		{
			if (listProperty.GetCustomAttribute<ResourceKeyAttribute>() is not ResourceKeyAttribute attribute) continue;

			if (listProperty.Value != null) continue;

			if (listProperty.UnderlyingType == typeof(Color))
			{
				Color color = AtlasTheme.GetBrush(attribute.Names.First()).Color;
				listProperty.Value = color;
			}
			else if (listProperty.UnderlyingType == typeof(double))
			{
				double value = AtlasTheme.GetDouble(attribute.Names.First());
				listProperty.Value = value;
			}
		}
		Application.Current!.RequestedThemeVariant = original;
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
					dictionary[name] = d;
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

	[ResourceKey("TabBackgroundBrush")]
	public Color? Background { get; set; }

	[ResourceKey("TabBackgroundFocusedBrush")]
	public Color? BackgroundFocused { get; set; }

	[Separator, ResourceKey("TitleBackgroundBrush")]
	public Color? TitleBackground { get; set; }

	[ResourceKey("TitleForegroundBrush")]
	public Color? TitleForeground { get; set; }

	[Separator, ResourceKey("TabSeparatorForegroundBrush")]
	public Color? SeparatorForeground { get; set; }

	[Separator, ResourceKey("MenuFlyoutPresenterBackground")]
	public Color? ContextMenuBackground { get; set; }

	[Separator, ResourceKey("TabProgressBarForegroundBrush")]
	public Color? ProgressBarForeground { get; set; }
}

[Params]
public class FontTheme : ThemeSection
{
	public override string ToString() => "Font";

	public static IEnumerable<FontFamily>? FontFamilies { get; set; }
	public static IEnumerable<string>? FontFamilyNames => FontFamilies?.Select(f => f.Name);

	[BindList(nameof(FontFamilyNames))]
	public string? FontFamily { get; set; }

	[BindList(nameof(FontFamilyNames))]
	public string? MonospaceFontFamily { get; set; } = "Courier New";

	[Separator, Range(10, 32), ResourceKey("TitleFontSize")]
	public double TitleFontSize { get; set; } = 16;

	[Range(10, 32), ResourceKey("DataGridFontSize")]
	public double DataGridFontSize { get; set; } = 15;

	[Range(10, 32), ResourceKey("ControlContentThemeFontSize")]
	public double ControlContentFontSize { get; set; } = 14;
}

[Params]
public class ToolbarTheme : ThemeSection
{
	public override string ToString() => "Toolbar";

	[ResourceKey("ToolbarBackgroundBrush")]
	public Color? Background { get; set; }

	[ResourceKey("ToolbarSeparatorBrush")]
	public Color? Separator { get; set; }

	[Separator, ResourceKey("ToolbarLabelForegroundBrush")]
	public Color? LabelForeground { get; set; }

	[ResourceKey("ToolbarHeaderLabelForegroundBrush")]
	public Color? HeaderLabelForeground { get; set; }

	[Separator, ResourceKey("ToolbarTextBackgroundBrush")]
	public Color? TextBackground { get; set; }

	[ResourceKey("ToolbarTextForegroundBrush")]
	public Color? TextForeground { get; set; }

	[Separator, ResourceKey("ToolbarButtonBackgroundPointerOverBrush")]
	public Color? ButtonBackgroundPointerOver { get; set; }

	[Separator, ResourceKey("IconForegroundBrush")]
	public Color? IconForeground { get; set; }

	[ResourceKey("IconForegroundHighlightBrush")]
	public Color? IconForegroundHighlight { get; set; }

	[Separator, ResourceKey("IconAltForegroundBrush")]
	public Color? IconAltForeground { get; set; }

	[ResourceKey("IconAltForegroundHighlightBrush")]
	public Color? IconAltForegroundHighlight { get; set; }

	[Separator, ResourceKey("IconForegroundDisabledBrush")]
	public Color? IconForegroundDisabled { get; set; }

	[Separator, ResourceKey("RadioButtonForegroundPointerOver")]
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
public class DataGridTheme : ThemeSection
{
	public override string ToString() => "Data Grid";

	[Separator, ResourceKey("DataGridColumnHeaderBackgroundBrush")]
	public Color? ColumnHeaderBackground { get; set; }

	[ResourceKey("ThemeButtonBackgroundBrushPointerOver")]
	public Color? ColumnHeaderBackgroundPointerOver { get; set; }

	[ResourceKey("DataGridColumnHeaderForegroundBrush")]
	public Color? ColumnHeaderForeground { get; set; }

	[ResourceKey("DataGridColumnHeaderForegroundBrushPointerOver")]
	public Color? ColumnHeaderForegroundPointerOver { get; set; }

	[Separator, ResourceKey("DataGridRowBackgroundBrush")]
	public Color? RowBackground { get; set; }

	[ResourceKey("DataGridRowHighlightBrush")]
	public Color? RowBackgroundHighlight { get; set; }

	[ResourceKey("DataGridCellForegroundBrush")]
	public Color? CellForeground { get; set; }

	[ResourceKey("DataGridCellForegroundBrushPointerOver")]
	public Color? CellForegroundPointerOver { get; set; }

	//[ResourceKey("DataGridForegroundSelectedBrush")]
	//public Color? ForegroundSelected { get; set; }

	[ResourceKey("DataGridCellBorderBrush")]
	public Color? CellBorder { get; set; }

	// [StyleValue] attribute

	[Separator, ResourceKey("DataGridHasLinksBackgroundBrush")]
	public Color? StyledHasLinksBackground { get; set; }

	[ResourceKey("DataGridHasLinksForegroundBrush")]
	public Color? StyledHasLinksForeground { get; set; }

	[ResourceKey("DataGridNoLinksBackgroundBrush")]
	public Color? StyledNoLinksBackground { get; set; }

	[ResourceKey("DataGridStyledBorderBrush")]
	public Color? StyledBorder { get; set; }
}

// Button, including TabControlTextButton
[Params]
public class ButtonTheme : ThemeSection
{
	public override string ToString() => "Button";

	[ResourceKey("ButtonBackground")]
	public Color? Background { get; set; }

	[ResourceKey("ButtonBackgroundPointerOver")]
	public Color? BackgroundPointerOver { get; set; }

	[ResourceKey("ButtonBackgroundPressed")]
	public Color? BackgroundPressed { get; set; }

	[ResourceKey("ButtonForeground", "ButtonForegroundPointerOver", "ButtonForegroundPressed")]
	public Color? Foreground { get; set; }
}

[Params]
public class TextControlTheme : ThemeSection
{
	public override string ToString() => "Text Control";

	[ResourceKey("LabelForegroundBrush")]
	public Color? LabelForeground { get; set; }

	[Separator, ResourceKey(
		"TextControlBackground",
		"ComboBoxBackground",
		"CalendarDatePickerBackground"
		)]
	public Color? TextControlBackground { get; set; }

	[ResourceKey(
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

	[ResourceKey(
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

	[Separator, ResourceKey("TextControlSelectionForegroundBrush")]
	public Color? TextControlSelectionForeground { get; set; }

	[ResourceKey("TextControlSelectionHighlightColor")]
	public Color? TextControlSelectionHighlight { get; set; }

	[Separator, ResourceKey("SystemControlErrorTextForegroundBrush")]
	public Color? ErrorTextForeground { get; set; }
}

[Params]
public class TextEditorTheme : ThemeSection
{
	public override string ToString() => "Text Editor";

	[ResourceKey("TextEditorBackgroundBrush")]
	public Color? Background { get; set; }

	[ResourceKey("TextEditorForegroundBrush")]
	public Color? Foreground { get; set; }

	[ResourceKey("TextAreaSelectionBrush")] // AvaloniaEdit name
	public Color? SelectedBackground { get; set; }

	[ResourceKey("LinkTextForegroundBrush")]
	public Color? LinkForeground { get; set; }

	// Json

	[Separator, ResourceKey("JsonHighlightPunctuationBrush")]
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

	[Separator, ResourceKey("XmlHighlightCommentBrush")]
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

	[ResourceKey("ChartLabelForegroundHighlightBrush")]
	public Color? LabelForegroundHighlight { get; set; }
}
