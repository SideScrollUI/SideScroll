using Atlas.Core;

namespace Atlas.Tabs.Test.Chart;

[ListItem]
public class TabTestChart
{
	public static TabTestChartLists Lists => new();
	public static TabTestChartProperties Properties => new();
	public static TabTestChartDimensions Dimensions => new();
	public static TabTestChartTimeRangeValue TimeRange => new();
	public static TabTestChartNoData NoData => new();
}
