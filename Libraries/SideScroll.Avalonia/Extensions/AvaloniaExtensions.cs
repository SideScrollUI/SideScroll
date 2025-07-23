using Avalonia.Media;

namespace SideScroll.Avalonia.Extensions;

public static class AvaloniaExtensions
{
	public static Color AsAvaloniaColor(this System.Drawing.Color color)
	{
		return new Color(color.A, color.R, color.G, color.B);
	}

	public static System.Drawing.Color AsSystemColor(this Color color)
	{
		return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
	}

	public static Color WithAlpha(this Color color, byte alpha)
	{
		return new Color(alpha, color.R, color.G, color.B);
	}

	public static T Also<T>(this T obj, Action<T> configure)
	{
		configure(obj);
		return obj;
	}
}
