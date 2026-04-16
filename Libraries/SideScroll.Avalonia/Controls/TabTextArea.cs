using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using SideScroll.Avalonia.Utilities;

namespace SideScroll.Avalonia.Controls;

/// <summary>
/// A read-only, word-wrapping, scrollable text area with a clipboard copy context menu.
/// For an editable version, see <see cref="TextEditor.TabAvaloniaEdit"/>.
/// </summary>
public class TabTextArea : Border
{
	/// <summary>Gets the raw text displayed in this area.</summary>
	public string Text { get; }

	/// <summary>Gets the text block that renders the content.</summary>
	public TextBlock TextBlock { get; }

	public TabTextArea(string? text = null)
	{
		Text = text ?? "";

		MaxWidth = TabForm.ControlMaxWidth;

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
