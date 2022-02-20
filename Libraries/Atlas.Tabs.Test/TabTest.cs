using Atlas.Core;
using Atlas.Tabs.Test.Actions;
using Atlas.Tabs.Test.Chart;
using Atlas.Tabs.Test.DataGrid;
using Atlas.Tabs.Test.DataRepo;
using Atlas.Tabs.Test.Exceptions;
using Atlas.Tabs.Test.Loading;
using Atlas.Tabs.Test.Objects;
using Atlas.Tabs.Tools;

namespace Atlas.Tabs.Test;

[ListItem]
public class TabTest
{
	public TabSample Sample => new();

	public TabTestObjects Objects => new();
	public TabTestDataGrid DataGrid => new();
	public TabTestParams Params => new();
	public TabTestLog Log => new();
	public TabActions Actions => new();
	public TabTestJson Json => new();
	public TabTestBookmarks Bookmarks => new();
	public TabTestSkip Skip => new(); // move into new Lists?
	public TabTestExceptions Exceptions => new();

	public TabTestBrowser WebBrowser => new();
	public TabTestTextEditor TextEditor => new();
	public TabTestChart Chart => new();
	public TabTestToolbar Toolbar => new();
	public TabSerializer Serializer => new();
	public TabTestProcess Process => new();

	public TabTestLoading Loading => new();
	public TabTestDataRepo DataRepos => new();
	public TabIcons Icons => new();
	public TabTools Tools => new();
}
