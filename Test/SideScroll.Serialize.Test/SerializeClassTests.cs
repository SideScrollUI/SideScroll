using NUnit.Framework;
using SideScroll.Serialize.Atlas;
using System.Collections;
using System.Text;

namespace SideScroll.Serialize.Test;

[Category("Serialize")]
public class SerializeClassTests : SerializeBaseTest
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

		Assert.That(output.UintTest, Is.EqualTo(input.UintTest));
		Assert.That(output.DoubleTest, Is.EqualTo(input.DoubleTest));
		Assert.That(output.StringTest, Is.EqualTo(input.StringTest));
	}

	public class StaticMembers
	{
		public static int StaticField = 1;
		public static int StaticProperty { get; set; } = 11;
	}

	[Test, Description("Serialize Static Members")]
	public void SerializeStaticMembers()
	{
		StaticMembers input = new();
		StaticMembers.StaticField = 2;
		StaticMembers.StaticProperty = 12;

		_serializer.Save(Call, input);
		StaticMembers.StaticField = 3;
		StaticMembers.StaticProperty = 13;
		var output = _serializer.Load<StaticMembers>(Call);

		Assert.That(StaticMembers.StaticField, Is.EqualTo(3));
		Assert.That(StaticMembers.StaticProperty, Is.EqualTo(13));
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

		Assert.That(output.UintTest, Is.EqualTo(input.UintTest));
		Assert.That(output.DoubleTest, Is.EqualTo(input.DoubleTest));
		Assert.That(output.StringTest, Is.EqualTo(input.StringTest));
	}

	public class Class1
	{
		public int Integer = 1;
	}

	public class Class2
	{
		public int Integer { get; set; } = 1;
	}

	[Test]
	public void RenameFieldToProperty()
	{
		Class1 input = new()
		{
			Integer = 2,
		};

		_serializer.Save(Call, input);

		ReplaceBytes("Class1", "Class2");

		var output = _serializer.Load<Class2>(Call);

		Assert.That(output.Integer, Is.EqualTo(input.Integer));
	}

	[Test]
	public void RenamePropertyToField()
	{
		Class2 input = new()
		{
			Integer = 2,
		};

		_serializer.Save(Call, input);

		ReplaceBytes("Class2", "Class1");

		var output = _serializer.Load<Class1>(Call);

		Assert.That(output.Integer, Is.EqualTo(input.Integer));
	}

	private void ReplaceBytes(string searchText, string replaceText)
	{
		var bytes = _serializer.Stream.GetBuffer();

		var oldBytes = Encoding.UTF8.GetBytes(searchText);
		var newBytes = Encoding.UTF8.GetBytes(replaceText);

		for (int i = 0; i <= bytes.Length - oldBytes.Length; i++)
		{
			if (bytes.Skip(i).Take(oldBytes.Length).SequenceEqual(oldBytes))
			{
				Array.Copy(newBytes, 0, bytes, i, newBytes.Length);
			}
		}
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

		Assert.That(output.UintTest, Is.EqualTo(input.UintTest));
		Assert.That(output.DoubleTest, Is.EqualTo(input.DoubleTest));
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

		Assert.That(output.UintTest, Is.EqualTo(input.UintTest));
		Assert.That(output.DoubleTest, Is.EqualTo(input.DoubleTest));
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

		Assert.That(output.List, Is.EqualTo(input.List));
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

		Assert.That(output.BaseClass?.A, Is.EqualTo(input.BaseClass!.A));
	}

	public class Circular
	{
		public Circular? Self;
	}

	[Test, Description("Serialize Circular Dependency")]
	public void SerializeCircularDependency()
	{
		var input = new Circular();
		input.Self = input;

		_serializer.Save(Call, input);
		var output = _serializer.Load<Circular>(Call);

		Assert.That(output, Is.EqualTo(output.Self));
	}

	[Test, Description("Serialize Dictionary Circular References")]
	public void SerializeDictionaryCircularReferences()
	{
		var input = new DictionaryTest();

		_serializer.Save(Call, input);
		var output = _serializer.Load<DictionaryTest>(Call);
		Assert.That(output, Is.Not.Null);

		//Assert.That(output, Is.EqualTo(input));
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

		Assert.That(loaded, Is.EqualTo(loaded.Child!.Parent));
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

		Assert.That(output.SubClass.A, Is.EqualTo(input.SubClass.A));
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

		Assert.That(output[0].A, Is.EqualTo(5));
		//Assert.That(output, Is.EqualTo(input)); // only works on primitives
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
		Assert.That(output, Is.Not.Null);

		Assert.That(s.B, Is.EqualTo(3));
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
		Assert.That(base64.Length, Is.GreaterThanOrEqualTo(400));
		Assert.That(base64.Length, Is.LessThanOrEqualTo(440));
	}
}
