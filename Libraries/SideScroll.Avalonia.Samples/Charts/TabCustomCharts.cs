using SideScroll.Attributes;
using SideScroll.Tabs.Samples.Charts;

namespace SideScroll.Avalonia.Samples.Charts;

[ListItem]
public class TabCustomCharts
{
	public static TabDashboard Dashboard => new();
	public static TabChartSizes Sizes => new();
	public static TabChartUpdating Updating => new();
	public static TabSampleCharts Samples => new();
}
