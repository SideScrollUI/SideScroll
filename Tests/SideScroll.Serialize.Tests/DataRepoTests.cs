using NUnit.Framework;
using SideScroll.Serialize.Atlas;
using SideScroll.Serialize.DataRepos;
using SideScroll.Serialize.Json;

namespace SideScroll.Serialize.Tests;

[Category("Serialize")]
public class DataRepoTests : SerializeBaseTest
{
	private readonly DataRepo _dataRepo = new(TestPath, "Test");

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("DataRepo");
	}

	[SetUp]
	public void Setup()
	{
	}

	private DataRepoInstance<int> OpenRepo(bool index = false)
	{
		string groupId = "DataRepoTest";
		var instance = _dataRepo.Open<int>(groupId, index);
		instance.DeleteAll(Call);
		return instance;
	}

	[Test]
	public void DataItemCollectionEnumerableConstructorPopulatesLookup()
	{
		var items = new DataItemCollection<int>(
		[
			new("b", 2),
			new("a", 1),
		]);

		Assert.That(items.ContainsKey("a"), Is.True);
		Assert.That(items.TryGetValue("b", out int value), Is.True);
		Assert.That(value, Is.EqualTo(2));
		Assert.That(items.SortedValues, Is.EqualTo(new[] { 1, 2 }));
	}

	[Test, Description("DataRepo int Save Load")]
	public void DataRepoSaveLoadInt()
	{
		string keyId = "int";
		int input = 1;
		_dataRepo.Save(keyId, input, Call);
		int output = _dataRepo.Load<int>(keyId, Call);

		Assert.That(output, Is.EqualTo(input));
	}

	[Test, Description("DataInstance int Save Load")]
	public void DataInstanceSaveLoadInt()
	{
		string keyId = "int";
		int input = 1;
		var instance = OpenRepo();
		instance.Save(Call, keyId, input);

		int output = instance.Load(Call, keyId);
		Assert.That(output, Is.EqualTo(input));
	}

	[Test, Description("DataInstance Paging")]
	public void DataInstancePaging()
	{
		int pageSize = 2;
		var instance = OpenRepo();
		for (int i = 0; i < 5; i++)
		{
			instance.Save(Call, i.ToString(), i);
		}

		var pageView = instance.LoadPageView(Call);
		pageView.PageSize = pageSize;

		// Order is unknown without indexing
		var page1 = pageView.Next(Call).ToList();
		Assert.That(page1, Has.Exactly(pageSize).Items);

		var page2 = pageView.Next(Call).ToList();
		Assert.That(page2, Has.Exactly(pageSize).Items);
	}

	[TestCase(0)]
	[TestCase(-1)]
	[NonParallelizable]
	public void DataPageViewRejectsNonPositivePageSizes(int pageSize)
	{
		DataRepoInstance<int> instance = OpenRepo();

		Assert.Multiple(() =>
		{
			ArgumentOutOfRangeException constructorException = Assert.Throws<ArgumentOutOfRangeException>(
				() => new DataPageView<int>(instance, ascending: true, pageSize))!;
			Assert.That(constructorException.ParamName, Is.EqualTo("pageSize"));

			ArgumentOutOfRangeException propertyException = Assert.Throws<ArgumentOutOfRangeException>(
				() => instance.LoadPageView(Call).PageSize = pageSize)!;
			Assert.That(propertyException.ParamName, Is.EqualTo(nameof(DataPageView<int>.PageSize)));

			ArgumentOutOfRangeException defaultException = Assert.Throws<ArgumentOutOfRangeException>(
				() => DataPageView<int>.DefaultPageSize = pageSize)!;
			Assert.That(defaultException.ParamName, Is.EqualTo(nameof(DataPageView<int>.DefaultPageSize)));
		});
	}

	[Test, Description("A larger page size clamps a page index past the last page")]
	public void DataPageViewClampsPageIndexToLastPage()
	{
		DataRepoInstance<int> instance = OpenRepo();
		for (int i = 0; i < 5; i++)
		{
			instance.Save(Call, i.ToString(), i);
		}

		DataPageView<int> pageView = instance.LoadPageView(Call);
		pageView.PageSize = 1;
		pageView.GetPage(0, Call);
		pageView.PageIndex = 4;

		pageView.PageSize = 2;

		Assert.That(pageView.PageCount, Is.EqualTo(3));
		Assert.That(pageView.PageIndex, Is.EqualTo(2));
		Assert.That(pageView.HasNext, Is.False);
		Assert.That(pageView.GetPage(pageView.PageIndex, Call), Has.Exactly(1).Items);
	}

	[Test, Description("DataInstance Index Paging")]
	public void DataInstancePagingIndex()
	{
		int pageSize = 2;
		var instance = OpenRepo(true);
		for (int i = 0; i < 5; i++)
		{
			instance.Save(Call, i.ToString(), i);
		}

		DataPageView<int> pageView = instance.LoadPageView(Call);
		pageView.PageSize = pageSize;

		var page1 = pageView.Next(Call).ToList();
		Assert.That(page1, Has.Exactly(pageSize).Items);
		Assert.That(page1[0].Value, Is.EqualTo(0));
		Assert.That(page1[1].Value, Is.EqualTo(1));

		var page2 = pageView.Next(Call).ToList();
		Assert.That(page2, Has.Exactly(pageSize).Items);
		Assert.That(page2[0].Value, Is.EqualTo(2));
		Assert.That(page2[1].Value, Is.EqualTo(3));
	}

	[Test, Description("DataInstance Index Replace")]
	public void DataInstancePagingReplace()
	{
		var instance = OpenRepo(true);

		int input = 1;
		instance.Save(Call, input.ToString(), input);
		instance.Save(Call, input.ToString(), input);

		DataItemCollection<int> loaded = instance.LoadAll(Call);
		Assert.That(loaded, Has.Exactly(1).Items);
		Assert.That(loaded[0].Value, Is.EqualTo(1));
	}

	[Test, Description("DataInstance Index MaxItems")]
	public void DataInstanceIndexMaxItems()
	{
		var instance = OpenRepo(true);
		instance.Index!.MaxItems = 2;
		for (int i = 0; i < 3; i++)
		{
			instance.Save(Call, i.ToString(), i);
		}

		DataItemCollection<int> allItems = instance.LoadAll(Call);

		Assert.That(allItems, Has.Exactly(2).Items);
		Assert.That(allItems[0].Value, Is.EqualTo(1));
		Assert.That(allItems[1].Value, Is.EqualTo(2));
	}

	[Test, Description("CleanupCache keeps recent items in a JSON DataRepo")]
	public void CleanupCacheJsonKeepsRecentItems()
	{
		var jsonRepo = new DataRepo(Path.Combine(TestPath, "CleanupCacheJson"), "Test", useJson: true);
		jsonRepo.DeleteRepo();

		jsonRepo.Save("item", 5, Call);
		jsonRepo.CleanupCache(Call, TimeSpan.FromDays(1));

		Assert.That(jsonRepo.Load<int>("item", Call), Is.EqualTo(5));
	}

	[Test, Description("JSON DataRepo bulk loading preserves saved keys and headers")]
	public void JsonDataRepoLoadAllPreservesKeys()
	{
		const string groupId = "JsonLoadAll";
		var jsonRepo = new DataRepo(Path.Combine(TestPath, groupId), "Test", useJson: true);
		jsonRepo.DeleteRepo();
		jsonRepo.Save(groupId, "saved-key", 5, Call);

		DataItemCollection<int> items = jsonRepo.LoadAll<int>(Call, groupId);
		List<SerializerHeader> headers = jsonRepo.LoadHeaders(typeof(int), groupId, Call);

		Assert.That(items, Has.Count.EqualTo(1));
		Assert.That(items[0].Key, Is.EqualTo("saved-key"));
		Assert.That(items[0].Value, Is.EqualTo(5));
		Assert.That(headers.Select(header => header.Name), Is.EqualTo(new[] { "saved-key" }));

		string itemPath = jsonRepo.GetDataPath(typeof(int), groupId, "saved-key");
		string headerJson = File.ReadAllText(Path.Combine(itemPath, SerializerFileJson.HeaderFileName));
		Assert.That(headerJson, Does.Contain("\"Version\":1"));
		Assert.That(headerJson, Does.Contain("\"Name\":\"saved-key\""));
	}

	[Test]
	public void LoadHeadersSkipsCorruptJsonHeader()
	{
		const string groupId = "CorruptJsonHeader";
		var jsonRepo = new DataRepo(Path.Combine(TestPath, groupId), "Test", useJson: true);
		jsonRepo.DeleteRepo();
		jsonRepo.Save(groupId, "valid", 1, Call);
		jsonRepo.Save(groupId, "corrupt", 2, Call);

		string corruptPath = jsonRepo.GetDataPath(typeof(int), groupId, "corrupt");
		File.WriteAllText(Path.Combine(corruptPath, SerializerFileJson.HeaderFileName), "{ invalid json");

		List<SerializerHeader> headers = jsonRepo.LoadHeaders(typeof(int), groupId, Call);

		Assert.That(headers.Select(header => header.Name), Is.EqualTo(new[] { "valid" }));
		Assert.That(Call.Log.EntriesText(), Does.Contain("Exception loading repository header"));
		Assert.That(Call.Log.EntriesText(), Does.Contain(Path.GetFileName(corruptPath)));
	}

	[Test]
	public void LoadAllSkipsCorruptJsonItem()
	{
		const string groupId = "CorruptJsonItem";
		var jsonRepo = new DataRepo(Path.Combine(TestPath, groupId), "Test", useJson: true);
		jsonRepo.DeleteRepo();
		jsonRepo.Save(groupId, "valid", 1, Call);
		jsonRepo.Save(groupId, "corrupt", 2, Call);

		string corruptPath = jsonRepo.GetDataPath(typeof(int), groupId, "corrupt");
		File.WriteAllText(Path.Combine(corruptPath, SerializerFileJson.HeaderFileName), "{ invalid json");

		DataItemCollection<int> items = jsonRepo.LoadAll<int>(Call, groupId);

		Assert.That(items.Select(item => item.Key), Is.EqualTo(new[] { "valid" }));
		Assert.That(Call.Log.EntriesText(), Does.Contain("Exception loading repository item"));
		Assert.That(Call.Log.EntriesText(), Does.Contain(Path.GetFileName(corruptPath)));
	}

	[Test, Description("Indexed JSON bulk and page loading use the persisted index keys")]
	public void IndexedJsonDataRepoPreservesKeys()
	{
		const string groupId = "JsonIndexedKeys";
		var jsonRepo = new DataRepo(Path.Combine(TestPath, groupId), "Test", useJson: true);
		jsonRepo.DeleteRepo();
		DataRepoInstance<int> instance = jsonRepo.Open<int>(groupId, indexed: true);
		instance.Save(Call, "first-key", 5);
		instance.Save(Call, "second-key", 5);

		DataItemCollection<int> items = instance.LoadAll(Call);
		DataPageView<int> page = instance.LoadPageView(Call);
		List<DataItem<int>> pageItems = page.Next(Call);

		Assert.That(items.Keys, Is.EqualTo(new[] { "first-key", "second-key" }));
		Assert.That(pageItems.Select(item => item.Key), Is.EqualTo(new[] { "first-key", "second-key" }));
	}

	[Test, Description("DataRepo indices reject negative retention limits")]
	public void DataRepoIndexRejectsNegativeMaxItems()
	{
		DataRepoInstance<int> instance = OpenRepo();

		Assert.Throws<ArgumentOutOfRangeException>(() => new DataRepoIndex<int>(instance, -1));

		var index = new DataRepoIndex<int>(instance);
		Assert.Throws<ArgumentOutOfRangeException>(() => index.MaxItems = -1);
	}

	[Test, Description("Items deleted by CleanupCache are pruned from the index on load")]
	public void CleanupCachePrunesDeletedIndexEntries()
	{
		string groupId = "CleanupIndexTest";
		var repo = new DataRepo(Path.Combine(TestPath, "CleanupCacheIndex"), "Test");
		repo.DeleteRepo();

		var instance = repo.Open<int>(groupId, indexed: true);
		for (int i = 0; i < 3; i++)
		{
			instance.Save(Call, i.ToString(), i);
		}

		// Backdate the first item's data file so CleanupCache deletes it
		string dataPath = repo.GetDataPath(typeof(int), groupId, "0");
		string filePath = Path.Combine(dataPath, SerializerFileAtlas.DataFileName);
		File.SetLastWriteTimeUtc(filePath, DateTime.UtcNow - TimeSpan.FromDays(2));

		repo.CleanupCache(Call, TimeSpan.FromDays(1));

		var indices = instance.Index!.Load(Call);
		Assert.That(indices.Items.Select(item => item.Key), Is.EqualTo(new[] { "1", "2" }));

		DataItemCollection<int> allItems = instance.LoadAll(Call);
		Assert.That(allItems, Has.Exactly(2).Items);
		Assert.That(allItems[0].Value, Is.EqualTo(1));
		Assert.That(allItems[1].Value, Is.EqualTo(2));
	}

	// ─── Index writes ────────────────────────────────────────────────────

	/// <summary>Exposes the protected Save() so a failing write can be forced.</summary>
	private class TestIndex(DataRepoInstance<int> instance) : DataRepoIndex<int>(instance)
	{
		public void SaveIndices(Indices indices) => Save(indices);
	}

	[Test, Description(
		"The index was opened with FileMode.Create before anything was written, so a failure part " +
		"way through truncated the last valid one instead of leaving it alone")]
	public void FailedIndexSaveKeepsThePreviousIndex()
	{
		string groupId = "IndexSaveFailure";
		var repo = new DataRepo(Path.Combine(TestPath, "IndexSaveFailure"), "Test");
		repo.DeleteRepo();

		DataRepoInstance<int> instance = repo.Open<int>(groupId, indexed: true);
		instance.Save(Call, "first", 1);
		instance.Save(Call, "second", 2);

		var index = new TestIndex(instance);
		long originalLength = new FileInfo(index.PrimaryIndexPath).Length;
		Assert.That(originalLength, Is.GreaterThan(0), "The index has to exist to prove it survives.");

		// BinaryWriter.Write(string) throws for a null, standing in for a full disk
		DataRepoIndex<int>.Indices broken = new()
		{
			NextIndex = 3,
			Items = [new(0, null!)],
		};

		Assert.Catch(() => index.SaveIndices(broken), "A failed index write has to reach the caller.");

		Assert.That(new FileInfo(index.PrimaryIndexPath).Length, Is.EqualTo(originalLength),
			"The previous index is untouched.");

		Assert.That(Directory.GetFiles(instance.GroupPath, "*.sidx*"), Has.Length.EqualTo(1),
			"No temp index files left behind.");

		// Still loadable, with both keys in their original order
		DataRepoIndex<int>.Indices reloaded = new TestIndex(instance).Load(Call);
		Assert.That(reloaded.Items.Select(item => item.Key), Is.EqualTo(new[] { "first", "second" }));
	}
}
