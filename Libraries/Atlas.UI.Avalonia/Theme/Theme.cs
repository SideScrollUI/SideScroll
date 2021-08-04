using Avalonia;
using Avalonia.Media;
using System;
using System.Resources;

namespace Atlas.UI.Avalonia
{
	public class Theme
	{
		public static SolidColorBrush Background => Get("ThemeBackgroundBrush");
		public static SolidColorBrush BackgroundFocused => Get("ThemeBackgroundFocusedBrush");
		public static SolidColorBrush Foreground => Get("ThemeForegroundBrush");
		public static SolidColorBrush BackgroundText => Get("ThemeBackgroundTextBrush");
		public static SolidColorBrush TabBackground => Get("ThemeTabBackgroundBrush");

		// Content
		public static SolidColorBrush GridForeground => Get("ThemeGridForegroundBrush");
		public static SolidColorBrush GridBackground => Get("ThemeGridBackgroundBrush");
		public static SolidColorBrush GridBackgroundSelected => Get("ThemeGridBackgroundSelectedBrush");
		
		// Links
		public static SolidColorBrush HasLinksBackground => Get("ThemeHasLinksBrush");
		public static SolidColorBrush NoLinksBackground => Get("ThemeNoLinksBrush");

		// Button
		public static SolidColorBrush ButtonBackground => Get("ThemeButtonBackgroundBrush");
		public static SolidColorBrush ButtonForeground => Get("ThemeButtonForegroundBrush");
		public static SolidColorBrush ButtonBackgroundHover => Get("ThemeButtonBackgroundHoverBrush");

		// Title
		public static SolidColorBrush TitleBackground => Get("TitleBackgroundBrush");
		public static SolidColorBrush TitleForeground => Get("TitleForegroundBrush");

		public static SolidColorBrush TextBackgroundBrush => Get("ThemeTextBackgroundBrush");

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

		// Chart 
		public static SolidColorBrush ChartBackgroundSelected => Get("ThemeChartBackgroundSelectedBrush");
		public static double ChartBackgroundSelectedAlpha => GetDouble("ChartBackgroundSelectedAlpha");
		public static double SplitterSize => GetDouble("SplitterSize");

		public static SolidColorBrush Get(string brushName)
		{
			if (Application.Current.Styles.TryGetResource(brushName, out object obj))
				return (SolidColorBrush)obj;

			throw new Exception("Brush not found: " + brushName);
		}

		private static double GetDouble(string name)
		{
			if (Application.Current.Styles.TryGetResource(name, out object value))
				return (double)value;

			throw new Exception("Double not found: " + name);
		}
	}
}
