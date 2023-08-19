using Atlas.Core;
using Atlas.Tabs.Tools;
using Atlas.UI.Avalonia.Tabs;
using Avalonia;
using OxyPlot.Avalonia;

namespace Atlas.Start.Avalonia;

static class Program
{
	static int Main(string[] args)
	{
		RegisterFileTypes();
		OxyPlotModule.EnsureLoaded();
		AppBuilder builder = BuildAvaloniaApp();

		try
		{
			return builder.StartWithClassicDesktopLifetime(args);
		}
		catch (Exception e)
		{
			LogUtils.LogException(e, "Atlas", "Atlas.Start.Avalonia");
			return 1;
		}
	}

	private static void RegisterFileTypes()
	{
		TabFile.RegisterType<TabFileImage>(".png", ".jpg", ".jpeg", ".gif", ".bmp");
	}

	public static AppBuilder BuildAvaloniaApp()
		=> AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.With(new Win32PlatformOptions
			{
				//UseDeferredRendering = false, // Causes DataGrid blank columns when scrolling right?
				AllowEglInitialization = true,
			})
			.LogToTrace();
}
