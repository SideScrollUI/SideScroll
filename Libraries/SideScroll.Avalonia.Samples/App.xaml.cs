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

	public override void OnFrameworkInitializationCompleted()
	{
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
		{
			desktopLifetime.MainWindow = new MainWindow();
		}
		else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewLifetime)
		{
			// Check if browser-specific view exists (for localStorage support)
			var browserViewType = Type.GetType("SideScroll.Demo.Avalonia.Browser.BrowserMainView, SideScroll.Demo.Avalonia.Browser");
			if (browserViewType != null)
			{
				singleViewLifetime.MainView = (Control?)Activator.CreateInstance(browserViewType);
				Console.WriteLine("✓ Using BrowserMainView with localStorage support");
			}
			else
			{
				singleViewLifetime.MainView = new MainView();
			}
		}

		base.OnFrameworkInitializationCompleted();
	}
}
