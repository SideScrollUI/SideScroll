using Atlas.Core;

namespace Atlas.Tabs.Samples.Chart;

[ListItem]
public class TabSampleCharts
{
	public static TabSampleChartLists Lists => new();
	public static TabSampleChartProperties Properties => new();
	public static TabSampleChartDimensions Dimensions => new();
	public static TabSampleChartTimeSeries TimeSeries => new();
	public static TabSampleChartNoData NoData => new();
}
