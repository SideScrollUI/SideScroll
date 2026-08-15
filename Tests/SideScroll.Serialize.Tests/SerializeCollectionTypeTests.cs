using NUnit.Framework;
using SideScroll.Attributes;
using SideScroll.Serialize.Atlas;
using SideScroll.Serialize.Atlas.TypeRepos;

namespace SideScroll.Serialize.Tests;

/// <summary>
/// The collections that aren't an IList, IDictionary, or HashSet had no repo of their own, so they
/// were saved through their properties and reloaded empty without anything reporting it
/// </summary>
[Category("Serialize")]
public class SerializeCollectionTypeTests : SerializeBaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("SerializeCollectionTypes");
	}

	private T SaveLoad<T>(T input) where T : class
	{
		var serializer = new SerializerMemoryAtlas();
		serializer.Save(Call, input);
		return serializer.Load<T>(Call);
	}

	private static readonly string[] Values = ["alpha", "beta", "gamma"];

	[Test]
	public void SerializeSortedSet()
	{
		var output = SaveLoad(new SortedSet<string>(Values));

		Assert.That(output, Is.EqualTo(new[] { "alpha", "beta", "gamma" }));
	}

	[Test, Description("A queue keeps the order it was enqueued in")]
	public void SerializeQueue()
	{
		var output = SaveLoad(new Queue<string>(Values));

		Assert.That(output, Is.EqualTo(Values));
		Assert.That(output.Dequeue(), Is.EqualTo("alpha"));
	}

	[Test, Description(
		"A stack enumerates from the top, so pushing the elements back in that order would reverse " +
		"it on every load")]
	public void SerializeStack()
	{
		var output = SaveLoad(new Stack<string>(Values));

		Assert.That(output, Is.EqualTo(new[] { "gamma", "beta", "alpha" }));
		Assert.That(output.Pop(), Is.EqualTo("gamma"));
	}

	[Test]
	public void SerializeLinkedList()
	{
		var output = SaveLoad(new LinkedList<string>(Values));

		Assert.That(output, Is.EqualTo(Values));
		Assert.That(output.First!.Value, Is.EqualTo("alpha"));
		Assert.That(output.Last!.Value, Is.EqualTo("gamma"));
	}

	[Test]
	public void SerializeEmptyCollections()
	{
		Assert.That(SaveLoad(new SortedSet<string>()), Is.Empty);
		Assert.That(SaveLoad(new Queue<string>()), Is.Empty);
		Assert.That(SaveLoad(new Stack<string>()), Is.Empty);
		Assert.That(SaveLoad(new LinkedList<string>()), Is.Empty);
	}

	/// <summary>Orders on a member, so an element compared before it's read compares as any other</summary>
	[PublicData]
	public class ComparableItem : IComparable<ComparableItem>
	{
		public string? Name { get; set; }

		public int CompareTo(ComparableItem? other) => string.CompareOrdinal(Name, other?.Name);
	}

	[Test, Description(
		"A SortedSet orders each element as it's added, so elements added before their own members " +
		"were read all compared equal and every one after the first was dropped as a duplicate")]
	public void SerializeSortedSetOfComparableItems()
	{
		SortedSet<ComparableItem> input =
		[
			new ComparableItem { Name = "alpha" },
			new ComparableItem { Name = "beta" },
			new ComparableItem { Name = "gamma" },
		];

		var output = SaveLoad(input);

		Assert.That(output, Has.Count.EqualTo(3));
		Assert.That(output.Select(i => i.Name), Is.EqualTo(new[] { "alpha", "beta", "gamma" }));
	}

	[PublicData]
	public class Holder
	{
		public Queue<string> Queue { get; set; } = new();
		public Stack<string> Stack { get; set; } = new();
	}

	[Test, Description("Nested in another object rather than being the root")]
	public void SerializeCollectionsAsMembers()
	{
		var input = new Holder();
		input.Queue.Enqueue("first");
		input.Queue.Enqueue("second");
		input.Stack.Push("bottom");
		input.Stack.Push("top");

		var output = SaveLoad(input);

		Assert.That(output.Queue.Dequeue(), Is.EqualTo("first"));
		Assert.That(output.Stack.Pop(), Is.EqualTo("top"));
	}

	[Test, Description("DeepClone() saves and loads, so this covers the same path as the tests above")]
	public void DeepCloneKeepsCollectionOrder()
	{
		var stack = new Stack<string>(Values);

		var clone = stack.DeepClone(Call);

		Assert.That(clone, Is.EqualTo(new[] { "gamma", "beta", "alpha" }));
		Assert.That(clone, Is.Not.SameAs(stack));
	}

	[Test, Description(
		"Serializer.Clone() copies in memory instead of saving and loading, so it reverses the " +
		"stack separately from the load path")]
	public void CloneKeepsCollectionOrderInMemory()
	{
		var input = new Holder();
		input.Queue.Enqueue("first");
		input.Queue.Enqueue("second");
		input.Stack.Push("bottom");
		input.Stack.Push("top");

		Holder clone = new Serializer().Clone(Call.Log, input)!;

		Assert.That(clone.Queue, Is.EqualTo(new[] { "first", "second" }));
		Assert.That(clone.Stack, Is.EqualTo(new[] { "top", "bottom" }));
	}

	[Test, Description("Only the named collections are claimed, so an enumerable type keeps its properties")]
	public void SupportedTypesAreNamedRatherThanInferred()
	{
		Assert.Multiple(() =>
		{
			Assert.That(TypeRepoCollection.CanAssign(typeof(SortedSet<string>)), Is.True);
			Assert.That(TypeRepoCollection.CanAssign(typeof(Queue<string>)), Is.True);
			Assert.That(TypeRepoCollection.CanAssign(typeof(Stack<string>)), Is.True);
			Assert.That(TypeRepoCollection.CanAssign(typeof(LinkedList<string>)), Is.True);

			// Claimed by the repos registered before this one
			Assert.That(TypeRepoCollection.CanAssign(typeof(List<string>)), Is.False);
			Assert.That(TypeRepoCollection.CanAssign(typeof(HashSet<string>)), Is.False);
			Assert.That(TypeRepoCollection.CanAssign(typeof(Dictionary<string, int>)), Is.False);
			Assert.That(TypeRepoCollection.CanAssign(typeof(string)), Is.False);
		});
	}

	private class DerivedQueue : Queue<string>;

	[Test, Description("A subclass is matched through the types it derives from")]
	public void DerivedCollectionsAreClaimed()
	{
		Assert.That(TypeRepoCollection.CanAssign(typeof(DerivedQueue)), Is.True);
	}
}
