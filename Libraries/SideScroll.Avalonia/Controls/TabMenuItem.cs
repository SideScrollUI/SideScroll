using Avalonia.Controls;

namespace SideScroll.Avalonia.Controls;

public class TabMenuItem : MenuItem
{
	protected override Type StyleKeyOverride => typeof(MenuItem);

	public TabMenuItem(string? header = null)
	{
		Header = header;
		ItemsSource = null; // Clear to avoid weak event handler leaks
	}
}
