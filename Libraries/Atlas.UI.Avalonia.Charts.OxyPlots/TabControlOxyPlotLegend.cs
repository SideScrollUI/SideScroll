using Atlas.Core;
using OxyPlot.Avalonia;

namespace Atlas.UI.Avalonia.Charts.OxyPlots;

public class TabControlOxyPlotLegend : TabControlChartLegend<OxyPlotLineSeries>
{
	public TabControlOxyPlot OxyChart;
	public PlotView PlotView => OxyChart.PlotView;

	public override string? ToString() => ChartView.ToString();

	public TabControlOxyPlotLegend(TabControlOxyPlot oxyChart) : base(oxyChart)
	{
		OxyChart = oxyChart;
	}

	public override TabChartLegendItem<OxyPlotLineSeries> AddSeries(ChartSeries<OxyPlotLineSeries> chartSeries)
	{
		OxyPlot.Series.Series series = chartSeries.LineSeries;

		var legendItem = new TabOxyPlotLegendItem(this, chartSeries);
		legendItem.OnSelectionChanged += LegendItem_SelectionChanged;
		legendItem.OnVisibleChanged += LegendItem_VisibleChanged;
		legendItem.PointerPressed += (s, e) =>
		{
			if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
			{
				SelectLegendItem(legendItem);
			}
		};
		LegendItems.Add(legendItem);
		if (series.Title != null)
		{
			_idxLegendItems.Add(series.Title, legendItem);
		}
		return legendItem;
	}

	public override void UpdateVisibleSeries()
	{
		if (PlotView.Model == null)
			return;

		base.UpdateVisibleSeries();

		// Update axis for new visible
		OxyChart.InvalidateChart();
	}

	public override void UpdateHighlight(bool showFaded)
	{
		base.UpdateHighlight(showFaded);

		OxyChart.InvalidateChart();
	}
}
