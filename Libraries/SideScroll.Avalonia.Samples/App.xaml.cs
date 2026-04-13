using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace SideScroll.Avalonia.Samples;

public class App : Application
{
	public override void Initialize()
	{
		AvaloniaXamlLoader.Load(this);
	}

	protected virtual Control CreateSingleView() => new MainView();

	public override void OnFrameworkInitializationCompleted()
	{
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
		{
			desktopLifetime.MainWindow = new MainWindow();
		}
		else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewLifetime)
		{
			singleViewLifetime.MainView = CreateSingleView();
		}

		base.OnFrameworkInitializationCompleted();
	}
}
