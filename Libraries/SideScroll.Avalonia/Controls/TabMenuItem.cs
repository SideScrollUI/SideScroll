using Avalonia.Controls;

namespace SideScroll.Avalonia.Controls;

/// <summary>
/// A styled menu item
/// </summary>
public class TabMenuItem : MenuItem
{
	protected override Type StyleKeyOverride => typeof(MenuItem);

	public TabMenuItem(string? header = null)
	{
		Header = header;
		ItemsSource = null; // Clear to avoid weak event handler leaks (this has reportedly been fixed by Avalonia but not confirmed)
	}
}
