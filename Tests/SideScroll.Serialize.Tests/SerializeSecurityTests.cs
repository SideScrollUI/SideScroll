using NUnit.Framework;
using SideScroll.Attributes;
using SideScroll.Serialize.Atlas;

namespace SideScroll.Serialize.Tests;

[Category("Serialize")]
public class SerializeSecurityTests : SerializeBaseTest
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

		Assert.That(output.NonSerialized, Is.EqualTo(1));
		Assert.That(output.Serialized, Is.EqualTo(10));
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

		Assert.That(output.UnserializedField, Is.Not.Null);
		Assert.That(output.UnserializedProperty, Is.Not.Null);

		Assert.That(output.UnserializedField.Value, Is.EqualTo(1));
		Assert.That(output.UnserializedProperty.Value, Is.EqualTo(1));
	}
}
