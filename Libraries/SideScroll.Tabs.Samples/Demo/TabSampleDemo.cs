using SideScroll.Attributes;
using SideScroll.Resources;
using SideScroll.Tabs.Samples.Chart;
using SideScroll.Tabs.Samples.DataGrid;
using SideScroll.Tabs.Samples.Objects;
using SideScroll.Tabs.Samples.Params;
using SideScroll.Tabs.Tools.FileViewer;

namespace SideScroll.Tabs.Samples.Demo;

[ListItem, PublicData]
public class TabSampleDemo
{
	public override string ToString() => "Demo";

	public static TabDemoPlanets Planets => new();
	public static TabSampleGridCollectionSize Collections => new();
	public static TabSampleObjectMembers Objects => new();
	public static TabSampleParamsDataTabs Params => new();
	public static string Json => TextSamples.Json;
	public static TabSampleChartTimeSeries Chart => new();
	public static TabFileViewer FileViewer => new();
	public static TabSampleDemo Copy => new();
}
