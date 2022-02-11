using NUnit.Framework;
using System;

namespace Atlas.Serialize.Test;

[Category("Serialize")]
public class SerializeTypes : TestSerializeBase
{
	private SerializerMemory _serializer;

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Serialize");
	}

	[SetUp]
	public void Setup()
	{
		_serializer = new SerializerMemoryAtlas();
	}

	[Test, Description("Serialize int")]
	public void SerializeInt()
	{
		int input = 5;

		_serializer.Save(Call, input);
		int output = _serializer.Load<int>(Call);

		Assert.AreEqual(input, output);
	}

	[Test, Description("Serialize Nullable int")]
	public void SerializeNullableInt()
	{
		int? input = 1;

		_serializer.Save(Call, input);
		int? output = _serializer.Load<int?>(Call);

		Assert.AreEqual(output, input);
	}

	[Test, Description("Serialize Enum")]
	public void SerializeEnum()
	{
		var input = new EnumTest
		{
			TestEnum = MyEnum.b
		};

		_serializer.Save(Call, input);
		EnumTest output = _serializer.Load<EnumTest>(Call);

		Assert.AreEqual(output.TestEnum, input.TestEnum);
	}

	[Test, Description("Serialize Nullable Enum")]
	public void SerializeNullableEnum()
	{
		MyEnum? input = MyEnum.b;

		_serializer.Save(Call, input);
		MyEnum? output = _serializer.Load<MyEnum?>(Call);

		Assert.AreEqual(output, input);
	}

	[Test, Description("Serialize Type")]
	public void SerializeType()
	{
		Type type = typeof(string);

		_serializer.Save(Call, type);
		Type output = _serializer.Load<Type>(Call);

		Assert.AreEqual(type, output);
	}

	public struct StructTest
	{
		public int value;
	}

	[Test, Description("Serialize Struct")]
	public void SerializeStruct()
	{
		var input = new StructTest()
		{
			value = 5
		};

		_serializer.Save(Call, input);
		StructTest output = _serializer.Load<StructTest>(Call);

		Assert.AreEqual(input, output);
	}

	[Test, Description("Serialize Objects")]
	public void SerializeObjects()
	{
		var input = new Objects();

		_serializer.Save(Call, input);
		var output = _serializer.Load<Objects>(Call);
		Assert.NotNull(output);
	}

	public class Objects
	{
		public object Object = 2;
	}

	public enum MyEnum
	{
		a,
		b
	}

	public class EnumTest
	{
		public MyEnum TestEnum = MyEnum.a;
	}

	[Test, Description("Serialize Version")]
	public void SerializeVersion()
	{
		var input = new Version("2.4");

		_serializer.Save(Call, input);
		var output = _serializer.Load<Version>(Call);

		Assert.AreEqual(input, output);
	}
}
