using Atlas.UI.Avalonia.Themes;
using Atlas.UI.Avalonia.Utilities;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;

namespace Atlas.UI.Avalonia.Controls;

// ReadOnly string control with wordwrap, scrolling, and clipboard copy
// See TabControlAvaloniaEdit for an editable version
public class TabControlTextArea : Border
{
	public string Text { get; set; }

	public TextBlock TextBlock { get; set; }

	public TabControlTextArea(string? text = null)
	{
		Text = text ?? "";

		Background = AtlasTheme.TextAreaBackground;
		BorderBrush = Brushes.Black;
		BorderThickness = new Thickness(1);
		HorizontalAlignment = HorizontalAlignment.Stretch;
		VerticalAlignment = VerticalAlignment.Top;
		MinWidth = 50;
		MaxWidth = TabControlParams.ControlMaxWidth;
		Margin = new Thickness(6);
		Padding = new Thickness(6, 3);

		TextBlock = new TextBlock
		{
			Background = AtlasTheme.TextAreaBackground, // Set background for ContentMenu
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch,
			Foreground = AtlasTheme.TitleForeground,
			TextWrapping = TextWrapping.Wrap,
			Text = Text,
			MinHeight = 24, // Single lines can get clipped if this is too low
		};
		AvaloniaUtils.AddContextMenu(TextBlock);

		var scrollViewer = new ScrollViewer
		{
			Content = TextBlock,
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch,
			HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
			VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
			BorderThickness = new Thickness(2), // doesn't work
			BorderBrush = Brushes.Black,
			Padding = new Thickness(10),
		};

		Child = scrollViewer;
	}
}
