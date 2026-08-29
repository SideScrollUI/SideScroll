namespace SideScroll;

/// <summary>
/// Represents a property binding configuration for controlling UI element properties
/// </summary>
public class PropertyBinding(string path, object? obj)
{
	/// <summary>
	/// Gets the property path to bind to
	/// </summary>
	public string Path => path;

	/// <summary>
	/// Gets the object containing the property to bind to
	/// </summary>
	public object? Object => obj;
}
