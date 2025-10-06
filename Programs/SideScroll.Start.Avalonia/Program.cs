using Avalonia;
using SideScroll.Avalonia.Controls.Viewer;
using SideScroll.Avalonia.Samples;
using SideScroll.Avalonia.ScreenCapture;
using SideScroll.Utilities;

namespace SideScroll.Start.Avalonia;

static class Program
{
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

	public static AppBuilder BuildAvaloniaApp()
		=> AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.WithInterFont()
			.LogToTrace();
}
