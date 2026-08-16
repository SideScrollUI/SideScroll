using NUnit.Framework;
using SideScroll.Tabs.Bookmarks.Models;
using SideScroll.Tabs.Settings;

namespace SideScroll.Tabs.Tests;

/// <summary>
/// An imported bookmark is json a caller supplies, so it can name any member of the types it
/// deserializes into. The members a bookmark deliberately leaves out of its own export are the ones
/// an import must not be able to set
/// </summary>
[Category("Tabs")]
public class BookmarkImportRestrictionTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("BookmarkImportRestrictions");
	}

	private const string CraftedJson = """
		{
		  "Name": "crafted",
		  "Changed": "INJECTED",
		  "TabBookmark": {
		    "IsRoot": true,
		    "TabDatas": [ { "SelectionType": 1 } ]
		  }
		}
		""";

	[Test, Description(
		"TabBookmark is [PublicData], so its [PrivateData] members rely entirely on the member " +
		"level rule, which only applied while writing")]
	public void ImportedBookmarkCannotSetPrivateMembers()
	{
		Assert.That(Bookmark.TryParseJson(CraftedJson, out Bookmark? bookmark), Is.True);

		Assert.That(bookmark!.Changed, Is.Null);
		Assert.That(bookmark.TabBookmark.IsRoot, Is.False);
		Assert.That(bookmark.TabBookmark.TabDatas.First().SelectionType, Is.EqualTo(SelectionType.None));
	}

	[Test, Description("The members a bookmark does export still import")]
	public void ImportedBookmarkKeepsPublicMembers()
	{
		Assert.That(Bookmark.TryParseJson(CraftedJson, out Bookmark? bookmark), Is.True);

		Assert.That(bookmark!.Name, Is.EqualTo("crafted"));
		Assert.That(bookmark.TabBookmark.TabDatas, Has.Count.EqualTo(1));
	}

	[Test, Description("A bookmark's own export never carries the private members, so nothing is lost")]
	public void ExportedBookmarkOmitsPrivateMembers()
	{
		var bookmark = new Bookmark { Name = "real", Changed = "description" };
		bookmark.TabBookmark.IsRoot = true;

		string json = bookmark.ToJson();

		Assert.That(json, Does.Contain("real"));
		Assert.That(json, Does.Not.Contain("description"));
		Assert.That(json, Does.Not.Contain("IsRoot"));
	}

	[Test, Description("Importing a bookmark with a cycle does not throw StackOverflowException")]
	public void ImportedBookmarkHandlesCycles()
	{
		TabBookmark root = new TabBookmark();
		TabDataBookmark data = new TabDataBookmark();
		root.TabDatas.Add(data);
		
		SelectedRowView rowView = new SelectedRowView("Row") { TabBookmark = root };
		data.SelectedRows.Add(rowView);

		var project = new Project();
		
		// This will throw StackOverflowException without cycle detection
		Assert.DoesNotThrow(() => root.Import(project));
	}

	[Test, Description(
		"Cycle detection tracks the bookmarks already visited, not the ones that compare equal to " +
		"one. Two distinct but equal siblings are both real subtrees and both have to be imported")]
	public void ImportedBookmarkKeepsEqualSiblings()
	{
		TabBookmark root = new();
		TabDataBookmark first = new() { DataRepoGroupId = "group" };
		TabDataBookmark second = new() { DataRepoGroupId = "group" };
		root.TabDatas.Add(first);
		root.TabDatas.Add(second);

		root.Import(new Project());

		// Import() marks each one it reaches, so a sibling skipped as a duplicate stays unset
		Assert.That(first.SelectionType, Is.EqualTo(SelectionType.Link));
		Assert.That(second.SelectionType, Is.EqualTo(SelectionType.Link),
			"The second was skipped as though it were the first");
	}
}
