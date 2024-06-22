using SideScroll;
using NUnit.Framework;

namespace SideScroll.Serialize.Test;

[Category("Serialize")]
public class TestSerializeSecurity : TestSerializeBase
{
	private SerializerMemory _serializer = new SerializerMemorySideScroll();

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Serialize");
	}

	[SetUp]
	public void Setup()
	{
		_serializer = new SerializerMemorySideScroll();
	}

	public class NonSerializedTest
	{
		[NonSerialized]
		public int NonSerialized = 1;
		public int Serialized = 2;
	}

	[Test, Description("Serialize Attribute NonSerialized")]
	public void SerializeAttributeNonSerialized()
	{
		var input = new NonSerializedTest
		{
			NonSerialized = 5,
			Serialized = 10,
		};

		_serializer.Save(Call, input);
		var output = _serializer.Load<NonSerializedTest>(Call);

		Assert.AreEqual(1, output.NonSerialized);
		Assert.AreEqual(10, output.Serialized);
	}

	[Unserialized]
	public class UnserializedClass
	{
		public int Value = 1;
	}

	public class UnserializedPropertyClass
	{
		public UnserializedClass UnserializedField = new();
		public UnserializedClass UnserializedProperty { get; set; } = new();
	}

	[Test, Description("Serialize Field and Property with [Unserialized] classes")]
	public void SerializeUnserializedPropertyClass()
	{
		var input = new UnserializedPropertyClass();
		input.UnserializedField.Value = 42;
		input.UnserializedProperty.Value = 42;

		_serializer.Save(Call, input);
		var output = _serializer.Load<UnserializedPropertyClass>(Call);

		Assert.IsNotNull(output.UnserializedField);
		Assert.IsNotNull(output.UnserializedProperty);

		Assert.AreEqual(1, output.UnserializedField.Value);
		Assert.AreEqual(1, output.UnserializedProperty.Value);
	}
}
