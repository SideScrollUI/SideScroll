using Avalonia.Media;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Painting;

namespace Atlas.UI.Avalonia.Charts.LiveCharts;

public class TabLiveChartLegendItem : TabChartLegendItem<ISeries>
{
	public readonly TabControlLiveChartLegend LiveChartLegend;
	public TabLiveChartLegendItem(TabControlLiveChartLegend legend, ChartSeries<ISeries> chartSeries) : 
		base(legend, chartSeries)
	{
		LiveChartLegend = legend;
	}

	public override void UpdateColor(Color color)
	{
		if (ChartSeries.LineSeries is LiveChartLineSeries lineSeries)
		{
			var skColor = color.AsSkColor();

			lineSeries.Stroke = new SolidColorPaint(skColor) { StrokeThickness = 2 };
			/*if (lineSeries.GeometryStroke != null)
			{
				lineSeries.GeometryStroke = new SolidColorPaint(skColor) { StrokeThickness = 2 };
			}*/
			if (lineSeries.GeometryFill != null)
			{
				lineSeries.GeometryFill = new SolidColorPaint(skColor);
			}
		}
	}

	public override void UpdateVisible()
	{
		bool isVisible = IsSelected || Highlight;
		if (isVisible != ChartSeries.LineSeries.IsVisible)
		{
			ChartSeries.LineSeries.IsVisible = isVisible;
			if (!isVisible)
			{
				if (ChartSeries.LineSeries is LiveChartLineSeries lineSeries)
				{
					// Workaround for LiveCharts not always removing invisible series from the UI
					var chart = (CartesianChart<SkiaSharpDrawingContext>)LiveChartLegend.Chart.CoreChart;
					lineSeries.RemoveFromUI(chart); // Not sure whether to keep this in, it works without it
					lineSeries.RemoveOldPaints(chart.View); // Sometimes required
				}
			}
		}
	}
}
