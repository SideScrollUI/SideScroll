using Avalonia.Media;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.Painting;

namespace SideScroll.Avalonia.Charts.LiveCharts;

public class TabLiveChartLegendItem(TabLiveChartLegend legend, ChartSeries<ISeries> chartSeries)
	: TabChartLegendItem<ISeries>(legend, chartSeries)
{
	public TabLiveChartLegend LiveChartLegend => legend;

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

	public override void UpdateVisible()
	{
		bool isVisible = IsSelected || Highlight;
		if (isVisible != ChartSeries.LineSeries.IsVisible)
		{
			ChartSeries.LineSeries.IsVisible = isVisible;
		}
	}
}
