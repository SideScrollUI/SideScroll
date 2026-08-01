using NUnit.Framework;
using SideScroll.Tabs.Bookmarks;
using SideScroll.Tabs.Bookmarks.Models;

namespace SideScroll.Tabs.Tests;

[Category("Tabs")]
public class BookmarkNavigatorTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("BookmarkNavigator");
	}

	[TestCase(0)]
	[TestCase(-1)]
	[Description(
		"TrimHistory() asked RemoveRange() to remove more entries than the history holds, so an " +
		"unusable limit threw during ordinary navigation instead of just keeping nothing")]
	public void NonPositiveMaxHistorySizeDoesNotThrow(int maxHistorySize)
	{
		BookmarkNavigator navigator = new()
		{
			MaxHistorySize = maxHistorySize,
		};

		Assert.DoesNotThrow(() => navigator.Append(new Bookmark(), true));
		Assert.DoesNotThrow(() => navigator.Append(new Bookmark(), true));
	}

	[Test, Description("The history is still trimmed to the configured limit")]
	public void HistoryIsTrimmedToMaxHistorySize()
	{
		BookmarkNavigator navigator = new()
		{
			MaxHistorySize = 3,
		};

		for (int i = 0; i < 10; i++)
		{
			navigator.Append(new Bookmark(), true);
		}

		Assert.That(navigator.History, Has.Count.LessThanOrEqualTo(3));
	}
}
