using Avalonia.Media;

namespace SideScroll.Extensions;

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
}
