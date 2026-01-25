using NUnit.Framework;
using SideScroll.Attributes;
using SideScroll.Serialize.Atlas;

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
}
