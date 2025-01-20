using NUnit.Framework;
using SideScroll.Serialize.Atlas;
using System.Collections;
using System.Diagnostics;

namespace SideScroll.Serialize.Test;

[Category("Serialize")]
public class TestSerializeCollections : TestSerializeBase
{
	private SerializerMemory _serializer = new SerializerMemoryAtlas();

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
		int[] input = [1, 2];
		input[0] = 5;

		_serializer.Save(Call, input);
		int[] output = _serializer.Load<int[]>(Call);

		Assert.That(output, Has.Exactly(2).Items);
		Assert.That(output[0], Is.EqualTo(5));
		Assert.That(output[1], Is.EqualTo(2));
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
		Assert.That(output, Is.Not.Null);
	}

	private class MultipleArrays
	{
		public int[] Array1 = [1, 2];
		public int[] Array2 = [3, 4];
	}

	[Test, Description("Multiple Arrays")]
	public void ArrayMultipleTest()
	{
		var arrays = new MultipleArrays();
		_serializer.Save(Call, arrays);
		var output = _serializer.Load<MultipleArrays>(Call);
		Assert.That(output, Is.Not.Null);
	}

	[Test, Description("Dictionary Integer Array Object Key")]
	public void ArrayTest()
	{
		int[] array1 = [];
		int[] array2 = [];

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
		var input = new List<string>
		{
			"abc",
			"123"
		};

		_serializer.Save(Call, input);
		var output = _serializer.Load<List<string>>(Call);

		Assert.That(output[0], Is.EqualTo(input[0]));
		Assert.That(output[1], Is.EqualTo(input[1]));
	}

	public class StringList : List<string>;

	[Test, Description("Serialize String List Type")]
	public void SerializeStringListType()
	{
		var input = new StringList
		{
			"abc",
			"123"
		};

		_serializer.Save(Call, input);
		var output = _serializer.Load<StringList>(Call);

		Assert.That(output[0], Is.EqualTo(input[0]));
		Assert.That(output[1], Is.EqualTo(input[1]));
	}

	[Test, Description("Serialize IList Type")]
	public void SerializeIListType()
	{
		var input = new StringList
		{
			"abc",
			"123"
		};

		_serializer.Save(Call, input);

		var output = (StringList)_serializer.Load<IList>(Call);

		Assert.That(output[0], Is.EqualTo(input[0]));
		Assert.That(output[1], Is.EqualTo(input[1]));
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
		Assert.That(output, Is.Not.Null);
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

		Assert.That(output, Does.ContainKey(typeof(int)));
		Assert.That(output, Does.ContainValue("integer"));
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

		Assert.That(output["a"], Is.EqualTo(input["a"]));
		Assert.That(output["b"], Is.EqualTo(input["b"]));
	}

	public class StringDictionary : Dictionary<string, string>;

	[Test, Description("Serialize String Dictionary Type")]
	public void SerializeStringDictionaryType()
	{
		var input = new StringDictionary
		{
			["a"] = "1",
			["b"] = "2"
		};

		_serializer.Save(Call, input);
		var output = _serializer.Load<StringDictionary>(Call);

		Assert.That(output["a"], Is.EqualTo(input["a"]));
		Assert.That(output["b"], Is.EqualTo(input["b"]));
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

		Assert.That(output["default"], Is.True);
	}

	[Test, Description("Serialize HashSet")]
	public void SerializeHashSet()
	{
		var input = new HashSet<string>
		{
			"test",
			"test2",
		};

		_serializer.Save(Call, input);
		var output = _serializer.Load<HashSet<string>>(Call);

		Assert.That(output, Has.Exactly(input.Count).Items);
		Assert.That(output, Does.Contain("test"));
		Assert.That(output, Does.Contain("test2"));
	}

	public class SelectedLabel : IEquatable<SelectedLabel>
	{
		public string? Label;

		public override string? ToString() => Label;

		public SelectedLabel() { }

		public SelectedLabel(string label)
		{
			Label = label;
		}

		public override int GetHashCode()
		{
			return Label?.GetHashCode() ?? 0;
		}

		public override bool Equals(object? obj)
		{
			return Equals(obj as SelectedLabel);
		}

		public bool Equals(SelectedLabel? other)
		{
			return other != null && Label == other.Label;
		}
	}

	[Test, Description("Serialize HashSet Objects")]
	public void SerializeHashSetObjects()
	{
		var label1 = new SelectedLabel("1");
		var label2 = new SelectedLabel("2");

		var input = new HashSet<SelectedLabel>
		{
			label1,
			label2,
		};

		_serializer.Save(Call, input);
		var output = _serializer.Load<HashSet<SelectedLabel>>(Call);

		Assert.That(output, Has.Exactly(input.Count).Items);
		Assert.That(output, Does.Contain(label1));
		Assert.That(output, Does.Contain(label2));
	}

	public class SelectedItem
	{
		public string? Label;
		public bool Pinned;
	}

	[Test, Description("Serialize HashSet")]
	public void SerializeHashSetObject()
	{
		var input = new HashSet<SelectedItem>();
		var inputItem = new SelectedItem
		{
			Label = "abc",
			Pinned = true,
		};
		input.Add(inputItem);

		_serializer.Save(Call, input);
		var output = _serializer.Load<HashSet<SelectedItem>>(Call);

		Assert.That(output, Has.Exactly(input.Count).Items);
		//Assert.That(output, Does.Contain("test"));
	}
}
