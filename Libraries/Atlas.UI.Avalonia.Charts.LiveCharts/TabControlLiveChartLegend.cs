using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.Avalonia;

namespace Atlas.UI.Avalonia.Charts.LiveCharts;

public class TabControlLiveChartLegend : TabControlChartLegend<ISeries>
{
	public TabControlLiveChart LiveChart;
	public CartesianChart Chart => LiveChart.Chart;

	public TabControlLiveChartLegend(TabControlLiveChart liveChart) : base(liveChart)
	{
		LiveChart = liveChart;
	}

	public override TabChartLegendItem<ISeries> AddSeries(ChartSeries<ISeries> chartSeries)
	{
		var legendItem = new TabLiveChartLegendItem(this, chartSeries);
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

		if (chartSeries.LineSeries.Name is string name)
		{
			_idxLegendItems.Add(name, legendItem);
		}
		return legendItem;
	}

	public override void UpdateHighlight(bool showFaded)
	{
		base.UpdateHighlight(showFaded);

		LiveChart.UpdateAxis();
		LiveChart.InvalidateChart();
	}
}

