using SideScroll.Serialize.DataRepos;
using NUnit.Framework;

namespace SideScroll.Serialize.Test;

[Category("Serialize")]
public class TestDataRepo : TestSerializeBase
{
	private DataRepo _dataRepo = new(TestPath, "Test");

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("TestDataRepo");
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

	[Test, Description("Serialize int Save Load")]
	public void SerializeInt()
	{
		string keyId = "int";
		int input = 1;
		_dataRepo.Save(keyId, input, Call);
		int output = _dataRepo.Load<int>(keyId, Call);

		Assert.AreEqual(input, output);
	}

	[Test, Description("DataInstance int Save Load")]
	public void TestDataInstanceInt()
	{
		string keyId = "int";
		int input = 1;
		var instance = OpenRepo();
		instance.Save(Call, keyId, input);

		int output = instance.Load(Call, keyId);
		Assert.AreEqual(input, output);
	}

	[Test, Description("DataInstance Paging")]
	public void TestDataInstancePaging()
	{
		int pageSize = 2;
		var instance = OpenRepo();
		for (int i = 0; i < 5; i++)
		{
			instance.Save(Call, i.ToString(), i);
		}

		var pageView = instance.LoadPageView(Call, true)!;
		pageView.PageSize = pageSize;

		// Order is unknown without indexing
		var page1 = pageView.Next(Call).ToList();
		Assert.AreEqual(pageSize, page1.Count);

		var page2 = pageView.Next(Call).ToList();
		Assert.AreEqual(pageSize, page2.Count);
	}

	[Test, Description("DataInstance Index Paging")]
	public void TestDataInstancePagingIndex()
	{
		int pageSize = 2;
		var instance = OpenRepo(true);
		for (int i = 0; i < 5; i++)
		{
			instance.Save(Call, i.ToString(), i);
		}

		DataPageView<int> pageView = instance.LoadPageView(Call, true)!;
		pageView.PageSize = pageSize;

		var page1 = pageView.Next(Call).ToList();
		Assert.AreEqual(pageSize, page1.Count);
		Assert.AreEqual(0, page1[0].Value);
		Assert.AreEqual(1, page1[1].Value);

		var page2 = pageView.Next(Call).ToList();
		Assert.AreEqual(pageSize, page2.Count);
		Assert.AreEqual(2, page2[0].Value);
		Assert.AreEqual(3, page2[1].Value);
	}

	[Test, Description("DataInstance Index Replace")]
	public void TestDataInstancePagingReplace()
	{
		var instance = OpenRepo(true);

		int input = 1;
		instance.Save(Call, input.ToString(), input);
		instance.Save(Call, input.ToString(), input);

		DataItemCollection<int> loaded = instance.LoadAll(Call);
		Assert.AreEqual(1, loaded.Count);
		Assert.AreEqual(1, loaded[0].Value);
	}

	[Test, Description("DataInstance Index MaxItems")]
	public void TestDataInstanceIndexMaxItems()
	{
		var instance = OpenRepo(true);
		instance.Index!.MaxItems = 2;
		for (int i = 0; i < 3; i++)
		{
			instance.Save(Call, i.ToString(), i);
		}

		DataItemCollection<int> allItems = instance.LoadAll(Call);

		Assert.AreEqual(2, allItems.Count);
		Assert.AreEqual(1, allItems[0].Value);
		Assert.AreEqual(2, allItems[1].Value);
	}
}
