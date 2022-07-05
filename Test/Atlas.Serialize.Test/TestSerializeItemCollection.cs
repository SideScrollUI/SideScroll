using System;
using System.Collections.Generic;
using Atlas.Core;
using NUnit.Framework;

namespace Atlas.Serialize.Test;

[Category("SerializeItemCollection")]
public class TestItemCollection : TestSerializeBase
{
	private SerializerMemory _serializer = new SerializerMemoryAtlas();

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("SerializeItemCollection");
	}

	[SetUp]
	public void Setup()
	{
		_serializer = new SerializerMemoryAtlas();
	}

	[Test, Description("Serialize ItemCollection")]
	public void SerializeItemCollection()
	{
		var input = new TestBindingList();

		_serializer.Save(Call, input);
		TestBindingList output = _serializer.Load<TestBindingList>(Call);

		//Assert.AreEqual(output.uintTest, input.uintTest);
	}

	public class ReferencedClass
	{
		public int IntField = 1;
	}

	public class TestBindingList
	{
		public ItemCollection<int> IntList = new() { 1 };
		public ItemCollection<string> StringList = new() { "abc" };
		public ItemCollection<ReferencedClass> RefList = new() { new ReferencedClass() };
	}
}
