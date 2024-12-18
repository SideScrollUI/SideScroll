using Avalonia.Media;
using SkiaSharp;

namespace SideScroll.Avalonia.Charts.LiveCharts;

public static class LiveChartExtensions
{
	public static SKColor AsSkColor(this Color color)
	{
		return new SKColor(color.R, color.G, color.B, color.A);
	}
}
