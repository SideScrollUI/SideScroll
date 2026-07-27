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
