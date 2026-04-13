using Avalonia.Controls;
using SideScroll.Avalonia.Samples;

namespace SideScroll.Demo.Avalonia.Browser;

public class BrowserApp : App
{
	protected override Control CreateSingleView() => new BrowserMainView();
}
