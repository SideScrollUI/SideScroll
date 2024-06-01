using Atlas.Core;
using Atlas.Tabs.Samples.Actions;
using Atlas.Tabs.Samples.Chart;
using Atlas.Tabs.Samples.DataGrid;
using Atlas.Tabs.Samples.DataRepo;
using Atlas.Tabs.Samples.Loading;
using Atlas.Tabs.Samples.Objects;
using Atlas.Tabs.Samples.Params;
using Atlas.Tabs.Tools;

namespace Atlas.Tabs.Samples;

[ListItem]
public class TabSamples
{
	public static TabSampleDemo Demo => new();

	public static TabSampleObjects Objects => new();
	public static TabSampleDataGrid DataGrid => new();
	public static TabSampleParams Params => new();
	public static TabSampleToolbar Toolbar => new();
	public static TabSampleActions Actions => new();
	public static TabSampleLoading Loading => new();
	public static TabSampleLog Log => new();

	public static TabSampleTextEditor TextEditor => new();
	public static TabSampleCharts Chart => new();

	public static TabSampleSerializer Serializer => new();
	public static TabSampleDataRepo DataRepos => new();
	public static TabSampleProcess Process => new();
	public static TabSampleBookmarks Bookmarks => new();
	public static TabIcons Icons => new();
	public static TabTools Tools => new();
}
