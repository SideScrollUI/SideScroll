using Atlas.Core;

namespace Atlas.Tabs.Test.Chart;

[ListItem]
public class TabTestChart
{
	public static TabTestChartList List => new();
	public static TabTestChartSplit Split => new();
	public static TabTestChartOverlay Overlay => new();
	public static TabTestChartTimeRangeValue TimeRange => new();
}
