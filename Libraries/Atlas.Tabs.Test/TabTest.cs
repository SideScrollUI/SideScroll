using Atlas.Core;
using Atlas.Tabs.Test.Actions;
using Atlas.Tabs.Test.Chart;
using Atlas.Tabs.Test.DataGrid;
using Atlas.Tabs.Test.DataRepo;
using Atlas.Tabs.Test.Exceptions;
using Atlas.Tabs.Test.Loading;
using Atlas.Tabs.Test.Objects;
using Atlas.Tabs.Test.Params;
using Atlas.Tabs.Tools;

namespace Atlas.Tabs.Test;

[ListItem]
public class TabTest
{
	public static TabSample Sample => new();

	public static TabTestObjects Objects => new();
	public static TabTestDataGrid DataGrid => new();
	public static TabTestParams Params => new();
	public static TabTestLog Log => new();
	public static TabActions Actions => new();
	public static TabTestJson Json => new();
	public static TabTestBookmarks Bookmarks => new();
	public static TabTestSkip Skip => new(); // move into new Lists?
	public static TabTestExceptions Exceptions => new();

	public static TabTestBrowser WebBrowser => new();
	public static TabTestTextEditor TextEditor => new();
	public static TabTestChart Chart => new();
	public static TabTestToolbar Toolbar => new();
	public static TabSerializer Serializer => new();
	public static TabTestProcess Process => new();

	public static TabTestLoading Loading => new();
	public static TabTestDataRepo DataRepos => new();
	public static TabIcons Icons => new();
	public static TabTools Tools => new();
}
