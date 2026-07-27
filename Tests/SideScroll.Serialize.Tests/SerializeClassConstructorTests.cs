using NUnit.Framework;
using SideScroll.Serialize.Atlas.Schema;
using SideScroll.Attributes;
using SideScroll.Serialize.Atlas;
using System.Globalization;

namespace SideScroll.Serialize.Tests;

[Category("Serialize")]
public class SerializeClassConstructorTests : SerializeBaseTest
{
	private SerializerMemory _serializer = new SerializerMemoryAtlas();

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("SerializeClassConstructor");
	}

	[SetUp]
	public void Setup()
	{
		_serializer = new SerializerMemoryAtlas();
	}

	public class NoConstructorBaseClass(int a)
	{
		public int A = a;

		[PrivateData]
		public int B = 0;
	}

	public class DerivedClassWithConstructor : NoConstructorBaseClass
	{
		public DerivedClassWithConstructor() : base(0)
		{
		}

		public DerivedClassWithConstructor(int a) : base(a)
		{
		}
	}

	public class DerivedClassWithConstructorReference
	{
		public NoConstructorBaseClass? BaseClass;
	}

	[Test, Description("Serialize No Default Constructor Base Class")]
	public void SerializeNoDefaultConstructorBaseClass()
	{
		var input = new DerivedClassWithConstructor();

		_serializer.Save(Call, input);
		var output = _serializer.Load<NoConstructorBaseClass>(Call);

		Assert.That(output.B, Is.EqualTo(input.B));
	}

	[Test, Description("Serialize No Default Constructor Base Class Reference")]
	public void SerializeNoDefaultConstructorBaseClassReference()
	{
		var input = new DerivedClassWithConstructorReference
		{
			BaseClass = new DerivedClassWithConstructor(1),
		};

		_serializer.Save(Call, input);
		var output = _serializer.Load<DerivedClassWithConstructorReference>(Call);

		Assert.That(output.BaseClass!.B, Is.EqualTo(input.BaseClass.B));
	}

	public class ProtectedConstructorBaseClass
	{
		protected ProtectedConstructorBaseClass(int a)
		{
			A = a;
		}

		public ProtectedConstructorBaseClass(int a, int b)
		{
			A = a;
			B = b;
		}

		public int A;

		[Unserialized]
		public int B;
	}

	public class DerivedClassWithProtectedConstructor : ProtectedConstructorBaseClass
	{
		public DerivedClassWithProtectedConstructor() : base(0)
		{
		}

		public DerivedClassWithProtectedConstructor(int a) : base(a)
		{
		}
	}

	[Test, Description("Serialize Protected Constructor Base Class")]
	public void SerializeProtectedConstructorBaseClass()
	{
		var input = new DerivedClassWithProtectedConstructor(2);
		var inputList = new List<ProtectedConstructorBaseClass> { input };

		_serializer.Save(Call, inputList);
		var output = _serializer.Load<List<ProtectedConstructorBaseClass>>(Call);

		Assert.That(output[0].B, Is.EqualTo(input.B));
	}

	public record CustomConstructorFieldClass
	{
		public int A = 1;

		public CustomConstructorFieldClass(int a)
		{
			A = a;
		}
	}

	public record CustomConstructorPropertyClass
	{
		public int A { get; set; } = 1;

		public CustomConstructorPropertyClass(int a)
		{
			A = a;
		}
	}

	[Test, Description("Serialize Custom Constructor Field Class")]
	public void SerializeCustomConstructorFieldClass()
	{
		var input = new CustomConstructorFieldClass(5);

		_serializer.Save(Call, input);
		var output = _serializer.Load<CustomConstructorFieldClass>(Call);

		Assert.That(output.A, Is.EqualTo(input.A));
	}

	[Test, Description("Serialize Custom Constructor Property Class")]
	public void SerializeCustomConstructorPropertyClass()
	{
		var input = new CustomConstructorPropertyClass(5);

		_serializer.Save(Call, input);
		var output = _serializer.Load<CustomConstructorPropertyClass>(Call);

		Assert.That(output.A, Is.EqualTo(input.A));
	}

	[Test, Description("Serialize Custom Constructor List Field Class")]
	public void SerializeCustomConstructorListFieldClass()
	{
		var item = new CustomConstructorFieldClass(5);
		var input = new List<CustomConstructorFieldClass> { item };

		_serializer.Save(Call, input);
		var output = _serializer.Load<List<CustomConstructorFieldClass>>(Call);

		Assert.That(output, Is.EqualTo(input));
	}

	public record CustomConstructorReadOnlyPropertyClass
	{
		public int A { get; } = 1;

		public CustomConstructorReadOnlyPropertyClass(int a)
		{
			A = a;
		}
	}

	[Test, Description("Serialize Custom Constructor Read Only Property Class")]
	public void SerializeCustomConstructorReadOnlyPropertyClass()
	{
		var input = new CustomConstructorReadOnlyPropertyClass(5);

		_serializer.Save(Call, input);
		var output = _serializer.Load<CustomConstructorReadOnlyPropertyClass>(Call);

		Assert.That(output.A, Is.EqualTo(input.A));
	}

	public record CustomConstructorReadOnlyStringPropertyClass
	{
		public string A { get; } = "abc";

		public CustomConstructorReadOnlyStringPropertyClass(string a)
		{
			A = a;
		}
	}

	[Test, Description("Serialize Custom Constructor Read Only Property Class")]
	public void SerializeCustomConstructorReadOnlyStringPropertyClass()
	{
		var input = new CustomConstructorReadOnlyStringPropertyClass("123");

		_serializer.Save(Call, input);
		var output = _serializer.Load<CustomConstructorReadOnlyStringPropertyClass>(Call);

		Assert.That(output.A, Is.EqualTo(input.A));
	}

	public class CustomConstructorWithNullableParam
	{
		public string A { get; } = "abc";

		public CustomConstructorWithNullableParam(string a, int? b = null)
		{
			A = a;
		}
	}

	[Test, Description("Serialize Custom Constructor with Nullable Param")]
	public void SerializeCustomConstructorWithNullableParam()
	{
		var input = new CustomConstructorWithNullableParam("123");

		_serializer.Save(Call, input);
		var output = _serializer.Load<CustomConstructorWithNullableParam>(Call);

		Assert.That(output, Is.Not.Null);
		Assert.That(output.A, Is.EqualTo(input.A));
	}

	public class PublicEmptyConstructor
	{
		public string? A { get; set; }
	}

	public class NonPublicEmptyConstructor
	{
		public string? A { get; set; }

		private NonPublicEmptyConstructor() { }

		public NonPublicEmptyConstructor(string a) { A = a; }
	}

	public class NonPublicParamConstructorOnly
	{
		public string? A { get; set; }

		private NonPublicParamConstructorOnly(string a) { A = a; }
	}

	public struct StructWithParamConstructor
	{
		public string A { get; }

		public StructWithParamConstructor(string a) { A = a; }
	}

	public struct StructWithNoConstructor
	{
		public string? A { get; set; }
	}

	[Test, Description("A non public parameterless constructor still counts, Activator.CreateInstance(type, true) can use it")]
	public void TypeHasEmptyConstructorIncludesNonPublic()
	{
		Assert.That(TypeSchema.TypeHasEmptyConstructor(typeof(PublicEmptyConstructor)), Is.True);
		Assert.That(TypeSchema.TypeHasEmptyConstructor(typeof(NonPublicEmptyConstructor)), Is.True);
		Assert.That(TypeSchema.TypeHasEmptyConstructor(typeof(StructWithNoConstructor)), Is.True);
	}

	[Test, Description("Types whose constructors all take parameters need the custom constructor path")]
	public void TypeHasEmptyConstructorExcludesParameterizedOnly()
	{
		Assert.That(TypeSchema.TypeHasEmptyConstructor(typeof(NoConstructorBaseClass)), Is.False);

		// Was reported as having one because its only constructor isn't public
		Assert.That(TypeSchema.TypeHasEmptyConstructor(typeof(NonPublicParamConstructorOnly)), Is.False);

		// Declaring one means read only members have to come from it, like Avalonia's Color
		Assert.That(TypeSchema.TypeHasEmptyConstructor(typeof(StructWithParamConstructor)), Is.False);
	}

	// ─── Culture ─────────────────────────────────────────────────────────

	// "Id" is the discriminator: tr-TR lowercases 'I' to the dotless 'ı', so a ToLower() based
	// match turns the member into "ıd" while the parameter "id" stays "id". "Title" still matches
	// either way, since a lowercase 'i' is unchanged
	public record ItemWithId
	{
		public int Id { get; } = 0;
		public string Title { get; } = "";

		public ItemWithId(int id, string title)
		{
			Id = id;
			Title = title;
		}
	}

	[Test, SetCulture("tr-TR"), Description(
		"Constructor parameters match their members by ordinal case, not by the current culture's casing")]
	public void CustomConstructorMatchesMembersInTurkishCulture()
	{
		Assert.That(TypeSchema.TypeGetCustomConstructor(typeof(ItemWithId)), Is.Not.Null,
			"The (id, title) constructor has to match the Id and Title members.");
	}

	[TestCase("en-US")]
	[TestCase("tr-TR")]
	[Description("The round trip gives the same result in a culture with a dotless lowercase i")]
	public void SerializeCustomConstructorIsCultureInvariant(string culture)
	{
		CultureInfo previous = CultureInfo.CurrentCulture;
		CultureInfo.CurrentCulture = new CultureInfo(culture);
		try
		{
			var input = new ItemWithId(5, "Test");

			_serializer.Save(Call, input);
			var output = _serializer.Load<ItemWithId>(Call);

			Assert.That(output.Id, Is.EqualTo(5), "Id is only restored if its constructor parameter matched.");
			Assert.That(output.Title, Is.EqualTo("Test"));
		}
		finally
		{
			CultureInfo.CurrentCulture = previous;
		}
	}
}
