using NUnit.Framework;
using SideScroll.Collections;

namespace SideScroll.Serialize.Test;

[Category("SerializeItemCollection")]
public class TestItemCollection : TestSerializeBase
{
	private SerializerMemory _serializer = new SerializerMemorySideScroll();

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("SerializeItemCollection");
	}

	[SetUp]
	public void Setup()
	{
		_serializer = new SerializerMemorySideScroll();
	}

	[Test, Description("Serialize ItemCollection")]
	public void SerializeItemCollection()
	{
		var input = new TestBindingList();

		_serializer.Save(Call, input);
		TestBindingList output = _serializer.Load<TestBindingList>(Call);

		//Assert.AreEqual(input.uintTest, output.uintTest);
	}

	public class ReferencedClass
	{
		public int IntField = 1;
	}

	public class TestBindingList
	{
		public ItemCollection<int> IntList = [1];
		public ItemCollection<string> StringList = ["abc"];
		public ItemCollection<ReferencedClass> RefList = [new ReferencedClass()];
	}
}
