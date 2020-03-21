using Avalonia.Media;

namespace Atlas.UI.Avalonia
{
	public class Theme
	{
		public static Color BackgroundColor = Color.Parse("#1e1e1e");
		public static Color BackgroundFocusedColor = Color.Parse("#262626");

		public static Color SplitterColor = Color.Parse("#111111");

		public static Color TitleBackgroundColor = Color.Parse("#4f52bb");
		public static Color TitleForegroundColor = Color.Parse("#f0f0f8");


		public static Color GridColumnHeaderForegroundColor = Color.Parse("#f0f0f8");
		public static Color GridColumnHeaderBackgroundColor = Color.Parse("#324699");

		public static Color GridForegroundColor = Colors.Black;
		public static Color GridBackgroundColor = Color.Parse("#FFFFFF");

		public static Color GridSelectedBackgroundColor = Color.Parse("#005555");

		public static Color ButtonBackgroundColor = Colors.SlateBlue;
		public static Color ButtonForegroundColor = Colors.AliceBlue;
		//public static Color ButtonMouseOverBackgroundColor = Color.Parse("#7827d4");
		//public static Color ButtonBackgroundHoverColor = Color.Parse("#4e8ef7"); // 659aff, DefaultTheme.xaml is overriding this
		public static Color ButtonBackgroundHoverColor = Color.Parse("#FF7827d4"); // 659aff, DefaultTheme.xaml is overriding this

		public static Color TextBackgroundDisabledColor = Color.Parse("#c5c6c6");

		// 
		public static Color ActiveSelectionHighlightColor = GridSelectedBackgroundColor;
		public static Color InactiveSelectionHighlightColor = GridSelectedBackgroundColor;

		public static Color EditableColor = Color.Parse("#c8c2f9");

		public static Color HasChildrenColor = Color.Parse("#f4c68d");
		public static Color HasNoChildrenColor = Colors.LightGray;
		//public static Color HasChildrenColor = Colors.Tan;

		// Notes
		//public static Color NoteForegroundColor = Color.Parse("#000000");
		//public static Color NotesBackgroundColor = Color.Parse("#d9bda4");

		public static Color NotesForegroundColor = Color.Parse("#eeeeee");
		public static Color NotesBackgroundColor = Color.Parse("#2d2d30");

		public static Color NotesButtonForegroundColor = Color.Parse("#000000");
		public static Color NotesButtonBackgroundColor = Color.Parse("#cecece");
		public static Color NotesButtonBackgroundHoverColor = Color.Parse("#4e8ef7"); // DefaultTheme.xaml is overriding this

		// Toolbar
		public static Color ToolbarButtonBackgroundColor = Color.Parse("#2a2954");
		//public static Color ToolbarButtonBackgroundColor = Color.Parse("#7e7e7e");
		//public static Color ToolbarButtonBackgroundColor = Color.Parse("#cecece");
		public static Color ToolbarButtonBackgroundHoverColor = Color.Parse("#4e8ef7"); // 659aff, DefaultTheme.xaml is overriding this
		public static Color ToolbarButtonSeparatorColor = Color.Parse("#004db0");
		public static Color ToolbarTextForegroundColor = Color.Parse("#8888FF");

		public static Color ControlBackgroundHover = Color.Parse("#ddffdd");
	}
}

/*
https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.colors?view=netframework-4.8
*/
