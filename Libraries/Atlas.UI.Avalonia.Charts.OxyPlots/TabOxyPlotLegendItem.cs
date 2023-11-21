using Avalonia.Media;
using OxyPlot;

namespace Atlas.UI.Avalonia.Charts.OxyPlots;

public class TabOxyPlotLegendItem : TabChartLegendItem<OxyPlotLineSeries>
{
	public readonly TabControlOxyPlotLegend OxyLegend;

	private MarkerType _originalMarkerType;

	public List<DataPoint>? Points { get; internal set; }

	public override string ToString() => Series.ToString() ?? GetType().ToString();

	public TabOxyPlotLegendItem(TabControlOxyPlotLegend legend, ChartSeries<OxyPlotLineSeries> chartSeries) :
		base(legend, chartSeries)
	{
		OxyLegend = legend;
		_originalMarkerType = chartSeries.LineSeries.MarkerType;
	}

	public override void UpdateVisible()
	{
		var lineSeries = ChartSeries.LineSeries;
		
		if (IsSelected || _highlight)
		{
			if (Points != null)
			{
				lineSeries.Points.Clear();
				lineSeries.Points.AddRange(Points);
			}
			lineSeries.LineStyle = LineStyle.Solid;
			lineSeries.MarkerType = _originalMarkerType;
			lineSeries.Selectable = true;
		}
		else
		{
			if (lineSeries.Points.Count > 0)
			{
				Points = new List<DataPoint>(lineSeries.Points);
			}
			lineSeries.Points.Clear();
			lineSeries.LineStyle = LineStyle.None;
			lineSeries.MarkerType = MarkerType.None;
			lineSeries.Selectable = false;
			lineSeries.Unselect();
		}
	}

	public override void UpdateColor(Color color)
	{
		if (Series is OxyPlot.Series.LineSeries lineSeries)
		{
			var newColor = OxyColor.FromArgb(color.A, color.R, color.G, color.B);
			lineSeries.MarkerFill = newColor;
			lineSeries.Color = newColor;
		}
	}
}
