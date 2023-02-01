using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Atlas.UI.Avalonia.Themes.Base;

public class DefaultTheme : Styles
{
	public DefaultTheme()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
