using NUnit.Framework;
using SideScroll.Tabs.Bookmarks.Models;
using SideScroll.Tabs.Settings;

namespace SideScroll.Tabs.Tests;

[Category("Tabs")]
public class TabDataBookmarkTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("TabDataBookmark");
	}

	// What TabDataGrid does when a user drags a column, it reorders the list in place
	private static void ReorderColumns(TabDataSettings dataSettings, params string[] columnNames)
	{
		dataSettings.ColumnNameOrder.Clear();
		dataSettings.ColumnNameOrder.AddRange(columnNames);
	}

	[Test, Description(
		"The settings handed back have to own their column order. Sharing the list means reordering " +
		"a column rewrites the column order already captured in the bookmark.")]
	public void ToDataSettings_ColumnNameOrder_IsNotSharedWithTheBookmark()
	{
		var dataBookmark = new TabDataBookmark
		{
			ColumnNameOrder = ["A", "B"],
		};

		TabDataSettings dataSettings = dataBookmark.ToDataSettings();
		Assert.That(dataSettings.ColumnNameOrder, Is.EqualTo(new[] { "A", "B" }), "Copied, not dropped.");

		ReorderColumns(dataSettings, "B", "A");

		Assert.That(dataBookmark.ColumnNameOrder, Is.EqualTo(new[] { "A", "B" }),
			"The bookmark keeps the order it captured.");
	}

	[Test, Description(
		"The same through the path a bookmark actually takes when it's navigated to. Bookmarks stay " +
		"in the navigator history, so mutating one changes what going back restores.")]
	public void ToViewSettings_ColumnReorder_DoesNotChangeTheBookmark()
	{
		var tabBookmark = new TabBookmark();
		tabBookmark.TabDatas.Add(new TabDataBookmark { ColumnNameOrder = ["A", "B"] });

		TabViewSettings viewSettings = tabBookmark.ToViewSettings();

		ReorderColumns(viewSettings.TabDataSettings[0], "B", "A");

		Assert.That(tabBookmark.TabDatas[0].ColumnNameOrder, Is.EqualTo(new[] { "A", "B" }));
	}

	[Test, Description("Two tabs opened from one bookmark can't reorder each other's columns")]
	public void ToDataSettings_CalledTwice_ReturnsIndependentLists()
	{
		var dataBookmark = new TabDataBookmark { ColumnNameOrder = ["A", "B"] };

		TabDataSettings first = dataBookmark.ToDataSettings();
		TabDataSettings second = dataBookmark.ToDataSettings();

		ReorderColumns(first, "B", "A");

		Assert.That(second.ColumnNameOrder, Is.EqualTo(new[] { "A", "B" }));
	}

	[Test, Description("The rest of the conversion still carries across")]
	public void ToDataSettings_CopiesTheOtherSettings()
	{
		var dataBookmark = new TabDataBookmark
		{
			ColumnNameOrder = ["A"],
			Filter = "search",
			SelectionType = SelectionType.User,
		};
		dataBookmark.SelectedRows.Add(new SelectedRowView(new SelectedRow { Label = "row" }));

		TabDataSettings dataSettings = dataBookmark.ToDataSettings();

		Assert.That(dataSettings.Filter, Is.EqualTo("search"));
		Assert.That(dataSettings.SelectionType, Is.EqualTo(SelectionType.User));
		Assert.That(dataSettings.SelectedRows.Single().Label, Is.EqualTo("row"));
	}
}
