using Avalonia.Media;

namespace Atlas.Extensions;

public static class AvaloniaExtensions
{
	public static Color AsAvaloniaColor(this System.Drawing.Color color)
	{
		return new Color(color.A, color.R, color.G, color.B);
	}
}
