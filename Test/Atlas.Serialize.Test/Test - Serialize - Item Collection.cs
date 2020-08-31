﻿using System;
using System.Collections.Generic;
using Atlas.Core;
using NUnit.Framework;

namespace Atlas.Serialize.Test
{
	[Category("SerializeItemCollection")]
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

		[Test, Description("Serialize ItemCollection")]
		public void SerializeItemCollection()
		{
			var input = new TestBindingList();

			serializer.Save(Call, input);
			TestBindingList output = serializer.Load<TestBindingList>(Call);

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
