using NUnit.Framework;
using SideScroll.Serialize.DataRepos;

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

		Assert.That(output, Is.EqualTo(input));
	}

	[Test, Description("DataInstance int Save Load")]
	public void TestDataInstanceInt()
	{
		string keyId = "int";
		int input = 1;
		var instance = OpenRepo();
		instance.Save(Call, keyId, input);

		int output = instance.Load(Call, keyId);
		Assert.That(output, Is.EqualTo(input));
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
		Assert.That(page1, Has.Exactly(pageSize).Items);

		var page2 = pageView.Next(Call).ToList();
		Assert.That(page2, Has.Exactly(pageSize).Items);
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
		Assert.That(page1, Has.Exactly(pageSize).Items);
		Assert.That(page1[0].Value, Is.EqualTo(0));
		Assert.That(page1[1].Value, Is.EqualTo(1));

		var page2 = pageView.Next(Call).ToList();
		Assert.That(page2, Has.Exactly(pageSize).Items);
		Assert.That(page2[0].Value, Is.EqualTo(2));
		Assert.That(page2[1].Value, Is.EqualTo(3));
	}

	[Test, Description("DataInstance Index Replace")]
	public void TestDataInstancePagingReplace()
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
	public void TestDataInstanceIndexMaxItems()
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
}
