using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Atlas.Serialize.Test
{
	[Category("Serialize")]
	public class TestSerializeCollections : TestSerializeBase
	{
		private SerializerMemory _serializer;

		[OneTimeSetUp]
		public void BaseSetup()
		{
			Initialize("SerializeCollections");
		}

		[SetUp]
		public void Setup()
		{
			_serializer = new SerializerMemoryAtlas();
		}

		[Test, Description("Serialize Array")]
		public void SerializeArray()
		{
			int[] input = { 1, 2 };
			input[0] = 5;

			_serializer.Save(Call, input);
			int[] output = _serializer.Load<int[]>(Call);

			Assert.AreEqual(2, output.Length);
			Assert.AreEqual(5, output[0]);
			Assert.AreEqual(2, output[1]);
		}

		[Test, Description("Serialize Byte Array")]
		public void SerializeByteArray()
		{
			byte[] input = new byte[1000];
			for (int i = 0; i < input.Length; i++)
			{
				input[i] = 1;
			}
			_serializer.Save(Call, input);
			byte[] output = _serializer.Load<byte[]>(Call);
			Assert.NotNull(output);
		}

		private class MultipleArrays
		{
			public int[] array1 = { 1, 2 };
			//public int[] array2 = { 3, 4 };
		}

		[Test, Description("ArrayMultipleTest")]
		public void ArrayMultipleTest()
		{
			var arrays = new MultipleArrays();
			_serializer.Save(Call, arrays);
			var output = _serializer.Load<MultipleArrays>(Call);
			Assert.NotNull(output);
		}


		[Test, Description("ArrayTest")]
		public void ArrayTest()
		{
			int[] array1 = { };
			int[] array2 = { };

			var idxObjectToIndex = new Dictionary<object, int>(); // for saving, not filled in for loading
			idxObjectToIndex[array1] = idxObjectToIndex.Count;

			if (idxObjectToIndex.ContainsKey(array2))
			{
				Debug.Assert(true);
			}
			else
			{
				idxObjectToIndex[array2] = idxObjectToIndex.Count;
			}
		}

		[Test, Description("Serialize String List")]
		public void SerializeStringList()
		{
			var input = new List<string>()
			{
				"abc",
				"123"
			};

			_serializer.Save(Call, input);
			var output = _serializer.Load<List<string>>(Call);

			Assert.AreEqual(input[0], output[0]);
			Assert.AreEqual(input[1], output[1]);
		}

		[Test, Description("Serialize Nullable List")]
		public void SerializeNullableList()
		{
			var input = new List<int?>
			{
				null,
				1,
				null,
				2
			};

			_serializer.Save(Call, input);
			var output = _serializer.Load<List<int?>>(Call);
			Assert.NotNull(output);
		}

		[Test, Description("Serialize Type Dictionary")]
		public void SerializeTypeDictionary()
		{
			var input = new Dictionary<Type, string>
			{
				[typeof(int)] = "integer"
			};

			_serializer.Save(Call, input);
			var output = _serializer.Load<Dictionary<Type, string>>(Call);

			Assert.IsTrue(output.ContainsKey(typeof(int)));
			Assert.IsTrue(output.ContainsValue("integer"));
		}

		[Test, Description("Serialize String Dictionary")]
		public void SerializeStringDictionary()
		{
			var input = new Dictionary<string, string>
			{
				["a"] = "1",
				["b"] = "2"
			};

			_serializer.Save(Call, input);
			var output = _serializer.Load<Dictionary<string, string>>(Call);

			Assert.AreEqual(input["a"], output["a"]);
			Assert.AreEqual(input["b"], output["b"]);
		}

		[Test, Description("Serialize Dictionary Containing Subclass of Type")]
		public void SerializeDictionaryOfObjects()
		{
			var input = new Dictionary<string, object>
			{
				{ "default", true }
			};

			_serializer.Save(Call, input);
			var output = _serializer.Load<Dictionary<string, object>>(Call);

			Assert.AreEqual(true, output["default"]);
		}

		[Test, Description("Serialize HashSet")]
		public void SerializeHashSet()
		{
			var input = new HashSet<string>
			{
				"test",
			};

			_serializer.Save(Call, input);
			var output = _serializer.Load<HashSet<string>>(Call);

			Assert.AreEqual(input.Count, output.Count);
			Assert.True(output.Contains("test"));
		}

		public class SelectedItem
		{
			public string Label;
			public bool Pinned;
		}

		public class TabInstanceConfiguration
		{
			public HashSet<SelectedItem> selected = new HashSet<SelectedItem>();
			public int? SplitterDistance;
			public int NumColumns;
		}

		[Test, Description("Serialize HashSet")]
		public void SerializeHashSetObject()
		{
			var input = new HashSet<SelectedItem>();
			var inputItem = new SelectedItem()
			{
				Label = "abc",
				Pinned = true,
			};
			input.Add(inputItem);

			_serializer.Save(Call, input);
			var output = _serializer.Load<HashSet<SelectedItem>>(Call);

			Assert.AreEqual(input.Count, output.Count);
			//Assert.True(output.Contains("test"));
		}
	}
}
