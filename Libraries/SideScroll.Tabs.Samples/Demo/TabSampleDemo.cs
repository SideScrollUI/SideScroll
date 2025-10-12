using SideScroll.Attributes;
using SideScroll.Resources;
using SideScroll.Tabs.Samples.Charts;
using SideScroll.Tabs.Samples.DataGrid;
using SideScroll.Tabs.Samples.Forms;
using SideScroll.Tabs.Samples.Forms.Todo;
using SideScroll.Tabs.Samples.Objects;
using SideScroll.Tabs.Tools.FileViewer;

namespace SideScroll.Tabs.Samples.Demo;

[ListItem, PublicData]
public class TabSampleDemo
{
	public override string ToString() => "Demo";

	public static TabDemoPlanets Planets => new();
	public static TabPlanetCharts Charts => new();
	public static TabSampleTodos Todo => new();
	public static TabSampleGridCollectionSize Collections => new();
	public static TabSampleObjectMembers Objects => new();
	public static TabSampleFormDataTabs Forms => new();
	public static string Json => TextSamples.Json;
	public static TabFileViewer FileViewer => new();
	public static TabProcessMonitor ProcessMonitor => new();
	public static TabSampleDemo Copy => new();
}
