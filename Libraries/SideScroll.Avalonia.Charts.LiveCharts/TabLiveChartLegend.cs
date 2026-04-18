using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.Avalonia;

namespace SideScroll.Avalonia.Charts.LiveCharts;

/// <summary>
/// The LiveCharts-specific chart legend. Overrides series addition to wire up
/// <see cref="TabLiveChartLegendItem"/> controls and propagates highlight changes back to the chart axes.
/// </summary>
public class TabLiveChartLegend(TabLiveChart liveChart)
	: TabChartLegend<ISeries>(liveChart)
{
	/// <summary>Gets the parent <see cref="TabLiveChart"/> this legend belongs to.</summary>
	public TabLiveChart LiveChart => liveChart;
	/// <summary>Gets the underlying LiveCharts <see cref="CartesianChart"/> control.</summary>
	public CartesianChart Chart => LiveChart.Chart;

	protected override TabChartLegendItem<ISeries> AddSeries(ChartSeries<ISeries> chartSeries)
	{
		var legendItem = new TabLiveChartLegendItem(this, chartSeries);
		legendItem.OnVisibilityChanged += LegendItem_VisibilityChanged;
		legendItem.PointerPressed += (_, e) =>
		{
			if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
			{
				SelectLegendItem(legendItem);
			}
		};
		LegendItems.Add(legendItem);

		if (chartSeries.LineSeries.Name is string name)
		{
			IdxLegendItems.Add(name, legendItem);
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

