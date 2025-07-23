using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using SideScroll.Avalonia.Utilities;

namespace SideScroll.Avalonia.Controls;

// ReadOnly string control with wordwrap, scrolling, and clipboard copy
// See TabAvaloniaEdit for an editable version
public class TabTextArea : Border
{
	public string Text { get; set; }

	public TextBlock TextBlock { get; protected set; }

	public TabTextArea(string? text = null)
	{
		Text = text ?? "";

		MaxWidth = TabObjectEditor.ControlMaxWidth;

		TextBlock = new TextBlock
		{
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch,
			TextWrapping = TextWrapping.Wrap,
			Text = Text,
			Margin = new Thickness(10),
		};
		AvaloniaUtils.AddContextMenu(TextBlock);

		var scrollViewer = new ScrollViewer
		{
			Content = TextBlock,
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch,
			HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
			VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
		};

		Child = scrollViewer;
	}
}
