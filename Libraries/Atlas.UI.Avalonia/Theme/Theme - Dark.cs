using Avalonia.Media;

namespace Atlas.UI.Avalonia
{
	public class ThemeDark
	{
		//public static Brush TestBrush = Avalonia.Res
		public static Color BackgroundColor = Color.Parse("#101010");
		public static Color BackgroundFocusedColor = Color.Parse("#161616");
		public static Color ForegroundColor = Color.Parse("#f8f8f8");

		public static Color TitleBackgroundColor = Color.Parse("#4f52bb");
		public static Color TitleForegroundColor = Color.Parse("#f0f0f8");

		public static Color GridForegroundColor = Color.Parse("#f3f3f3");
		public static Color GridBackgroundColor = Color.Parse("#181818");
		public static Color GridBackgroundSelectedColor = Color.Parse("#333333");

		public static Color ButtonBackgroundColor = Color.Parse("#303281");
		public static Color ButtonForegroundColor = Colors.AliceBlue;
		//public static Color ButtonMouseOverBackgroundColor = Color.Parse("#7827d4");
		//public static Color ButtonBackgroundHoverColor = Color.Parse("#4e8ef7"); // 659aff, DefaultTheme.xaml is overriding this
		public static Color ButtonBackgroundHoverColor = Color.Parse("#7827d4"); // 659aff, DefaultTheme.xaml is overriding this

		public static Color TextBackgroundDisabledColor = Color.Parse("#c5c6c6");

		public static Color ActiveSelectionHighlightColor = GridBackgroundSelectedColor;
		public static Color InactiveSelectionHighlightColor = GridBackgroundSelectedColor;

		public static Color HasChildrenColor = Color.Parse("#224444");
		public static Color HasNoChildrenColor = Color.Parse("#222222");
	}
}

/*
Not used yet, need to move these to xaml or have xaml reference these
https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.colors?view=netframework-4.8
*/
