using NUnit.Framework;
using SideScroll.Serialize.DataRepos;

namespace SideScroll.Serialize.Tests;

[Category("Serialize")]
public class DataViewCollectionTests : SerializeBaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("DataViewCollection");
	}

	private sealed class SampleView : IDataViewItem
	{
		public string? Name { get; private set; }

		public void Load(object sender, object obj, params object?[] loadParams)
		{
			Name = obj.ToString();
		}
	}

	// Counts the repository deletes that a single collection delete performs
	private sealed class CountingDataRepoView<T>(DataRepo dataRepo, string groupId) : DataRepoView<T>(dataRepo, groupId)
	{
		public int DeleteCount { get; private set; }

		public override void Delete(Call? call = null, string? key = null)
		{
			DeleteCount++;
			base.Delete(call, key);
		}
	}

	private CountingDataRepoView<string> LoadView(string groupId, params string[] keys)
	{
		var dataRepo = new DataRepo(Path.Combine(TestPath, "DataViewCollection"), "Test");
		var dataRepoView = new CountingDataRepoView<string>(dataRepo, groupId);

		dataRepoView.DeleteAll(Call);
		foreach (string key in keys)
		{
			dataRepoView.Save(Call, key, key);
		}
		dataRepoView.LoadAll(Call);

		return dataRepoView;
	}

	[Test, Description("Removing through the collection deletes the repository item once")]
	public void RemovingThroughViewCollectionDeletesOnce()
	{
		CountingDataRepoView<string> dataRepoView = LoadView("RemoveOnce", "a", "b");
		using var collection = new DataViewCollection<string, SampleView>(dataRepoView);

		int deleteEvents = 0;
		collection.OnDelete += (_, _) => deleteEvents++;

		IDataItem dataItem = dataRepoView.Items.First(item => item.Key == "a");
		collection.Remove(dataItem);

		Assert.Multiple(() =>
		{
			Assert.That(dataRepoView.DeleteCount, Is.EqualTo(1));
			Assert.That(deleteEvents, Is.EqualTo(1));
			Assert.That(dataRepoView.Items.Keys, Is.EqualTo(new[] { "b" }));
			Assert.That(collection.Items.Select(view => view.Name), Is.EqualTo(new[] { "b" }));
		});
	}

	[Test, Description("Removing through the untyped collection deletes the repository item once")]
	public void RemovingThroughValueCollectionDeletesOnce()
	{
		CountingDataRepoView<string> dataRepoView = LoadView("RemoveValueOnce", "a", "b");
		using var collection = new DataViewCollection<string>(dataRepoView);

		int deleteEvents = 0;
		collection.OnDelete += (_, _) => deleteEvents++;

		IDataItem dataItem = dataRepoView.Items.First(item => item.Key == "a");
		collection.Remove(dataItem);

		Assert.Multiple(() =>
		{
			Assert.That(dataRepoView.DeleteCount, Is.EqualTo(1));
			Assert.That(deleteEvents, Is.EqualTo(1));
			Assert.That(collection.Items, Is.EqualTo(new[] { "b" }));
		});
	}

	[Test, Description("Deleting from the repository removes the view item once")]
	public void DeletingFromRepositoryRemovesViewItem()
	{
		CountingDataRepoView<string> dataRepoView = LoadView("RepositoryDelete", "a", "b");
		using var collection = new DataViewCollection<string, SampleView>(dataRepoView);

		int deleteEvents = 0;
		collection.OnDelete += (_, _) => deleteEvents++;

		dataRepoView.Delete(Call, "a");

		Assert.Multiple(() =>
		{
			Assert.That(dataRepoView.DeleteCount, Is.EqualTo(1));
			Assert.That(deleteEvents, Is.EqualTo(1));
			Assert.That(collection.Items.Select(view => view.Name), Is.EqualTo(new[] { "b" }));
		});
	}

	[Test, Description("Reloading the repository view keeps the collection subscribed")]
	public void ReloadingRepositoryViewKeepsCollectionSynchronized()
	{
		CountingDataRepoView<string> dataRepoView = LoadView("Reload", "a");
		using var collection = new DataViewCollection<string, SampleView>(dataRepoView);

		dataRepoView.Save(Call, "b", "b");
		dataRepoView.LoadAll(Call);

		Assert.That(collection.Items.Select(view => view.Name), Is.EquivalentTo(new[] { "a", "b" }));

		// Replacing the Items instance used to leave the collection attached to the abandoned one
		dataRepoView.Save(Call, "c", "c");

		Assert.That(collection.Items.Select(view => view.Name), Is.EquivalentTo(new[] { "a", "b", "c" }));
	}

	[Test, Description("Sorting the repository view keeps the collection subscribed")]
	public void SortingRepositoryViewKeepsCollectionSynchronized()
	{
		CountingDataRepoView<string> dataRepoView = LoadView("Sort", "b", "a");
		using var collection = new DataViewCollection<string, SampleView>(dataRepoView);

		dataRepoView.SortBy(nameof(string.Length));
		dataRepoView.Save(Call, "c", "c");

		Assert.That(collection.Items.Select(view => view.Name), Is.EquivalentTo(new[] { "a", "b", "c" }));
	}

	[Test, Description("Clearing the repository drops the collection's lookups with its items")]
	public void ClearingRepositoryDropsCollectionLookups()
	{
		CountingDataRepoView<string> dataRepoView = LoadView("Reset", "a");
		using var collection = new DataViewCollection<string, SampleView>(dataRepoView);

		IDataItem dataItem = dataRepoView.Items.First();
		int deleteEvents = 0;
		collection.OnDelete += (_, _) => deleteEvents++;

		dataRepoView.DeleteAll(Call);

		Assert.That(collection.Items, Is.Empty);

		// The item is no longer displayed, so removing it again reports nothing
		collection.Remove(dataItem);

		Assert.That(deleteEvents, Is.Zero);
	}

	[Test, Description("Disposing detaches the collection from the repository view")]
	public void DisposeStopsMirroringRepositoryView()
	{
		CountingDataRepoView<string> dataRepoView = LoadView("Dispose", "a");
		var collection = new DataViewCollection<string, SampleView>(dataRepoView);

		collection.Dispose();
		dataRepoView.Save(Call, "b", "b");

		Assert.That(collection.Items.Select(view => view.Name), Is.EqualTo(new[] { "a" }));
	}
}
