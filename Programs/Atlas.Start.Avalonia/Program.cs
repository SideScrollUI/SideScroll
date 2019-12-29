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
		static void Main(string[] args)
		{
			OxyPlotModule.EnsureLoaded();
			AppBuilder builder = BuildAvaloniaApp(args);

			builder.Start<MainWindow>();
			// todo: switch to this in next version?
			//var project = LoadProject(UserSettings.DefaultProjectPath);
			//var mainWindow = new MainWindow(project);
			//builder.Start<MainWindow>(mainWindow);
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
