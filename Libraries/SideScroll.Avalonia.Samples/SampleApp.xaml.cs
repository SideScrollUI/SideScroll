using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SideScroll.Avalonia.Samples.Tabs;
using SideScroll.Tabs.Bookmarks.Tabs;

namespace SideScroll.Avalonia.Samples;

public class App : Application
{
	public override void Initialize()
	{
		TabSchema.DefaultRootTab = new TabAvaloniaSamples();

		AvaloniaXamlLoader.Load(this);
	}

	protected virtual Control CreateSingleView() => new SampleMainView();

	public override void OnFrameworkInitializationCompleted()
	{
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
		{
			desktopLifetime.MainWindow = new SampleMainWindow();
		}
		else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewLifetime)
		{
			singleViewLifetime.MainView = CreateSingleView();
		}

		base.OnFrameworkInitializationCompleted();
	}
}
