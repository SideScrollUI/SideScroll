using Avalonia;
using Avalonia.Media;

namespace Atlas.UI.Avalonia.Themes;

public static class Theme
{
	public static SolidColorBrush Background => Get("ThemeBackgroundBrush");
	public static SolidColorBrush BackgroundFocused => Get("ThemeBackgroundFocusedBrush");
	public static SolidColorBrush Foreground => Get("ThemeForegroundBrush");
	public static SolidColorBrush BackgroundText => Get("ThemeBackgroundTextBrush");
	public static SolidColorBrush TabBackground => Get("ThemeTabBackgroundBrush");

	public static SolidColorBrush BorderHigh => GetColorBrush("ThemeBorderHighColor");
	public static SolidColorBrush GridStyledLines => Get("ThemeGridStyledLinesBrush");

	// Content
	public static SolidColorBrush GridForeground => Get("ThemeGridForegroundBrush");
	public static SolidColorBrush GridBackground => Get("ThemeGridBackgroundBrush");
	public static SolidColorBrush GridBackgroundSelected => Get("ThemeGridBackgroundSelectedBrush");
	public static SolidColorBrush GridBorder => Get("ThemeGridBorderBrush");

	// Links
	public static SolidColorBrush StyledLabelForeground => Get("ThemeStyledLabelForegroundBrush");
	public static SolidColorBrush HasLinksBackground => Get("ThemeHasLinksBrush");
	public static SolidColorBrush NoLinksBackground => Get("ThemeNoLinksBrush");
	public static SolidColorBrush StyleLineBackground => Get("ThemeStyleLineBrush");

	// ScrollBar
	public static SolidColorBrush ScrollBarThumb => Get("ThemeScrollBarThumbBrush");

	// Button
	public static SolidColorBrush ActionButtonBackground => Get("ThemeActionButtonBackgroundBrush");
	public static SolidColorBrush ActionButtonForeground => Get("ThemeActionButtonForegroundBrush");
	public static SolidColorBrush ButtonBackground => Get("ThemeButtonBackgroundBrush");
	public static SolidColorBrush ButtonForeground => Get("ThemeButtonForegroundBrush");
	public static SolidColorBrush ButtonBackgroundHover => Get("ThemeButtonBackgroundHoverBrush");

	// Title
	public static SolidColorBrush TitleBackground => Get("TitleBackgroundBrush");
	public static SolidColorBrush TitleForeground => Get("TitleForegroundBrush");

	public static SolidColorBrush TextBackground => Get("ThemeTextBackgroundBrush");
	public static SolidColorBrush ForegroundLight => Get("ThemeForegroundLightBrush");

	public static SolidColorBrush TextBackgroundDisabled => Get("TextBackgroundDisabledBrush");

	public static SolidColorBrush Editable => Get("EditableBrush");

	// Toolbar
	public static SolidColorBrush ToolbarButtonBackground => Get("ToolbarButtonBackgroundBrush");
	public static SolidColorBrush ToolbarButtonBackgroundHover => Get("ToolbarButtonBackgroundHoverBrush");
	public static SolidColorBrush ToolbarButtonSeparator => Get("ToolbarButtonSeparatorBrush");

	public static SolidColorBrush ToolbarLabelForeground => Get("ToolbarLabelForegroundBrush");

	public static SolidColorBrush ToolbarTextBackground => Get("ToolbarTextBackgroundBrush");
	public static SolidColorBrush ToolbarTextForeground => Get("ToolbarTextForegroundBrush");
	public static SolidColorBrush ToolbarCaret => Get("ToolbarCaretBrush");

	public static SolidColorBrush ToolbarBorderMid => Get("ToolbarBorderMidBrush");
	public static SolidColorBrush ToolbarBorderHigh => Get("ToolbarBorderHighBrush");

	public static SolidColorBrush ControlBackgroundHover => Get("ControlBackgroundHoverBrush");

	public static SolidColorBrush IconForeground => Get("IconForegroundBrush");

	// Chart 
	public static SolidColorBrush ChartBackgroundSelected => Get("ThemeChartBackgroundSelectedBrush");
	public static double ChartBackgroundSelectedAlpha => GetDouble("ChartBackgroundSelectedAlpha");
	public static double SplitterSize => GetDouble("SplitterSize");

	public static SolidColorBrush Get(string brushName)
	{
		if (Application.Current!.Styles.TryGetResource(brushName, out object? obj))
			return (SolidColorBrush)obj!;

		throw new Exception("Brush not found: " + brushName);
	}

	public static Color GetColor(string colorName)
	{
		if (Application.Current!.Styles.TryGetResource(colorName, out object? obj))
			return (Color)obj!;

		throw new Exception("Color not found: " + colorName);
	}

	public static SolidColorBrush GetColorBrush(string colorName)
	{
		return new SolidColorBrush(GetColor(colorName));
	}

	private static double GetDouble(string name)
	{
		if (Application.Current!.Styles.TryGetResource(name, out object? value))
			return (double)value!;

		throw new Exception("Double not found: " + name);
	}
}
