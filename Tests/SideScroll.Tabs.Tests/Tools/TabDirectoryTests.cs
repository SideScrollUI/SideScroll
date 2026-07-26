using NUnit.Framework;
using SideScroll.Tabs.Bookmarks.Models;
using SideScroll.Tabs.Tools.FileViewer;

namespace SideScroll.Tabs.Tests;

[Category("Tabs")]
public class TabDirectoryTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("TabDirectory");
	}

	private static string Root => Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "Viewed"));

	private static SelectedRow RowWithKey(string? dataKey) => new()
	{
		Label = "display",
		DataKey = dataKey,
	};

	[Test, Description("A row inside the directory resolves to its full path")]
	public void GetSelectedPath_RowInsideDirectory_Resolves()
	{
		string expected = Path.Combine(Root, "file.txt");

		string? path = TabDirectory.GetSelectedPath(Root, RowWithKey(expected));

		Assert.That(path, Is.EqualTo(expected));
	}

	[Test, Description("Nested entries are still inside the directory")]
	public void GetSelectedPath_NestedRow_Resolves()
	{
		string expected = Path.Combine(Root, "sub", "file.txt");

		string? path = TabDirectory.GetSelectedPath(Root, RowWithKey(expected));

		Assert.That(path, Is.EqualTo(expected));
	}

	[TestCaseSource(nameof(OutsidePaths))]
	[Description(
		"Selected rows are restored from deserialized view settings, so a path that escapes the " +
		"directory being viewed must not resolve. Delete() removes directories recursively.")]
	public void GetSelectedPath_RowOutsideDirectory_ReturnsNull(string dataKey)
	{
		Assert.That(TabDirectory.GetSelectedPath(Root, RowWithKey(dataKey)), Is.Null);
	}

	public static IEnumerable<TestCaseData> OutsidePaths()
	{
		yield return new TestCaseData(Path.Combine(Root, "..")).SetName("Parent directory");
		yield return new TestCaseData(Path.Combine(Root, "..", "..")).SetName("Grandparent directory");
		yield return new TestCaseData(Path.Combine(Root, "..", "Sibling")).SetName("Sibling directory");
		yield return new TestCaseData(Root).SetName("The directory itself");
		yield return new TestCaseData(Path.GetTempPath()).SetName("Unrelated absolute path");
	}

	[Test, Description("A row without a DataKey has no authoritative path, so nothing is deleted for it")]
	public void GetSelectedPath_NoDataKey_ReturnsNull()
	{
		Assert.That(TabDirectory.GetSelectedPath(Root, RowWithKey(null)), Is.Null);
		Assert.That(TabDirectory.GetSelectedPath(Root, RowWithKey("")), Is.Null);
	}

	[Test, Description("The label is a display string and must not be used to build the delete path")]
	public void GetSelectedPath_IgnoresTheLabel()
	{
		var selectedRow = new SelectedRow
		{
			Label = "file.txt", // Looks like a valid entry
			DataKey = null,
		};

		Assert.That(TabDirectory.GetSelectedPath(Root, selectedRow), Is.Null);
	}
}
