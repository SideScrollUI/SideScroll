using SideScroll.Attributes;

namespace SideScroll.Tabs.Samples.Charts;

[ListItem]
public class TabSampleCharts
{
	public static TabSampleChartLists Lists => new();
	public static TabSampleChartProperties Properties => new();
	public static TabSampleChartDimensions Dimensions => new();
	public static TabSampleChartTimeSeries TimeSeries => new();
	public static TabSampleChartTimeSeriesFractional Fractional => new();
	public static TabSampleChartSeriesCount SeriesCount => new();
	public static TabSampleChartNoData NoData => new();
	public static TabProcessMonitor ProcessMonitor => new();
}
