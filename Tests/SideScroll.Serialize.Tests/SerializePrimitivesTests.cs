using NUnit.Framework;
using SideScroll.Serialize.Atlas;

namespace SideScroll.Serialize.Tests;

[Category("Serialize")]
public class SerializePrimitivesTests : SerializeBaseTest
{
	private SerializerMemoryAtlas _serializer = new();

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Serialize");
	}

	[SetUp]
	public void Setup()
	{
		_serializer = new();
	}

	[Test, Description("Serialize int")]
	public void SerializeInt()
	{
		int input = 5;

		_serializer.Save(Call, input);
		int output = _serializer.Load<int>(Call);

		Assert.That(output, Is.EqualTo(input));
	}

	[Test, Description("Serialize uint")]
	public void SerializeUInt()
	{
		uint input = 5;

		_serializer.Save(Call, input);
		uint output = _serializer.Load<uint>(Call);

		Assert.That(output, Is.EqualTo(input));
	}

	[Test, Description("Serialize Nullable int")]
	public void SerializeNullableInt()
	{
		int? input = 1;

		_serializer.Save(Call, input);
		int? output = _serializer.Load<int?>(Call);

		Assert.That(output, Is.EqualTo(input));
	}

	[Test, Description("Serialize long")]
	public void SerializeLong()
	{
		long input = 5;

		_serializer.Save(Call, input);
		long output = _serializer.Load<long>(Call);

		Assert.That(output, Is.EqualTo(input));
	}

	[Test, Description("Serialize ulong")]
	public void SerializeULong()
	{
		ulong input = 5;

		_serializer.Save(Call, input);
		ulong output = _serializer.Load<ulong>(Call);

		Assert.That(output, Is.EqualTo(input));
	}

	[Test, Description("Serialize short")]
	public void SerializeShort()
	{
		short input = 5;

		_serializer.Save(Call, input);
		short output = _serializer.Load<short>(Call);

		Assert.That(output, Is.EqualTo(input));
	}

	[Test, Description("Serialize ushort")]
	public void SerializeUShort()
	{
		ushort input = 5;

		_serializer.Save(Call, input);
		ushort output = _serializer.Load<ushort>(Call);

		Assert.That(output, Is.EqualTo(input));
	}

	[Test, Description("Serialize byte")]
	public void SerializeByte()
	{
		byte input = 1;

		_serializer.Save(Call, input);
		byte output = _serializer.Load<byte>(Call);

		Assert.That(output, Is.EqualTo(input));
	}

	[Test, Description("Serialize sbyte")]
	public void SerializeSByte()
	{
		sbyte input = -1;

		_serializer.Save(Call, input);
		sbyte output = _serializer.Load<sbyte>(Call);

		Assert.That(output, Is.EqualTo(input));
	}

	[Test, Description("Serialize char")]
	public void SerializeChar()
	{
		char input = 'a';

		_serializer.Save(Call, input);
		char output = _serializer.Load<char>(Call);

		Assert.That(output, Is.EqualTo(input));
	}

	[Test, Description("Serialize float")]
	public void SerializeFloat()
	{
		float input = 1.2f;

		_serializer.Save(Call, input);
		float output = _serializer.Load<float>(Call);

		Assert.That(output, Is.EqualTo(input));
	}

	[Test, Description("Serialize double")]
	public void SerializeDouble()
	{
		double input = 1.2;

		_serializer.Save(Call, input);
		double output = _serializer.Load<double>(Call);

		Assert.That(output, Is.EqualTo(input));
	}
}
