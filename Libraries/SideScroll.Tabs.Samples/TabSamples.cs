using SideScroll.Attributes;
using SideScroll.Tabs.Samples.Actions;
using SideScroll.Tabs.Samples.Chart;
using SideScroll.Tabs.Samples.DataGrid;
using SideScroll.Tabs.Samples.DataRepo;
using SideScroll.Tabs.Samples.Demo;
using SideScroll.Tabs.Samples.Loading;
using SideScroll.Tabs.Samples.Objects;
using SideScroll.Tabs.Samples.Forms;
using SideScroll.Tabs.Tools;

namespace SideScroll.Tabs.Samples;

[ListItem]
public class TabSamples
{
	public static TabSampleDemo Demo => new();

	public static TabSampleObjects Objects => new();
	public static TabSampleDataGrid DataGrid => new();
	public static TabSampleForms Forms => new();
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
