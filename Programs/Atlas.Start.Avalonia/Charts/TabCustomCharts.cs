using Atlas.Core;
using Atlas.Tabs.Samples.Chart;

namespace Atlas.Start.Avalonia.Charts;

[ListItem]
public class TabCustomCharts
{
	public static TabDashboard Dashboard => new();
	public static TabChartSizes Sizes => new();
	public static TabChartUpdating Updating => new();
	public static TabSampleCharts Test => new();
}
