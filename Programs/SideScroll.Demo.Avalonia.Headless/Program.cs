using Avalonia;
using Avalonia.Headless;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using SideScroll.Avalonia.Samples;

namespace SideScroll.Demo.Avalonia.Headless;

internal static class Program
{
	private static void Main(string[] args)
	{
		AppBuilder.Configure<App>()
			.UseSkia()
			.UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
			.SetupWithoutStarting();

		SampleMainWindow window = new();
		window.Show();
		Dispatcher.UIThread.RunJobs();

		WriteableBitmap? bitmap = window.CaptureRenderedFrame();

		string outputPath = Path.Combine(AppContext.BaseDirectory, "output.png");
		bitmap?.Save(outputPath);

		Console.WriteLine($"Saved bitmap to: {outputPath}");
	}
}
