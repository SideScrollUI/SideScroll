using Atlas.Core;
using Avalonia;
using OxyPlot.Avalonia;
using System;

namespace Atlas.Start.Avalonia
{
	class Program
	{
		static int Main(string[] args)
		{
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
