using Atlas.Core;
using Avalonia;
using Avalonia.Data.Core.Plugins;
using OxyPlot.Avalonia;

namespace Atlas.Start.Avalonia;

static class Program
{
	static int Main(string[] args)
	{
		OxyPlotModule.EnsureLoaded();
		AppBuilder builder = BuildAvaloniaApp();

		// Remove Default DataAnnotations Validators
		// These validators show before values are entered, which ends up showing too many initial warnings
		// https://docs.avaloniaui.net/docs/data-binding/data-validation
		// Add custom template?
		BindingPlugins.DataValidators.RemoveAt(0);

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

	public static AppBuilder BuildAvaloniaApp()
		=> AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.WithInterFont()
			.LogToTrace();
}
