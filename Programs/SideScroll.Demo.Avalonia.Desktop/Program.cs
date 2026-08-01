using Avalonia;
using SideScroll.Avalonia.Controls.ScreenCapture;
using SideScroll.Avalonia.Controls.Viewer;
using SideScroll.Avalonia.Samples;
using SideScroll.Utilities;
using System.Diagnostics;

namespace SideScroll.Demo.Avalonia.Desktop;

internal static class Program
{
	[STAThread]
	private static int Main(string[] args)
	{
		AppBuilder builder = BuildAvaloniaApp();

		TabViewer.Plugins.Add(new ScreenCapture.TabViewerPlugin());

		try
		{
			return builder.StartWithClassicDesktopLifetime(args);
		}
		catch (Exception e)
		{
			var settings = SampleProjectSettings.Default;
			LogUtils.Save(settings.ExceptionsPath, settings.Name!, e);
			Debug.Fail(e.ToString());
			return 1;
		}
	}

	private static AppBuilder BuildAvaloniaApp()
		=> AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.WithInterFont()
			.LogToTrace();
}
