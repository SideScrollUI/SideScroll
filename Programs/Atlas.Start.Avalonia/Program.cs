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

			// Fix borrowed from https://github.com/zkSNACKs/WalletWasabi/commit/02338667e0c2fd577bbcdb7c575a29c9b0cc4d01
			// TODO remove this overriding of RenderTimer when Avalonia 0.9 is released.
			// fixes "Thread Leak" issue in 0.8.1 Avalonia.
			var old = builder.WindowingSubsystemInitializer;

			builder.UseWindowingSubsystem(() =>
			{
				old();

				AvaloniaLocator.CurrentMutable.Bind<IRenderTimer>().ToConstant(new DefaultRenderTimer(60));
			});

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
				//.UseDataGrid()
				//.BeforeStarting(_ => OxyPlotModule.Initialize())
				.LogToDebug();
	}
}
