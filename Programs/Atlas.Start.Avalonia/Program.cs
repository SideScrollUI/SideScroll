﻿using System;
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
			//AppBuilder builder = AppBuilder.Configure<App>().UsePlatformDetect();
			AppBuilder builder = AppBuilder.Configure<App>().UsePlatformDetect().UseSkia().LogToDebug();
			//AvaloniaLocator.Current.GetService<IGlobalStyles>().Styles.AddRange(new OxyPlot.Avalonia.Themes.Default());

			builder.BeforeStarting(_ => OxyPlotModule.Initialize());

			builder.Start<MainWindow>();
			//builder.Start<MainWindow>(mainWindow);
		}

		// Not currently used
		public static AppBuilder BuildAvaloniaApp(string[] args)
			=> AppBuilder.Configure<App>()
				.UsePlatformDetect()
				.BeforeStarting(_ => OxyPlotModule.Initialize())
				.LogToDebug();
	}
}
