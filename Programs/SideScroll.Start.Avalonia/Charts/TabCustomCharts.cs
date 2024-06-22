using SideScroll;
using SideScroll.Tabs.Samples.Chart;

namespace SideScroll.Start.Avalonia.Charts;

[ListItem]
public class TabCustomCharts
{
	public static TabDashboard Dashboard => new();
	public static TabChartSizes Sizes => new();
	public static TabChartUpdating Updating => new();
	public static TabSampleCharts Test => new();
}
