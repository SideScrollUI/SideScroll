using NUnit.Framework;
using SideScroll.Serialize.Atlas;

namespace SideScroll.Serialize.Tests;

/// <summary>
/// Tests for types that have dedicated TypeRepo implementations beyond primitives and classes:
/// Enum, Type, Version, Guid, Uri, DateOnly, TimeOnly, decimal
/// </summary>
[Category("Serialize")]
public class SerializeSpecialTypesTests : SerializeBaseTest
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

	// --- Enum ---

	public enum MyEnum { a, b }

	public class EnumTest
	{
		public MyEnum TestEnum = MyEnum.a;
	}

	[Test, Description("Serialize Enum")]
	public void SerializeEnum()
	{
		var input = new EnumTest { TestEnum = MyEnum.b };

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

	// --- System.Type ---

	[Test, Description("Serialize Type")]
	public void SerializeType()
	{
		Type type = typeof(string);

		_serializer.Save(Call, type);
		Type output = _serializer.Load<Type>(Call);

		Assert.That(output, Is.EqualTo(type));
	}

	// --- Version ---

	[Test, Description("Serialize Version")]
	public void SerializeVersion()
	{
		var input = new Version("2.4");

		_serializer.Save(Call, input);
		var output = _serializer.Load<Version>(Call);

		Assert.That(output, Is.EqualTo(input));
	}

	// --- Guid ---

	public class GuidContainer
	{
		public Guid Id { get; set; }
		public string? Name { get; set; }
	}

	[Test, Description("Serialize Guid")]
	public void SerializeGuid()
	{
		var input = Guid.Parse("12345678-1234-1234-1234-123456789abc");

		_serializer.Save(Call, input);
		var output = _serializer.Load<Guid>(Call);

		Assert.That(output, Is.EqualTo(input));
	}

	[Test, Description("Serialize Guid in object")]
	public void SerializeGuidInObject()
	{
		var input = new GuidContainer
		{
			Id = Guid.Parse("12345678-1234-1234-1234-123456789abc"),
			Name = "test",
		};

		_serializer.Save(Call, input);
		var output = _serializer.Load<GuidContainer>(Call);

		Assert.That(output.Id, Is.EqualTo(input.Id));
		Assert.That(output.Name, Is.EqualTo(input.Name));
	}

	// --- Uri ---

	[Test, Description("Serialize Uri")]
	public void SerializeUri()
	{
		var input = new Uri("https://example.com/path?query=value");

		_serializer.Save(Call, input);
		var output = _serializer.Load<Uri>(Call);

		Assert.That(output, Is.EqualTo(input));
	}

	[Test, Description("Serialize relative Uri")]
	public void SerializeRelativeUri()
	{
		var input = new Uri("/relative/path", UriKind.Relative);

		_serializer.Save(Call, input);
		var output = _serializer.Load<Uri>(Call);

		Assert.That(output.OriginalString, Is.EqualTo(input.OriginalString));
	}

	// --- DateOnly ---

	[Test, Description("Serialize DateOnly")]
	public void SerializeDateOnly()
	{
		var input = new DateOnly(2024, 6, 15);

		_serializer.Save(Call, input);
		var output = _serializer.Load<DateOnly>(Call);

		Assert.That(output, Is.EqualTo(input));
	}

	// --- TimeOnly ---

	[Test, Description("Serialize TimeOnly")]
	public void SerializeTimeOnly()
	{
		var input = new TimeOnly(13, 45, 30, 500);

		_serializer.Save(Call, input);
		var output = _serializer.Load<TimeOnly>(Call);

		Assert.That(output, Is.EqualTo(input));
	}

	// --- decimal ---

	[Test, Description("Serialize decimal")]
	public void SerializeDecimal()
	{
		decimal input = 1.23M;

		_serializer.Save(Call, input);
		decimal output = _serializer.Load<decimal>(Call);

		Assert.That(output, Is.EqualTo(input));
	}

	[Test, Description("Serialize decimal with string array")]
	public void SerializeDecimalWithStringArray()
	{
		var input = new DecimalWithStringArray
		{
			Value = 4m,
			Lines = ["line1", "line2"],
		};

		_serializer.Save(Call, input);
		var output = _serializer.Load<DecimalWithStringArray>(Call);

		Assert.That(output.Value, Is.EqualTo(input.Value));
		Assert.That(output.Lines, Has.Length.EqualTo(2));
	}

	[Test, Description("Serialize decimal with list")]
	public void SerializeDecimalWithList()
	{
		var input = new DecimalWithList
		{
			Value = 4m,
			Items = [new DecimalListItem { Id = 1 }],
		};

		_serializer.Save(Call, input);
		var output = _serializer.Load<DecimalWithList>(Call);

		Assert.That(output.Value, Is.EqualTo(input.Value));
		Assert.That(output.Items, Has.Count.EqualTo(1));
	}

	[Test, Description("Serialize decimal in inherited class")]
	public void SerializeDecimalInherited()
	{
		var input = new DerivedDecimalClass("test", 4.5m);

		_serializer.Save(Call, input);
		var output = _serializer.Load<DerivedDecimalClass>(Call);

		Assert.That(output.Name, Is.EqualTo(input.Name));
		Assert.That(output.Value, Is.EqualTo(input.Value));
		Assert.That(output.BaseValue, Is.EqualTo(input.BaseValue));
	}

	public class DecimalWithStringArray
	{
		public decimal Value { get; set; }
		public string[]? Lines { get; set; }
	}

	public class DecimalListItem
	{
		public int Id { get; set; }
	}

	public class DecimalWithList
	{
		public decimal Value { get; set; }
		public List<DecimalListItem> Items { get; set; } = [];
	}

	public class BaseDecimalClass(string name)
	{
		public string Name { get; set; } = name;
		public decimal BaseValue { get; set; } = 1.5m;
	}

	public class DerivedDecimalClass(string name, decimal value) : BaseDecimalClass(name)
	{
		public decimal Value { get; set; } = value;
	}
}
