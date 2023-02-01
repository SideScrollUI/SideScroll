using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Atlas.UI.Avalonia.Themes;

public class FluentTheme : Styles
{
	public FluentTheme()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
