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

	private class OrderByItem
	{
		public int Value { get; set; }
		public int Field;
	}

	private static DataItemCollection<OrderByItem> OrderByItems() =>
	[
		new("b", new OrderByItem { Value = 2 }),
		new("a", new OrderByItem { Value = 1 }),
	];

	[Test, Description(
		"A missing property used to be null forgiven into the ordering lambda, so it surfaced as a " +
		"NullReferenceException thrown later from inside OrderBy() naming neither the type nor the member")]
	public void OrderByReportsAnUnknownMemberName()
	{
		DataItemCollection<OrderByItem> items = OrderByItems();

		var ascending = Assert.Throws<ArgumentException>(() => items.OrderBy("Missing").ToList());
		Assert.That(ascending!.Message, Does.Contain("OrderByItem").And.Contains("Missing"));

		Assert.Throws<ArgumentException>(() => items.OrderByDescending("Missing").ToList());
	}

	[Test, Description("GetProperty() only finds properties, so a field name fails the same way")]
	public void OrderByReportsAFieldName()
	{
		DataItemCollection<OrderByItem> items = OrderByItems();

		Assert.Throws<ArgumentException>(() => items.OrderBy(nameof(OrderByItem.Field)).ToList());
	}

	[Test, Description("Control: a real property still orders both ways")]
	public void OrderByUsesAKnownProperty()
	{
		DataItemCollection<OrderByItem> items = OrderByItems();

		Assert.That(items.OrderBy(nameof(OrderByItem.Value)).Select(i => i.Key), Is.EqualTo(new[] { "a", "b" }));
		Assert.That(items.OrderByDescending(nameof(OrderByItem.Value)).Select(i => i.Key), Is.EqualTo(new[] { "b", "a" }));
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
		Assert.That(pageView.GetPage(Call), Has.Exactly(1).Items);
	}

	[Test, Description(
		"ModifiedUtc is a grid column, so it stays cached rather than putting a stat syscall on the " +
		"render path for every visible row. Refresh() is how a caller opts into the current state")]
	public void DataItemModifiedUtcIsCachedUntilRefreshed()
	{
		string path = Path.Combine(TestPath, "ModifiedUtc", Path.GetRandomFileName());
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		File.WriteAllText(path, "first");

		var dataItem = new DataItem<int>("key", 1, path);
		DateTime? first = dataItem.ModifiedUtc;
		Assert.That(first, Is.Not.Null);

		File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddHours(1));

		Assert.That(dataItem.ModifiedUtc, Is.EqualTo(first), "Repeated reads don't touch the file.");

		dataItem.Refresh();
		Assert.That(dataItem.ModifiedUtc, Is.Not.EqualTo(first));
	}

	[Test, Description("A path that never existed still reports no modified time")]
	public void DataItemModifiedUtcIsNullWithoutAFile()
	{
		var dataItem = new DataItem<int>("key", 1, Path.Combine(TestPath, "missing-" + Path.GetRandomFileName()));

		Assert.That(dataItem.ModifiedUtc, Is.Null);
	}

	// 3 items at a page size of 2 gives 2 pages, so the last index is 1
	private DataPageView<int> PageView(bool indexed, int itemCount = 3, int pageSize = 2)
	{
		DataRepoInstance<int> instance = OpenRepo(indexed);
		for (int i = 0; i < itemCount; i++)
		{
			instance.Save(Call, i.ToString(), i);
		}

		DataPageView<int> pageView = instance.LoadPageView(Call);
		pageView.PageSize = pageSize;
		return pageView;
	}

	[TestCase(false)]
	[TestCase(true)]
	[Description("Nothing is loaded until a page is asked for, so the index starts as null rather than -1")]
	public void DataPageViewStartsWithNoPageIndex(bool indexed)
	{
		DataPageView<int> pageView = PageView(indexed);

		Assert.That(pageView.PageIndex, Is.Null);
		Assert.That(pageView.HasPrevious, Is.False);
	}

	[TestCase(false)]
	[TestCase(true)]
	[Description(
		"PageCount used to count the paths for an indexed instance while GetPage() paged the index, " +
		"and both only after GetPage() had populated them, so HasNext was false on a loaded repository")]
	public void DataPageViewCountsItemsBeforeAnyPageIsLoaded(bool indexed)
	{
		DataPageView<int> pageView = PageView(indexed);

		Assert.That(pageView.ItemCount, Is.EqualTo(3));
		Assert.That(pageView.PageCount, Is.EqualTo(2));
		Assert.That(pageView.HasNext, Is.True);
	}

	[TestCase(false)]
	[TestCase(true)]
	[Description("GetPage() loads the first page, which Next() only did because the index started below zero")]
	public void DataPageViewGetPageLoadsTheFirstPage(bool indexed)
	{
		DataPageView<int> pageView = PageView(indexed);

		Assert.That(pageView.GetPage(Call), Has.Exactly(2).Items);
		Assert.That(pageView.PageIndex, Is.EqualTo(0));

		// A second call stays put rather than advancing
		Assert.That(pageView.GetPage(Call), Has.Exactly(2).Items);
		Assert.That(pageView.PageIndex, Is.EqualTo(0));
	}

	[TestCase(false)]
	[TestCase(true)]
	[Description("Next() and Previous() from an unset index both land on the first page")]
	public void DataPageViewNavigatesFromNoPageIndex(bool indexed)
	{
		Assert.That(PageView(indexed).Next(Call), Has.Exactly(2).Items);
		Assert.That(PageView(indexed).Previous(Call), Has.Exactly(2).Items);
	}

	[TestCase(false)]
	[TestCase(true)]
	public void DataPageViewRejectsANegativePageIndex(bool indexed)
	{
		DataPageView<int> pageView = PageView(indexed);

		ArgumentOutOfRangeException propertyException = Assert.Throws<ArgumentOutOfRangeException>(
			() => pageView.PageIndex = -1)!;
		Assert.That(propertyException.ParamName, Is.EqualTo(nameof(DataPageView<int>.PageIndex)));

		ArgumentOutOfRangeException pageException = Assert.Throws<ArgumentOutOfRangeException>(
			() => pageView.GetPage(-1, Call))!;
		Assert.That(pageException.ParamName, Is.EqualTo("page"));
	}

	[TestCase(false)]
	[TestCase(true)]
	[Description("HasNext compared PageIndex + 1, which overflows to int.MinValue and reported a page that isn't there")]
	public void DataPageViewHandlesAnOverflowingPageIndex(bool indexed)
	{
		DataPageView<int> pageView = PageView(indexed);
		pageView.PageIndex = int.MaxValue;

		Assert.That(pageView.HasNext, Is.False);
		Assert.That(pageView.GetPage(int.MaxValue, Call), Is.Empty);

		// Steps back to the last page rather than incrementing past int.MaxValue
		Assert.That(pageView.Next(Call), Has.Exactly(1).Items);
		Assert.That(pageView.PageIndex, Is.EqualTo(1));
	}

	[TestCase(false)]
	[TestCase(true)]
	[Description("Ascending is mutable, and keeping the cached paths left unindexed pages in the old order")]
	public void DataPageViewReversingDirectionReordersPages(bool indexed)
	{
		DataPageView<int> pageView = PageView(indexed, itemCount: 4, pageSize: 2);

		List<int> ascending = [.. pageView.GetPage(Call).Select(i => i.Value)];

		pageView.Ascending = false;
		List<int> descending = [.. pageView.GetPage(Call).Select(i => i.Value)];

		Assert.That(descending, Is.Not.EqualTo(ascending));
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

	// Writes an index header with the given entry count and no entries after it
	private static void WriteIndexCount(TestIndex index, int count)
	{
		using var stream = File.Create(index.PrimaryIndexPath);
		using var writer = new BinaryWriter(stream);
		writer.Write(count);
		writer.Write(0L);
	}

	// 0 entries fit after a 12 byte header, so any positive count is impossible
	[TestCase(-1)]
	[TestCase(1)]
	[TestCase(int.MaxValue)]
	[Description(
		"The index is derived from the data directories, so a corrupt count rebuilds from the headers " +
		"rather than throwing. Throwing bricks a repository that BuildIndices() can reconstruct, and " +
		"Load() is reached through property getters that bindings evaluate")]
	public void IndexLoadRebuildsFromAnInvalidEntryCount(int count)
	{
		string groupId = $"InvalidIndexCount_{count}";
		DataRepoInstance<int> instance = _dataRepo.Open<int>(groupId, true);
		instance.DeleteAll(Call);
		for (int i = 0; i < 3; i++)
		{
			instance.Save(Call, i.ToString(), i);
		}

		var index = new TestIndex(instance);
		try
		{
			WriteIndexCount(index, count);

			DataRepoIndex<int>.Indices indices = index.Load(Call);

			Assert.That(indices.Items, Has.Count.EqualTo(3), "Rebuilt from the data headers.");
			Assert.That(Call.Log.EntriesText(), Does.Contain("Rebuilding an unreadable repository index"));
		}
		finally
		{
			File.Delete(index.PrimaryIndexPath);
		}
	}

	[Test, Description("Control: a valid index is still read from the file rather than rebuilt")]
	public void IndexLoadReadsAValidIndex()
	{
		DataRepoInstance<int> instance = _dataRepo.Open<int>("ValidIndexCount", true);
		instance.DeleteAll(Call);
		for (int i = 0; i < 3; i++)
		{
			instance.Save(Call, i.ToString(), i);
		}

		DataRepoIndex<int>.Indices indices = new TestIndex(instance).Load(Call);

		Assert.That(indices.Items, Has.Count.EqualTo(3));
		Assert.That(Call.Log.EntriesText(), Does.Not.Contain("Rebuilding an unreadable repository index"));
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
