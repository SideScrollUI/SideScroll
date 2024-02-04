using Atlas.UI.Avalonia.Themes;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Atlas.UI.Avalonia.Controls;

public class ToolbarTextBox : TextBox
{
	protected override Type StyleKeyOverride => typeof(TextBox);

	public ToolbarTextBox(string text = "")
	{
		Text = text;
		TextWrapping = TextWrapping.NoWrap;
		VerticalAlignment = VerticalAlignment.Center;
		Background = AtlasTheme.ToolbarTextBackground;
		Foreground = AtlasTheme.ToolbarTextForeground;
		CaretBrush = AtlasTheme.ToolbarCaret;

		// Fluent
		Resources.Add("TextControlBackgroundPointerOver", Background);
		Resources.Add("TextControlBackgroundFocused", Background);
		Resources.Add("TextControlPlaceholderForegroundFocused", Foreground);
		Resources.Add("TextControlPlaceholderForegroundPointerOver", Foreground);
	}
}
