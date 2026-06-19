using Avalonia;
using Avalonia.Headless;
using Avalonia.Media.Imaging;
using SideScroll.Avalonia.Controls.Viewer;
using SideScroll.Avalonia.Samples;
using SideScroll.Avalonia.Samples.Tabs;
using SideScroll.Tabs;

namespace SideScroll.Demo.Avalonia.Headless;

internal static class Program
{
	private static void Main(string[] args)
	{
		AppBuilder.Configure<App>()
			.UseSkia()
			.UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
			.SetupWithoutStarting();

		var project = Project.Load(SampleProjectSettings.Default);

		Bitmap bitmap = AvaloniaHeadlessCapture.RenderAndCrop(
			project,
			tab: new TabAvaloniaSamples(),
			captureFrame: window => window.CaptureRenderedFrame(),
			minTabDepth: 1,
			maxTabDepth: 3,
			maxWidth: 4000,
			maxHeight: 1000);

		string outputPath = Path.Combine(AppContext.BaseDirectory, "output.png");
		bitmap.Save(outputPath);

		Console.WriteLine($"Saved bitmap to: {outputPath}");
	}
}
