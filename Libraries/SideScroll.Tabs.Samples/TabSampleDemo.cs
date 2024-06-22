using SideScroll.Core;
using SideScroll.Resources;
using SideScroll.Tabs.Samples.Chart;
using SideScroll.Tabs.Samples.DataGrid;
using SideScroll.Tabs.Samples.Objects;
using SideScroll.Tabs.Samples.Params;

namespace SideScroll.Tabs.Samples;

[ListItem, PublicData]
public class TabSampleDemo
{
	public override string ToString() => "Demo";

	public static TabSampleGridCollectionSize Collections => new();
	public static TabSampleObjectMembers Objects => new();
	public static TabSampleParamsDataTabs Params => new();
	public static string Json => TextSamples.Json;
	public static TabSampleChartTimeSeries Chart => new();
	public static TabSampleDemo Copy => new();
}
