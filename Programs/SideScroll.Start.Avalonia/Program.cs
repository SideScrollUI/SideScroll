using Avalonia;
using Avalonia.Data.Core.Plugins;
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

		// Remove Default DataAnnotations Validators
		// These validators show before values are entered, which ends up showing too many initial warnings
		// https://docs.avaloniaui.net/docs/data-binding/data-validation
		// Add custom template?
		BindingPlugins.DataValidators.RemoveAt(0);

		TabViewer.Plugins.Add(new ScreenCapture.TabViewerPlugin());

		try
		{
			return builder.StartWithClassicDesktopLifetime(args);
		}
		catch (Exception e)
		{
			LogUtils.LogException(e, "SideScroll", "SideScroll.Start.Avalonia");
			return 1;
		}
	}

	public static AppBuilder BuildAvaloniaApp()
		=> AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.WithInterFont()
			.LogToTrace();
}
