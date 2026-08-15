using NUnit.Framework;
using SideScroll.Attributes;
using SideScroll.Serialize.Atlas;
using SideScroll.Serialize.Atlas.TypeRepos;
using System.Collections.ObjectModel;

namespace SideScroll.Serialize.Tests;

/// <summary>
/// A ReadOnlyCollection implements the non-generic IList, so the list repo claimed it and saved it,
/// then failed to construct it while loading and left an empty one behind
/// </summary>
[Category("Serialize")]
public class SerializeReadOnlyCollectionTests : SerializeBaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("SerializeReadOnlyCollections");
	}

	private T SaveLoad<T>(T input) where T : class
	{
		var serializer = new SerializerMemoryAtlas();
		serializer.Save(Call, input);
		return serializer.Load<T>(Call);
	}

	[PublicData]
	public class Item
	{
		public string? Name { get; set; }
	}

	[PublicData]
	public class Holder
	{
		public ReadOnlyCollection<string> Names { get; set; } = new([]);
		public ReadOnlyCollection<Item> Items { get; set; } = new([]);
	}

	[Test]
	public void SerializeReadOnlyCollectionOfStrings()
	{
		var output = SaveLoad(new Holder { Names = new(["alpha", "beta"]) });

		Assert.That(output.Names, Is.EqualTo(new[] { "alpha", "beta" }));
	}

	[Test, Description("The order it was constructed in is the order it keeps")]
	public void SerializeReadOnlyCollectionOfObjects()
	{
		var input = new Holder
		{
			Items = new([new Item { Name = "alpha" }, new Item { Name = "beta" }]),
		};

		var output = SaveLoad(input);

		Assert.That(output.Items.Select(i => i.Name), Is.EqualTo(new[] { "alpha", "beta" }));
	}

	[Test]
	public void SerializeEmptyReadOnlyCollection()
	{
		var output = SaveLoad(new Holder());

		Assert.That(output.Names, Is.Empty);
		Assert.That(output.Items, Is.Empty);
	}

	[Test, Description("As the root object rather than a member")]
	public void SerializeReadOnlyCollectionAsTheRoot()
	{
		var output = SaveLoad(new ReadOnlyCollection<string>(["alpha", "beta"]));

		Assert.That(output, Is.EqualTo(new[] { "alpha", "beta" }));
	}

	[Test, Description(
		"The collection is created around its list before the elements are read, so an element can " +
		"reference the collection it belongs to")]
	public void SerializeReadOnlyCollectionReferencingItself()
	{
		var items = new List<SelfReferencing>();
		var collection = new ReadOnlyCollection<SelfReferencing>(items);
		items.Add(new SelfReferencing { Owner = collection });

		var output = SaveLoad(new SelfHolder { Collection = collection });

		Assert.That(output.Collection, Has.Count.EqualTo(1));
		Assert.That(output.Collection[0].Owner, Is.SameAs(output.Collection));
	}

	[PublicData]
	public class SelfReferencing
	{
		public ReadOnlyCollection<SelfReferencing>? Owner { get; set; }
	}

	[PublicData]
	public class SelfHolder
	{
		public ReadOnlyCollection<SelfReferencing> Collection { get; set; } = new([]);
	}

	[Test, Description("Cloning has to build the collection around its list too")]
	public void CloneReadOnlyCollection()
	{
		var input = new Holder { Names = new(["alpha", "beta"]) };

		var clone = input.DeepClone(Call);

		Assert.That(clone.Names, Is.EqualTo(new[] { "alpha", "beta" }));
		Assert.That(clone.Names, Is.Not.SameAs(input.Names));
	}

	[Test]
	public void ReadOnlyCollectionsAreClaimedBeforeTheListRepo()
	{
		Assert.Multiple(() =>
		{
			Assert.That(TypeRepoReadOnlyCollection.CanAssign(typeof(ReadOnlyCollection<string>)), Is.True);
			Assert.That(TypeRepoReadOnlyCollection.CanAssign(typeof(List<string>)), Is.False);
			Assert.That(TypeRepoReadOnlyCollection.CanAssign(typeof(string)), Is.False);
		});
	}

	private class DerivedReadOnly(IList<string> list) : ReadOnlyCollection<string>(list);

	[Test, Description("A subclass is claimed when it keeps a constructor taking the list to wrap")]
	public void DerivedReadOnlyCollectionsAreClaimed()
	{
		Assert.That(TypeRepoReadOnlyCollection.CanAssign(typeof(DerivedReadOnly)), Is.True);
	}
}
