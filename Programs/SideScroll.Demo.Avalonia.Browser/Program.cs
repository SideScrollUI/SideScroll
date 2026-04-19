using Avalonia;
using Avalonia.Browser;

namespace SideScroll.Demo.Avalonia.Browser;

internal static class Program
{
	private static Task Main(string[] args) => BuildAvaloniaApp()
			.WithInterFont()
			.StartBrowserAppAsync("out");

	private static AppBuilder BuildAvaloniaApp()
		=> AppBuilder.Configure<BrowserApp>();
}
