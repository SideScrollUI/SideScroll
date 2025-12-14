using Avalonia;
using Avalonia.Browser;
using SideScroll.Avalonia.Samples;

namespace SideScroll.Demo.Avalonia.Browser;

internal sealed class Program
{
    private static Task Main(string[] args) => BuildAvaloniaApp()
            .WithInterFont()
            .StartBrowserAppAsync("out");

    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}
