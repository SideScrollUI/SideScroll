using Avalonia.Media;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.Painting;

namespace SideScroll.Avalonia.Charts.LiveCharts;

/// <summary>
/// A legend item for <see cref="TabLiveChart"/> that updates the underlying <see cref="LiveChartLineSeries"/>
/// stroke and fill paints when the color or visibility changes.
/// </summary>
public class TabLiveChartLegendItem(TabLiveChartLegend legend, ChartSeries<ISeries> chartSeries)
	: TabChartLegendItem<ISeries>(legend, chartSeries)
{
	/// <summary>Gets the parent LiveCharts legend panel.</summary>
	public TabLiveChartLegend LiveChartLegend => legend;

	/// <summary>Updates the <see cref="LiveChartLineSeries"/> stroke and geometry fill to the given color.</summary>
	public override void UpdateColor(Color color)
	{
		if (ChartSeries.LineSeries is LiveChartLineSeries lineSeries)
		{
			var skColor = color.AsSkColor();
			if (lineSeries.Stroke is SolidColorPaint paint && paint.Color == skColor) return;

			lineSeries.Stroke = new SolidColorPaint(skColor) { StrokeThickness = 2 };
			/*if (lineSeries.GeometryStroke != null)
			{
				lineSeries.GeometryStroke = new SolidColorPaint(skColor) { StrokeThickness = 2 };
			}*/
			if (lineSeries.GeometryFill != null)
			{
				lineSeries.GeometryFill = new SolidColorPaint(skColor);
			}

			var chart = LiveChartLegend.Chart.CoreChart;

			lock (LiveChartLegend.Chart.CoreCanvas.Sync)
			{
				lineSeries.RemoveOldPaints(chart.View);
			}
		}
	}

	/// <summary>Syncs the <see cref="LiveChartLineSeries"/> visibility with the current selected/highlight state.</summary>
	public override void UpdateVisible()
	{
		bool isVisible = IsSelected || Highlight;
		if (isVisible != ChartSeries.LineSeries.IsVisible)
		{
			ChartSeries.LineSeries.IsVisible = isVisible;
		}
	}
}
