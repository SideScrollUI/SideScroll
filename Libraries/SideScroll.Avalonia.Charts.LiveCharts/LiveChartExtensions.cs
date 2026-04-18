using Avalonia.Media;
using SkiaSharp;

namespace SideScroll.Avalonia.Charts.LiveCharts;

/// <summary>
/// Extension methods for converting Avalonia color types to SkiaSharp equivalents used by LiveCharts.
/// </summary>
public static class LiveChartExtensions
{
	/// <summary>Converts an Avalonia <see cref="Color"/> to a SkiaSharp <see cref="SKColor"/>.</summary>
	public static SKColor AsSkColor(this Color color)
	{
		return new SKColor(color.R, color.G, color.B, color.A);
	}
}
