using NUnit.Framework;
using SideScroll.Collections;
using System.Collections;
using System.Collections.ObjectModel;

namespace SideScroll.Tests.Collections;

[Category("Core")]
public class ItemQueueCollectionTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Core");
	}

	private static ItemQueueCollection<int> CreateQueue(int maxCount = 3)
	{
		return new ItemQueueCollection<int> { MaxCount = maxCount };
	}

	[Test]
	public void AddDropsOldestOverMaxCount()
	{
		ItemQueueCollection<int> queue = CreateQueue();

		for (int i = 0; i < 5; i++)
		{
			queue.Add(i);
		}

		Assert.That(queue, Is.EqualTo(new[] { 2, 3, 4 }));
	}

	[Test, Description("The limit applies when items are added through a base class reference")]
	public void AddThroughBaseClassDropsOldest()
	{
		ItemQueueCollection<int> queue = CreateQueue();
		ObservableCollection<int> asObservable = queue;

		for (int i = 0; i < 5; i++)
		{
			asObservable.Add(i);
		}

		Assert.That(queue, Is.EqualTo(new[] { 2, 3, 4 }));
	}

	[Test, Description("The limit applies when items are added through IList")]
	public void AddThroughIListDropsOldest()
	{
		ItemQueueCollection<int> queue = CreateQueue();
		IList asList = queue;

		for (int i = 0; i < 5; i++)
		{
			asList.Add(i);
		}

		Assert.That(queue, Is.EqualTo(new[] { 2, 3, 4 }));
	}

	[Test, Description("AddRange writes to the backing list, so it needs its own trim")]
	public void AddRangeDropsOldest()
	{
		ItemQueueCollection<int> queue = CreateQueue();

		queue.AddRange([0, 1, 2, 3, 4]);

		Assert.That(queue, Is.EqualTo(new[] { 2, 3, 4 }));
	}

	[Test]
	public void StaysUnderMaxCount()
	{
		ItemQueueCollection<int> queue = CreateQueue();

		queue.Add(1);
		queue.Add(2);

		Assert.That(queue, Is.EqualTo(new[] { 1, 2 }));
	}

	[Test, Description("A negative queue limit is treated as zero instead of removing from an empty collection")]
	public void NegativeMaxCountIsClampedToZero()
	{
		ItemQueueCollection<int> queue = CreateQueue();
		queue.AddRange([1, 2]);

		Assert.DoesNotThrow(() => queue.MaxCount = -1);
		Assert.That(queue.MaxCount, Is.Zero);
		Assert.That(queue, Is.Empty);
		Assert.DoesNotThrow(() => queue.Add(3));
		Assert.That(queue, Is.Empty);
	}

	[Test, Description("Adding a collection to itself snapshots the source before mutation")]
	public void AddRangeFromSelf()
	{
		ItemQueueCollection<int> queue = CreateQueue(maxCount: 10);
		queue.AddRange([1, 2]);

		Assert.DoesNotThrow(() => queue.AddRange(queue));
		Assert.That(queue, Is.EqualTo(new[] { 1, 2, 1, 2 }));
	}
}
