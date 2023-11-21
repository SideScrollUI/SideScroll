using Atlas.Resources;
using Atlas.UI.Avalonia.Themes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using System.Diagnostics.CodeAnalysis;

namespace Atlas.UI.Avalonia.Controls;

public class TabControlSearch : Grid
{
	protected override Type StyleKeyOverride => typeof(Grid);

	public TextBox TextBoxSearch;

	public string? Text
	{
		get => TextBoxSearch.Text;
		set => TextBoxSearch.Text = value;
	}

	public TabControlSearch()
	{
		Initialize();
	}

	[MemberNotNull(nameof(TextBoxSearch))]
	private void Initialize()
	{
		ColumnDefinitions = new ColumnDefinitions("*");
		RowDefinitions = new RowDefinitions("Auto");

		HorizontalAlignment = HorizontalAlignment.Stretch;
		VerticalAlignment = VerticalAlignment.Top;

		Focusable = true;

		AddTextBox();
		AddIcon();
	}

	[MemberNotNull(nameof(TextBoxSearch))]
	private void AddTextBox()
	{
		TextBoxSearch = new TextBox()
		{
			VerticalContentAlignment = VerticalAlignment.Center,
			Padding = new Thickness(5, 3, 25, 3),
			Watermark = "Search",
			Background = AtlasTheme.ToolbarTextBackground,
			Foreground = AtlasTheme.ToolbarTextForeground,
			CaretBrush = AtlasTheme.ToolbarCaret,
		};

		TextBoxSearch.Resources.Add("ThemeBackgroundHoverBrush", TextBoxSearch.Background); // Disable for now
		TextBoxSearch.Resources.Add("ThemeBorderMidBrush", AtlasTheme.ToolbarBorderMid);
		TextBoxSearch.Resources.Add("ThemeBorderHighBrush", AtlasTheme.ToolbarBorderHigh);

		// Fluent
		TextBoxSearch.Resources.Add("TextControlBackgroundPointerOver", TextBoxSearch.Background);
		TextBoxSearch.Resources.Add("TextControlBackgroundFocused", TextBoxSearch.Background);
		TextBoxSearch.Resources.Add("TextControlPlaceholderForegroundFocused", TextBoxSearch.Foreground);
		TextBoxSearch.Resources.Add("TextControlPlaceholderForegroundPointerOver", TextBoxSearch.Foreground);

		Children.Add(TextBoxSearch);
	}

	private void AddIcon()
	{
		Stream stream = Icons.Png.Search16.Stream;
		stream.Seek(0, SeekOrigin.Begin);

		var image = new Image
		{
			Width = 16,
			Height = 16,
			Source = new Bitmap(stream),
			Margin = new Thickness(7, 4),
			HorizontalAlignment = HorizontalAlignment.Right,
		};
		Children.Add(image);
	}

	protected override void OnGotFocus(GotFocusEventArgs e)
	{
		TextBoxSearch.Focus();
	}
}
