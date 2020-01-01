using System;
using Atlas.Tabs;
using Avalonia;
using Avalonia.Logging.Serilog;
using Avalonia.Rendering;
using OxyPlot.Avalonia;

namespace Atlas.Start.Avalonia
{
	class Program
	{
		static int Main(string[] args)
		{
			OxyPlotModule.EnsureLoaded();
			AppBuilder builder = BuildAvaloniaApp(args);

			return builder.StartWithClassicDesktopLifetime(args);
		}

		public static AppBuilder BuildAvaloniaApp(string[] args)
			=> AppBuilder.Configure<App>()
				.UsePlatformDetect()
				.With(new Win32PlatformOptions
				{
					UseDeferredRendering = false,
					AllowEglInitialization = true,
				})
				.LogToDebug();
	}
}
