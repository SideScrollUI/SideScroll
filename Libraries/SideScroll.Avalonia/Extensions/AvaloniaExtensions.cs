using Avalonia.Media;

namespace SideScroll.Avalonia.Extensions;

/// <summary>
/// Extension methods for Avalonia UI framework
/// </summary>
public static class AvaloniaExtensions
{
	/// <summary>
	/// Converts a System.Drawing.Color to an Avalonia Color
	/// </summary>
	public static Color AsAvaloniaColor(this System.Drawing.Color color)
	{
		return new Color(color.A, color.R, color.G, color.B);
	}

	/// <summary>
	/// Converts an Avalonia Color to a System.Drawing.Color
	/// </summary>
	public static System.Drawing.Color AsSystemColor(this Color color)
	{
		return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
	}

	/// <summary>
	/// Creates a new Color with the specified alpha transparency value
	/// </summary>
	public static Color WithAlpha(this Color color, byte alpha)
	{
		return new Color(alpha, color.R, color.G, color.B);
	}

	/// <summary>
	/// Applies a configuration action to an object and returns the object (fluent API pattern)
	/// </summary>
	public static T Also<T>(this T obj, Action<T> configure)
	{
		configure(obj);
		return obj;
	}
}
