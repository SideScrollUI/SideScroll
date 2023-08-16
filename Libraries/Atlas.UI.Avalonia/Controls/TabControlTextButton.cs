using Atlas.UI.Avalonia.Themes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Styling;

namespace Atlas.UI.Avalonia.Controls;

public class TabControlTextButton : Button, IStyleable
{
	Type IStyleable.StyleKey => typeof(Button);

	public Brush BackgroundBrush = AtlasTheme.ActionButtonBackground;
	public Brush ForegroundBrush = AtlasTheme.ActionButtonForeground;
	public Brush HoverBrush = AtlasTheme.ButtonBackgroundHover;

	public TabControlTextButton(string? label = null)
	{
		Content = label;

		InitializeControl();
	}

	public void InitializeControl()
	{
		Background = BackgroundBrush;
		Foreground = ForegroundBrush;

		BorderBrush = new SolidColorBrush(Colors.Black);

		Resources.Add("ButtonBackgroundPointerOver", HoverBrush);
		Resources.Add("ButtonForegroundPointerOver", ForegroundBrush);

		Resources.Add("ButtonBackgroundPressed", BackgroundBrush);
		Resources.Add("ButtonForegroundPressed", ForegroundBrush);

		//Resources.Add("ButtonBorderBrushPointerOver", Theme.BorderHigh);

		Resources.Add("ButtonBorderThemeThickness", new Thickness(1.5));
		Resources.Add("ControlCornerRadius", new CornerRadius(5));
	}

	public void BindVisible(string propertyName)
	{
		var binding = new Binding(propertyName)
		{
			Path = propertyName,
			Mode = BindingMode.OneWay,
		};
		this.Bind(IsVisibleProperty, binding);
	}
}
