using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using SideScroll.UI.Avalonia.Themes;

namespace SideScroll.UI.Avalonia.Controls.Toolbar;

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
		Background = SideScrollTheme.ToolbarTextBackground;
		Foreground = SideScrollTheme.ToolbarTextForeground;
		CaretBrush = SideScrollTheme.ToolbarTextCaret;

		// Fluent
		Resources["TextControlBackgroundPointerOver"] = Background;
		Resources["TextControlBackgroundFocused"] = Background;
		Resources["TextControlPlaceholderForegroundFocused"] = SideScrollTheme.ToolbarTextForeground;
		Resources["TextControlPlaceholderForegroundPointerOver"] = SideScrollTheme.ToolbarTextForeground;
	}
}
