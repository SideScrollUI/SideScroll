using Avalonia;
using SideScroll.Avalonia.Controls.Viewer;
using SideScroll.Avalonia.Samples;
using SideScroll.Avalonia.ScreenCapture;
using SideScroll.Utilities;

namespace SideScroll.Demo.Avalonia.Desktop;

static class Program
{
	[STAThread]
	static int Main(string[] args)
	{
		AppBuilder builder = BuildAvaloniaApp();

		TabViewer.Plugins.Add(new ScreenCapture.TabViewerPlugin());

		try
		{
			return builder.StartWithClassicDesktopLifetime(args);
		}
		catch (Exception e)
		{
			var settings = SampleProjectSettings.Settings;
			LogUtils.Save(settings.ExceptionsPath, settings.Name!, e);
			return 1;
		}
	}

	private static AppBuilder BuildAvaloniaApp()
		=> AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.WithInterFont()
			.LogToTrace();
}
