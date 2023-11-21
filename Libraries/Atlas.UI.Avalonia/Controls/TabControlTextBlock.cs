using Atlas.UI.Avalonia.Themes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using System.Diagnostics.CodeAnalysis;

namespace Atlas.UI.Avalonia.Controls;

// ReadOnly string control with wordwrap, scrolling, and clipboard copy
public class TabControlTextBlock : Border
{
	public string Text { get; set; }

	public TextBlock TextBlock { get; set; }

	protected override Type StyleKeyOverride => typeof(TextBlock);

	public TabControlTextBlock(string? text)
	{
		Text = text ?? "";

		InitializeComponent();
	}

	[MemberNotNull(nameof(TextBlock))]
	private void InitializeComponent()
	{
		Background = AtlasTheme.TextBackground;
		BorderBrush = Brushes.Black;
		BorderThickness = new Thickness(1);
		HorizontalAlignment = HorizontalAlignment.Stretch;
		VerticalAlignment = VerticalAlignment.Top;
		MinWidth = 50;
		MaxWidth = TabControlParams.ControlMaxWidth;
		Margin = new Thickness(6);
		Padding = new Thickness(6, 3);

		TextBlock = new TextBlock()
		{
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch,
			Foreground = AtlasTheme.TitleForeground,
			TextWrapping = TextWrapping.Wrap,
			FontSize = 14,
			Text = Text,
		};
		AvaloniaUtils.AddContextMenu(TextBlock);

		var scrollViewer = new ScrollViewer()
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
