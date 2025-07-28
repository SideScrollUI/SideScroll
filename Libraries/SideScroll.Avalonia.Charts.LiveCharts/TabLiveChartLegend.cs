using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.Avalonia;

namespace SideScroll.Avalonia.Charts.LiveCharts;

public class TabLiveChartLegend(TabLiveChart liveChart)
	: TabChartLegend<ISeries>(liveChart)
{
	public TabLiveChart LiveChart => liveChart;
	public CartesianChart Chart => LiveChart.Chart;

	protected override TabChartLegendItem<ISeries> AddSeries(ChartSeries<ISeries> chartSeries)
	{
		var legendItem = new TabLiveChartLegendItem(this, chartSeries);
		legendItem.OnVisibilityChanged += LegendItem_VisibilityChanged;
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

