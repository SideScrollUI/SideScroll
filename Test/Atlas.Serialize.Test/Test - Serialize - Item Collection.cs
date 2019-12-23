using System;
using System.Collections.Generic;
using System.IO;
using Atlas.Core;
using NUnit.Framework;

namespace Atlas.Serialize.Test
{
	[NUnit.Framework.Category("SerializeItemCollection")]
	public class TestItemCollection : TestSerializeBase
	{
		private SerializerMemory serializer;

		[OneTimeSetUp]
		public void BaseSetup()
		{
			Initialize("SerializeItemCollection");
		}

		[SetUp]
		public void Setup()
		{
			serializer = new SerializerMemory();
		}

		[Test, NUnit.Framework.Description("Serialize ItemCollection")]
		public void SerializeItemCollection()
		{
			TestBindingList input = new TestBindingList();

			serializer.Save(call, input);
			TestBindingList output = serializer.Load<TestBindingList>(call);

			//Assert.AreEqual(output.uintTest, input.uintTest);
		}

		public class ReferencedClass
		{
			public int intField = 1;
		}

		public class TestBindingList
		{
			public ItemCollection<int> intList = new ItemCollection<int>() { 1 };
			public ItemCollection<string> stringList = new ItemCollection<string>() { "abc" };
			public ItemCollection<ReferencedClass> refList = new ItemCollection<ReferencedClass>() { new ReferencedClass() };
		}
	}
}
/*

*/
