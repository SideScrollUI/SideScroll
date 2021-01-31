using Avalonia;
using OxyPlot.Avalonia;

namespace Atlas.Start.Avalonia
{
	class Program
	{
		static int Main(string[] args)
		{
			OxyPlotModule.EnsureLoaded();
			AppBuilder builder = BuildAvaloniaApp();

			return builder.StartWithClassicDesktopLifetime(args);
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
}
