using Atlas.UI.Avalonia.Themes;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Atlas.UI.Avalonia.Controls.Toolbar;

public class ToolbarTextBox : TextBox
{
	protected override Type StyleKeyOverride => typeof(TextBox);

	public ToolbarTextBox(string text = "")
	{
		Text = text;
		TextWrapping = TextWrapping.NoWrap;
		VerticalAlignment = VerticalAlignment.Center;

		LoadTheme();

		ActualThemeVariantChanged += (sender, e) => LoadTheme();
	}

	private void LoadTheme()
	{
		Background = AtlasTheme.ToolbarTextBackground;
		Foreground = AtlasTheme.ToolbarTextForeground;
		CaretBrush = AtlasTheme.ToolbarTextCaret;

		// Fluent
		Resources["TextControlBackgroundPointerOver"] = Background;
		Resources["TextControlBackgroundFocused"] = Background;
		Resources["TextControlPlaceholderForegroundFocused"] = AtlasTheme.ToolbarTextForeground;
		Resources["TextControlPlaceholderForegroundPointerOver"] = AtlasTheme.ToolbarTextForeground;
	}
}
