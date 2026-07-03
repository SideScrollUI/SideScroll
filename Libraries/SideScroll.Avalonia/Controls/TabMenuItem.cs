using Avalonia.Controls;

namespace SideScroll.Avalonia.Controls;

/// <summary>
/// A styled menu item
/// </summary>
public class TabMenuItem : MenuItem
{
	/// <inheritdoc/>
	protected override Type StyleKeyOverride => typeof(MenuItem);

	/// <summary>Initializes the menu item with an optional header text.</summary>
	public TabMenuItem(string? header = null)
	{
		Header = header;
		ItemsSource = null; // Clear to avoid weak event handler leaks (this has reportedly been fixed by Avalonia but not confirmed)
	}
}
