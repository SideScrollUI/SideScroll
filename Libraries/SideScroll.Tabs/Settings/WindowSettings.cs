namespace SideScroll.Tabs.Settings;

/// <summary>
/// Represents window state and position settings
/// </summary>
public class WindowSettings
{
	/// <summary>
	/// Gets or sets whether the window is maximized
	/// </summary>
	public bool Maximized { get; set; }

	/// <summary>
	/// Gets or sets the window's left position in pixels
	/// </summary>
	public double Left { get; set; }

	/// <summary>
	/// Gets or sets the window's top position in pixels
	/// </summary>
	public double Top { get; set; }

	/// <summary>
	/// Gets or sets the window width in pixels (default: 1600)
	/// </summary>
	public double Width { get; set; } = 1600;

	/// <summary>
	/// Gets or sets the window height in pixels (default: 1050)
	/// </summary>
	public double Height { get; set; } = 1050;
}
