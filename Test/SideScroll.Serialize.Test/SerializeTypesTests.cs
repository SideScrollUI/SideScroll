using NUnit.Framework;
using SideScroll.Serialize.Atlas;

namespace SideScroll.Serialize.Test;

[Category("Serialize")]
public class SerializeTypesTests : SerializeBaseTest
{
	private SerializerMemory _serializer = new SerializerMemoryAtlas();

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

	[Test, Description("Serialize Enum")]
	public void SerializeEnum()
	{
		var input = new EnumTest
		{
			TestEnum = MyEnum.b
		};

		_serializer.Save(Call, input);
		EnumTest output = _serializer.Load<EnumTest>(Call);

		Assert.That(output.TestEnum, Is.EqualTo(input.TestEnum));
	}

	[Test, Description("Serialize Nullable Enum")]
	public void SerializeNullableEnum()
	{
		MyEnum? input = MyEnum.b;

		_serializer.Save(Call, input);
		MyEnum? output = _serializer.Load<MyEnum?>(Call);

		Assert.That(output, Is.EqualTo(input));
	}

	[Test, Description("Serialize Type")]
	public void SerializeType()
	{
		Type type = typeof(string);

		_serializer.Save(Call, type);
		Type output = _serializer.Load<Type>(Call);

		Assert.That(output, Is.EqualTo(type));
	}

	public struct StructTest
	{
		public int value;
	}

	[Test, Description("Serialize Struct")]
	public void SerializeStruct()
	{
		var input = new StructTest
		{
			value = 5
		};

		_serializer.Save(Call, input);
		StructTest output = _serializer.Load<StructTest>(Call);

		Assert.That(output, Is.EqualTo(input));
	}

	[Test, Description("Serialize Objects")]
	public void SerializeObjects()
	{
		var input = new Objects();

		_serializer.Save(Call, input);
		var output = _serializer.Load<Objects>(Call);
		Assert.That(output, Is.Not.Null);
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

		Assert.That(output, Is.EqualTo(input));
	}

	[Test, Description("Serialize decimal")]
	public void SerializeDecimal()
	{
		decimal input = 1.23M;

		_serializer.Save(Call, input);
		decimal output = _serializer.Load<decimal>(Call);

		Assert.That(output, Is.EqualTo(input));
	}
}
