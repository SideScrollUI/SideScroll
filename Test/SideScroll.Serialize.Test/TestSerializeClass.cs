using NUnit.Framework;
using SideScroll.Serialize.Atlas;
using System.Collections;

namespace SideScroll.Serialize.Test;

[Category("Serialize")]
public class SerializeClass : TestSerializeBase
{
	private SerializerMemory _serializer = new SerializerMemoryAtlas();

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("SerializeClass", Logs.LogLevel.Debug);
	}

	[SetUp]
	public void Setup()
	{
		_serializer = new SerializerMemoryAtlas();
	}

	public class Fields
	{
		public uint UintTest = 1;
		public double DoubleTest = 2.3;
		public string StringTest = "mystring";
	}

	[Test, Description("Serialize Field Primitives")]
	public void SerializeFields()
	{
		var input = new Fields
		{
			UintTest = 5,
			DoubleTest = 2.5,
			StringTest = "abc",
		};

		_serializer.Save(Call, input);
		var output = _serializer.Load<Fields>(Call);

		Assert.AreEqual(input.UintTest, output.UintTest);
		Assert.AreEqual(input.DoubleTest, output.DoubleTest);
		Assert.AreEqual(input.StringTest, output.StringTest);
	}

	public class Properties
	{
		public uint UintTest { get; set; } = 1;
		public double DoubleTest { get; set; } = 2.3;
		public string StringTest { get; set; } = "mystring";
		public Type? Type { get; set; } = null;
	}

	[Test, Description("Serialize Properties")]
	public void SerializeProperties()
	{
		var input = new Properties
		{
			UintTest = 5,
			DoubleTest = 2.5,
			StringTest = "abc",
		};

		_serializer.Save(Call, input);
		var output = _serializer.Load<Properties>(Call);

		Assert.AreEqual(input.UintTest, output.UintTest);
		Assert.AreEqual(input.DoubleTest, output.DoubleTest);
		Assert.AreEqual(input.StringTest, output.StringTest);
	}

	public class NullablePropertyPrimitives
	{
		public uint? UintTest { get; set; } = 1;
		public double? DoubleTest { get; set; } = 2.3;
	}

	public class NullableFieldPrimitives
	{
		public uint? UintTest = 1;
		public double? DoubleTest = 2.3;
	}

	[Test, Description("Serialize Nullable Field Primitives")]
	public void SerializeNullableFieldPrimitives()
	{
		var input = new NullableFieldPrimitives
		{
			UintTest = 5,
			DoubleTest = 2.5,
		};

		_serializer.Save(Call, input);
		var output = _serializer.Load<NullableFieldPrimitives>(Call);

		Assert.AreEqual(input.UintTest, output.UintTest);
		Assert.AreEqual(input.DoubleTest, output.DoubleTest);
	}

	[Test, Description("Serialize Nullable Properties Primitive")]
	public void SerializeNullablePropertyPrimitives()
	{
		var input = new NullablePropertyPrimitives
		{
			UintTest = 5,
			DoubleTest = 2.5,
		};

		_serializer.Save(Call, input);
		var output = _serializer.Load<NullablePropertyPrimitives>(Call);

		Assert.AreEqual(input.UintTest, output.UintTest);
		Assert.AreEqual(input.DoubleTest, output.DoubleTest);
	}

	public class FieldInterfaceList
	{
		public IList? List;
	}

	[Test, Description("Serialize Properties")]
	public void SerializeFieldInterfaceList()
	{
		var input = new FieldInterfaceList
		{
			List = new List<uint> { 1, 2, 3 }
		};

		_serializer.Save(Call, input);
		var output = _serializer.Load<FieldInterfaceList>(Call);

		Assert.AreEqual(input.List, output.List);
	}

	public class BaseClass
	{
		public int A = 1;
	}

	public class DerivedClass : BaseClass;

	public class DerivedClassReference
	{
		public BaseClass? BaseClass;
	}

	[Test, Description("Serialize Field Subclass")]
	public void SerializeFieldSubclass()
	{
		var input = new DerivedClassReference
		{
			BaseClass = new DerivedClass(),
		};

		_serializer.Save(Call, input);
		var output = _serializer.Load<DerivedClassReference>(Call);

		Assert.AreEqual(input.BaseClass!.A, output.BaseClass?.A);
	}

	public class Circular
	{
		public Circular? Self;
	}

	[Test, Description("Serialize Circular Dependency")]
	public void SerializeCircular()
	{
		var input = new Circular();
		input.Self = input;

		_serializer.Save(Call, input);
		var output = _serializer.Load<Circular>(Call);

		Assert.AreEqual(output.Self, output);
	}

	[Test, Description("Serialize Dictionary Circular References")]
	public void SerializeDictionaryCircularReferences()
	{
		var input = new DictionaryTest();

		_serializer.Save(Call, input);
		var output = _serializer.Load<DictionaryTest>(Call);
		Assert.NotNull(output);

		//Assert.AreEqual(input, output);
	}

	[Test, Description("Serialize Parent Child")]
	public void SerializeParentChild()
	{
		var parent = new Parent();
		var child = new Child();
		parent.Child = child;
		child.Parent = parent;

		_serializer.Save(Call, parent);
		Parent loaded = _serializer.Load<Parent>(Call);

		Assert.AreEqual(loaded.Child!.Parent, loaded);
	}

	public class Base
	{
		public int A { get; set; } = 1;
	}

	public class SubClass : Base
	{
		public int B { get; set; } = 2;
	}

	public class SubClassContainer
	{
		public SubClass SubClass = new()
		{
			A = 3
		};
	}


	[Test, Description("Serialize List Containing Subclass of Type")]
	public void SerializeSubClassContainer()
	{
		var input = new SubClassContainer();

		_serializer.Save(Call, input);
		var output = _serializer.Load<SubClassContainer>(Call);

		Assert.AreEqual(input.SubClass.A, output.SubClass.A);
	}

	[Test, Description("Serialize List Containing Subclass of Type")]
	public void SerializeListContainingSubclassOfType()
	{
		var input = new List<Base>
		{
			new SubClass { A = 5 }
		};
		_serializer.Save(Call, input);
		var output = _serializer.Load<List<Base>>(Call);

		Assert.AreEqual(5, output[0].A);
		//Assert.AreEqual(input, output); // only works on primitives
	}

	[Test, Description("Serialize Dictionary Containing Subclass of Type")]
	public void SerializeDictionaryContainingSubclassOfType()
	{
		var input = new Dictionary<Base, Base>();

		var b = new Base();
		var s = new SubClass
		{
			B = 3
		};
		input[s] = b;
		_serializer.Save(Call, input);
		var output = _serializer.Load<Dictionary<Base, Base>>(Call);
		Assert.NotNull(output);

		Assert.AreEqual(s.B, 3);
	}

	public class Parent
	{
		public Child? Child;
	}

	public class Child
	{
		public Parent? Parent;
	}

	public class DictionaryTest
	{
		public Dictionary<Parent, Child> Items = [];

		public DictionaryTest()
		{
			Parent parent = new();
			Child child = new();
			parent.Child = child;
			child.Parent = parent;
			Items[parent] = child;
		}
	}

	[Test]
	public void ToBase64String()
	{
		Properties input = new()
		{
			UintTest = 5,
			DoubleTest = 2.5,
			StringTest = "abc",
		};

		string base64 = SerializerMemory.ToBase64String(Call, input);

		// Todo: Fix, compression size isn't predictable :(
		// Use specific compression level instead for small?
		// Uncompressed: 912 -> Compressed: 313 -> Base64: 420
		// Uncompressed: 912 -> Compressed: 316 -> Base64: 424
		Assert.GreaterOrEqual(base64.Length, 400);
		Assert.LessOrEqual(base64.Length, 440);
	}
}
