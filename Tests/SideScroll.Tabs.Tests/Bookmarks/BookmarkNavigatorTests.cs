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

	[Test, Description(
		"Falling back to a bare SynchronizationContext posted every notification to the thread " +
		"pool, so a navigator built on a background thread reported changes off the thread that " +
		"made them, which is a cross thread access for a bound control")]
	public void NotificationsRunOnTheThreadThatChangedTheProperty()
	{
		BookmarkNavigator navigator = null!;
		Task.Run(() => navigator = new BookmarkNavigator()).Wait();

		int notifiedThreadId = 0;
		using ManualResetEventSlim notified = new();
		navigator.PropertyChanged += (sender, e) =>
		{
			notifiedThreadId = Environment.CurrentManagedThreadId;
			notified.Set();
		};

		navigator.CurrentIndex = 0;

		Assert.That(notified.Wait(TimeSpan.FromSeconds(5)), Is.True, "never notified");
		Assert.That(notifiedThreadId, Is.EqualTo(Environment.CurrentManagedThreadId));
	}

	[Test, Description("A context captured on the thread that built it is still used")]
	public void ContextFromTheConstructingThreadIsKept()
	{
		SynchronizationContext context = new();
		SynchronizationContext.SetSynchronizationContext(context);
		try
		{
			Assert.That(new BookmarkNavigator().Context, Is.SameAs(context));
		}
		finally
		{
			SynchronizationContext.SetSynchronizationContext(null);
		}
	}

	[Test, Description(
		"History and CurrentIndex are both settable and deserialized, so the index can point " +
		"outside the history, which Update() indexed without the bounds check Current uses")]
	public void UpdateWithAnIndexOutsideTheHistoryDoesNothing()
	{
		BookmarkNavigator navigator = new()
		{
			History = [],
			CurrentIndex = -1,
		};

		Assert.DoesNotThrow(() => navigator.Update(new Bookmark()));

		navigator.CurrentIndex = 5;
		Assert.DoesNotThrow(() => navigator.Update(new Bookmark()));
	}

	[Test, Description("Update() still writes through to the current bookmark")]
	public void UpdateAppliesToTheCurrentBookmark()
	{
		BookmarkNavigator navigator = new();
		TabBookmark tabBookmark = new();

		navigator.Update(new Bookmark { TabBookmark = tabBookmark });

		Assert.That(navigator.Current!.TabBookmark, Is.SameAs(tabBookmark));
	}

	[Test, Description(
		"SeekBackward() only compared against zero, so an index past the end stepped back to " +
		"another position still outside the history and indexed it")]
	public void SeekBackwardWithAnIndexPastTheHistoryReturnsNull()
	{
		BookmarkNavigator navigator = new()
		{
			CurrentIndex = 100,
		};

		Assert.That(navigator.SeekBackward(), Is.Null);
	}

	[Test, Description("SeekForward() covered the upper bound, a negative index reached the history")]
	public void SeekForwardWithANegativeIndexReturnsNull()
	{
		BookmarkNavigator navigator = new()
		{
			CurrentIndex = -5,
		};

		Assert.That(navigator.SeekForward(), Is.Null);
	}

	[Test, Description("Seeking still walks the history it was already able to")]
	public void SeekMovesThroughTheHistory()
	{
		BookmarkNavigator navigator = new();
		navigator.Append(new Bookmark(), true);
		navigator.Append(new Bookmark(), true);

		Assert.That(navigator.CurrentIndex, Is.EqualTo(2));
		Assert.That(navigator.SeekBackward(), Is.Not.Null);
		Assert.That(navigator.CurrentIndex, Is.EqualTo(1));

		// The clone SeekBackward() appended is ahead of it now
		Assert.That(navigator.SeekForward(), Is.Not.Null);
		Assert.That(navigator.CurrentIndex, Is.EqualTo(2));
	}

	[Test, Description("Seeking past either end returns null rather than moving")]
	public void SeekStopsAtTheEndsOfTheHistory()
	{
		BookmarkNavigator navigator = new();

		Assert.That(navigator.SeekBackward(), Is.Null, "at the start");
		Assert.That(navigator.SeekForward(), Is.Null, "at the end");
		Assert.That(navigator.CurrentIndex, Is.EqualTo(0));
	}
}
