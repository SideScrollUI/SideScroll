using Atlas.UI.Avalonia.Themes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;

namespace Atlas.UI.Avalonia.Controls;

public class TabControlButton : Button, IStyleable
{
	Type IStyleable.StyleKey => typeof(TabControlButton);

	public Brush BackgroundBrush = AtlasTheme.ButtonBackground;
	public Brush ForegroundBrush = AtlasTheme.ButtonForeground;
	public Brush HoverBrush = AtlasTheme.ButtonBackgroundHover;

	public TabControlButton(string? label = null)
	{
		Content = label;

		InitializeControl();
	}

	public void InitializeControl()
	{
		Background = BackgroundBrush;
		//Foreground = ForegroundBrush;
		BorderBrush = new SolidColorBrush(Colors.Black);

		PointerEnter += Button_PointerEnter;
		PointerLeave += Button_PointerLeave;
	}

	private void Button_PointerEnter(object? sender, PointerEventArgs e)
	{
		//BorderBrush = new SolidColorBrush(Colors.Black); // can't overwrite hover border :(
		Background = HoverBrush;
	}

	private void Button_PointerLeave(object? sender, PointerEventArgs e)
	{
		Background = BackgroundBrush;
		//BorderBrush = button.Background;
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
