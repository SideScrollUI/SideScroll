using Atlas.Core;

namespace Atlas.Tabs.Test.Chart;

[ListItem]
public class TabTestChart
{
	public TabTestChartList List => new();
	public TabTestChartSplit Split => new();
	public TabTestChartOverlay Overlay => new();
	public TabTestChartTimeRangeValue TimeRange => new();
}
