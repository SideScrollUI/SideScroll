using NUnit.Framework;
using SideScroll.Collections;

namespace SideScroll.Tests.Collections;

[Category("Core")]
public class ItemCollectionUITests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("ItemCollectionUI");
	}

	[Test, Description("Posted bulk additions use the values present when AddRange is called")]
	public void AddRangeSnapshotsPostedEnumerable()
	{
		var context = new QueuedContext();
		ItemCollectionUI<int> items = new()
		{
			Context = context,
			PostOnly = true,
		};
		List<int> source = [1, 2];

		items.AddRange(source);
		source.Clear();
		source.Add(3);
		context.Drain();

		Assert.That(items, Is.EqualTo(new[] { 1, 2 }));
	}

	[Test, Description("Posted replacements use the values present when Replace is called")]
	public void ReplaceSnapshotsPostedEnumerable()
	{
		var context = new QueuedContext();
		ItemCollectionUI<int> items = new([0])
		{
			Context = context,
			PostOnly = true,
		};
		List<int> source = [1, 2];

		items.Replace(source);
		source.Clear();
		context.Drain();

		Assert.That(items, Is.EqualTo(new[] { 1, 2 }));
	}

	[Test, Description("A posted removal follows its item when an insert changes the original index")]
	public void RemoveItemResolvesCurrentIndexInCallback()
	{
		var context = new QueuedContext();
		ItemCollectionUI<string> items = new(["a", "b"])
		{
			Context = context,
			PostOnly = true,
		};

		items.RemoveAt(1);
		items.Insert(0, "new");
		context.DrainReverse();

		Assert.That(items, Is.EqualTo(new[] { "new", "a" }));
	}

	[Test, Description(
		"Falling back to a bare SynchronizationContext made UsePost true from then on, so every " +
		"change was posted to the thread pool, including ones already on the UI thread")]
	public void InitializingWithoutAContextLeavesItNull()
	{
		ItemCollectionUI<int> collection = [];

		Task.Run(() => collection.InitializeContext()).Wait();

		Assert.That(collection.Context, Is.Null);
		Assert.That(collection.UsePost, Is.False);
	}

	[Test, Description("Without a context an item is added in place rather than queued elsewhere")]
	public void AddWithoutAContextAppliesImmediately()
	{
		ItemCollectionUI<int> collection = [];
		Task.Run(() => collection.InitializeContext()).Wait();

		collection.Add(5);

		Assert.That(collection, Is.EqualTo(new[] { 5 }));
	}

	[Test, Description("A later call can still pick up a context, the unusable one used to be kept")]
	public void InitializeContextRetriesAfterFindingNone()
	{
		ItemCollectionUI<int> collection = [];
		Task.Run(() => collection.InitializeContext()).Wait();

		QueuedContext context = new();
		SynchronizationContext.SetSynchronizationContext(context);
		try
		{
			collection.InitializeContext();
			Assert.That(collection.Context, Is.SameAs(context));
		}
		finally
		{
			SynchronizationContext.SetSynchronizationContext(null);
		}
	}

	[Test, Description("A context that is found is still captured and still marshalled to")]
	public void ContextFromTheCallingThreadIsKept()
	{
		ItemCollectionUI<int> collection = [];
		QueuedContext context = new();

		SynchronizationContext.SetSynchronizationContext(context);
		try
		{
			collection.InitializeContext();
		}
		finally
		{
			// Off that context now, the way a background thread adding to it would be
			SynchronizationContext.SetSynchronizationContext(null);
		}

		Assert.That(collection.Context, Is.SameAs(context));

		collection.Add(7);
		Assert.That(collection, Is.Empty, "queued, not applied yet");

		context.Drain();
		Assert.That(collection, Is.EqualTo(new[] { 7 }));
	}

	private sealed class QueuedContext : SynchronizationContext
	{
		private readonly Queue<(SendOrPostCallback Callback, object? State)> _callbacks = new();

		public override void Post(SendOrPostCallback callback, object? state)
		{
			_callbacks.Enqueue((callback, state));
		}

		public void Drain()
		{
			while (_callbacks.TryDequeue(out var work))
			{
				work.Callback(work.State);
			}
		}

		public void DrainReverse()
		{
			foreach (var work in _callbacks.Reverse())
			{
				work.Callback(work.State);
			}
			_callbacks.Clear();
		}
	}
}
