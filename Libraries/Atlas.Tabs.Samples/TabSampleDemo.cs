using Atlas.Core;
using Atlas.Resources;
using Atlas.Tabs.Samples.Chart;
using Atlas.Tabs.Samples.DataGrid;
using Atlas.Tabs.Samples.Objects;
using Atlas.Tabs.Samples.Params;

namespace Atlas.Tabs.Samples;

[ListItem, PublicData]
public class TabSampleDemo
{
	public override string ToString() => "Demo";

	public static TabSampleGridCollectionSize Collections => new();
	public static TabSampleChartTimeSeries Chart => new();
	public static TabSampleParamsDataTabs Params => new();
	public static string Json => TextSamples.Json;
	public static TabSampleObjectMembers Objects => new();
	public static TabSampleDemo Copy => new();
}
