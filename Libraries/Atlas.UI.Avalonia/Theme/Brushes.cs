using Avalonia;
using Avalonia.Media;

namespace Atlas.UI.Avalonia.Theme
{
	public class Brushes
	{
		public static SolidColorBrush Background => Get("ThemeBackgroundBrush");
		public static SolidColorBrush BackgroundFocused => Get("ThemeBackgroundFocusedBrush");
		public static SolidColorBrush Foreground => Get("ThemeForegroundBrush");
		public static SolidColorBrush BackgroundText => Get("ThemeBackgroundTextBrush");

		// Content
		public static SolidColorBrush GridForeground => Get("ThemeGridForegroundBrush");
		public static SolidColorBrush GridBackground => Get("ThemeGridBackgroundBrush");
		public static SolidColorBrush GridBackgroundSelected => Get("ThemeGridBackgroundSelectedBrush");
		
		// Links
		public static SolidColorBrush HasLinks => Get("ThemeHasLinksBrush");
		public static SolidColorBrush NoLinks => Get("ThemeNoLinksBrush");


		// Button
		public static SolidColorBrush ButtonBackground => Get("ThemeButtonBackgroundBrush");
		public static SolidColorBrush ButtonForeground => Get("ThemeButtonForegroundBrush");
		public static SolidColorBrush ButtonBackgroundHover => Get("ThemeButtonBackgroundHoverBrush");


		// Title
		public static SolidColorBrush TitleBackground => Get("TitleBackgroundBrush");
		public static SolidColorBrush TitleForeground => Get("TitleForegroundBrush");

		public static SolidColorBrush TextBackgroundDisabled => Get("TextBackgroundDisabledBrush");

		public static SolidColorBrush Editable => Get("EditableBrush");

		// Toolbar
		public static SolidColorBrush ToolbarButtonBackground => Get("ToolbarButtonBackgroundBrush");
		public static SolidColorBrush ToolbarButtonBackgroundHover => Get("ToolbarButtonBackgroundHoverBrush");
		public static SolidColorBrush ToolbarButtonSeparator => Get("ToolbarButtonSeparatorBrush");
		public static SolidColorBrush ToolbarTextForeground => Get("ToolbarTextForegroundBrush");

		public static SolidColorBrush ControlBackgroundHover => Get("ControlBackgroundHoverBrush");

		public static SolidColorBrush Get(string brushName)
		{
			if (Application.Current.Styles.TryGetResource(brushName, out object obj))
				return (SolidColorBrush)obj;
			return null;
		}
	}
}
