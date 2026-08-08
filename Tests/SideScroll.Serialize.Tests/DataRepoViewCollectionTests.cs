using NUnit.Framework;
using SideScroll.Serialize.DataRepos;
using System.Runtime.CompilerServices;

namespace SideScroll.Serialize.Tests;

[Category("Serialize")]
public class DataRepoViewCollectionTests : SerializeBaseTest
{
	private readonly DataRepo _dataRepo = new(TestPath, "DataRepoViewCollection");

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("DataRepoViewCollection");
	}

	private DataRepoViewCollection<int> CreateCollection() => new(_dataRepo, "Default");

	[Test]
	public void LoadReusesTheViewWhileItIsReferenced()
	{
		var collection = CreateCollection();

		DataRepoView<int> first = collection.Load(Call, "Group");
		DataRepoView<int> second = collection.Load(Call, "Group");

		Assert.That(second, Is.SameAs(first));
	}

	[Test]
	public void LoadUsesTheDefaultGroupWhenNoneIsGiven()
	{
		var collection = CreateCollection();

		DataRepoView<int> view = collection.Load(Call);

		Assert.That(view.GroupId, Is.EqualTo("Default"));
		Assert.That(collection.Load(Call, "Default"), Is.SameAs(view));
	}

	[Test]
	public void SeparateGroupsGetSeparateViews()
	{
		var collection = CreateCollection();

		DataRepoView<int> a = collection.Load(Call, "A");
		DataRepoView<int> b = collection.Load(Call, "B");

		Assert.That(b, Is.Not.SameAs(a));
	}

	// Kept out of the test method so the view has no local still referencing it
	[MethodImpl(MethodImplOptions.NoInlining)]
	private WeakReference LoadAndRelease(DataRepoViewCollection<int> collection, string groupId)
	{
		return new WeakReference(collection.Load(Call, groupId));
	}

	[Test]
	[Description(
		"Every view holds the items loaded for its group and the groupId comes from the caller, so " +
		"the collection can't hold them strongly. It can't evict a live one either, that would hand " +
		"out a second view mirroring the same repository")]
	public void UnreferencedViewsAreReleased()
	{
		var collection = CreateCollection();

		WeakReference reference = LoadAndRelease(collection, "Collected");

		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();

		Assert.That(reference.IsAlive, Is.False);

		// The collection has to stay reachable, it's what would be holding the view
		GC.KeepAlive(collection);
	}

	[Test]
	public void ReloadingACollectedGroupCreatesAWorkingView()
	{
		var collection = CreateCollection();

		LoadAndRelease(collection, "Reloaded");

		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();

		DataRepoView<int> reloaded = collection.Load(Call, "Reloaded");

		Assert.That(reloaded, Is.Not.Null);
		Assert.That(reloaded.GroupId, Is.EqualTo("Reloaded"));
	}
}
