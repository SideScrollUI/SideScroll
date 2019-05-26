using System;
using Avalonia;
using Avalonia.Logging.Serilog;
using OxyPlot.Avalonia;

namespace Atlas.Start.Avalonia
{
	class Program
	{
		static void Main(string[] args)
		{
			OxyPlotModule.EnsureLoaded();
			AppBuilder builder = AppBuilder.Configure<App>().UsePlatformDetect().UseDataGrid().LogToDebug();

			builder.BeforeStarting(_ => OxyPlotModule.Initialize());

			builder.Start<MainWindow>();
			//builder.Start<MainWindow>(mainWindow);
		}

		// Not currently used
		public static AppBuilder BuildAvaloniaApp(string[] args)
			=> AppBuilder.Configure<App>()
				.UsePlatformDetect()
				.UseDataGrid()
				.BeforeStarting(_ => OxyPlotModule.Initialize())
				.LogToDebug();
	}
}
