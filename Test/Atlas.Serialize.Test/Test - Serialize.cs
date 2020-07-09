using Atlas.Core.Time;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Atlas.Serialize.Test
{
	[Category("Serialize")]
	public class SerializeTypes : TestSerializeBase
	{
		private SerializerMemory serializer;
		
		[OneTimeSetUp]
		public void BaseSetup()
		{
			Initialize("Serialize");
		}

		[SetUp]
		public void Setup()
		{
			serializer = new SerializerMemory();
		}

		[Test, Description("Serialize Primitives")]
		public void SerializePrimitives()
		{
			var input = new Primitives()
			{
				uintTest = 5,
				doubleTest = 2.5,
				stringTest = "abc",
			};

			serializer.Save(call, input);
			var output = serializer.Load<Primitives>(call);

			Assert.AreEqual(output.uintTest, input.uintTest);
			Assert.AreEqual(output.doubleTest, input.doubleTest);
			Assert.AreEqual(output.stringTest, input.stringTest);
		}

		[Test, Description("Serialize Nullable int")]
		public void SerializeNullableInt()
		{
			int? input = 1;

			serializer.Save(call, input);
			int? output = serializer.Load<int?>(call);

			Assert.AreEqual(output, input);
		}

		[Test, Description("Serialize Nullable Primitive")]
		public void SerializeNullablePrimitive()
		{
			int? input = 5;

			serializer.Save(call, input);
			int? output = serializer.Load<int?>(call);

			Assert.AreEqual(output, input);
		}

		[Test, Description("Serialize Nullable Field Primitives")]
		public void SerializeNullableFieldPrimitives()
		{
			var input = new NullableFieldPrimitives()
			{
				uintTest = 5,
				doubleTest = 2.5,
			};

			serializer.Save(call, input);
			var output = serializer.Load<NullableFieldPrimitives>(call);

			Assert.AreEqual(output.uintTest, input.uintTest);
			Assert.AreEqual(output.doubleTest, input.doubleTest);
		}

		[Test, Description("Serialize Nullable Properties Primitive")]
		public void SerializeNullablePropertyPrimitives()
		{
			var input = new NullablePropertyPrimitives()
			{
				uintTest = 5,
				doubleTest = 2.5,
			};

			serializer.Save(call, input);
			var output = serializer.Load<NullablePropertyPrimitives>(call);

			Assert.AreEqual(output.uintTest, input.uintTest);
			Assert.AreEqual(output.doubleTest, input.doubleTest);
		}

		[Test, Description("Serialize int")]
		public void SerializeInt()
		{
			int input = 5;

			serializer.Save(call, input);
			int output = serializer.Load<int>(call);

			Assert.AreEqual(input, output);
		}

		[Test, Description("Serialize Enum")]
		public void SerializeEnum()
		{
			var input = new EnumTest();
			input.testEnum = MyEnum.b;

			serializer.Save(call, input);
			EnumTest output = serializer.Load<EnumTest>(call);

			Assert.AreEqual(output.testEnum, input.testEnum);
		}

		[Test, Description("Serialize Nullable Enum")]
		public void SerializeNullableEnum()
		{
			MyEnum? input = MyEnum.b;

			serializer.Save(call, input);
			MyEnum? output = serializer.Load<MyEnum?>(call);

			Assert.AreEqual(output, input);
		}

		[Test, Description("Serialize Type")]
		public void SerializeType()
		{
			Type type = typeof(string);

			serializer.Save(call, type);
			Type output = serializer.Load<Type>(call);

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

			serializer.Save(call, input);
			StructTest output = serializer.Load<StructTest>(call);

			Assert.AreEqual(input, output);
		}

		// DateTime has no set operators and relies on constructor
		[Test, Description("Serialize DateTime Local")]
		public void SerializeDateTimeLocal()
		{
			DateTime input = DateTime.Now;

			serializer.Save(call, input);
			DateTime output = serializer.Load<DateTime>(call);

			Assert.AreEqual(input, output);
		}

		// DateTime has no set operators and relies on constructor
		[Test, Description("Serialize DateTime UTC")]
		public void SerializeDateTimeUtc()
		{
			DateTime input = DateTime.UtcNow;

			serializer.Save(call, input);
			DateTime output = serializer.Load<DateTime>(call);

			Assert.AreEqual(input, output);
		}

		// DateTimeOffset has no set operators and relies on constructor
		[Test, Description("Serialize DateTimeOffset Local")]
		public void SerializeDateTimeOffsetLocal()
		{
			DateTime dateTime = DateTime.Now;
			DateTimeOffset input = new DateTimeOffset(dateTime);

			serializer.Save(call, input);
			DateTimeOffset output = serializer.Load<DateTimeOffset>(call);

			Assert.AreEqual(input, output);
		}

		// DateTimeOffset has no set operators and relies on constructor
		[Test, Description("Serialize DateTimeOffset UTC")]
		public void SerializeDateTimeOffsetUtc()
		{
			DateTime dateTime = DateTime.UtcNow;
			DateTimeOffset input = new DateTimeOffset(dateTime);

			serializer.Save(call, input);
			DateTimeOffset output = serializer.Load<DateTimeOffset>(call);

			Assert.AreEqual(input, output);
		}

		// DateTime has no set operators and relies on constructor
		[Test, Description("Serialize TimeZoneInfo")]
		public void SerializeTimeZoneInfo()
		{
			TimeZoneInfo input = TimeZoneInfo.Local;

			serializer.Save(call, input);
			TimeZoneInfo output = serializer.Load<TimeZoneInfo>(call);

			Assert.AreEqual(input, output);
		}

		// DateTime has no set operators and relies on constructor
		[Test, Description("Serialize TimeZoneView")]
		public void SerializeTimeZoneView()
		{
			TimeZoneView input = TimeZoneView.Local;

			serializer.Save(call, input);
			TimeZoneView output = serializer.Load<TimeZoneView>(call);

			Assert.AreEqual(input.Abbreviation, output.Abbreviation);
			Assert.AreEqual(input.Name, output.Name);
			Assert.AreEqual(input.TimeZoneInfo, output.TimeZoneInfo);
		}

		public class NullableDateTime
		{
			public long Long { get; set; }
			public DateTime? TimeStamp { get; set; }
		}

		// DateTime has no set operators and relies on constructor
		[Test, Description("Serialize Long and DateTime")]
		public void SerializeLongAndDateTime()
		{
			var input = new NullableDateTime()
			{
				TimeStamp = DateTime.UtcNow,
			};

			serializer.Save(call, input);
			var output = serializer.Load<NullableDateTime>(call);

			Assert.AreEqual(input.TimeStamp, output.TimeStamp);
		}

		[Test, Description("Serialize Byte Array")]
		public void SerializeByteArray()
		{
			byte[] input = new byte[1000];
			for (int i = 0; i < input.Length; i++)
				input[i] = 1;
			serializer.Save(call, input);
			byte[] output = serializer.Load<byte[]>(call);
		}

		[Test, Description("Serialize Nullable List")]
		public void SerializeNullableList()
		{
			var input = new List<int?>();
			input.Add(null);
			input.Add(1);
			input.Add(null);
			input.Add(2);

			serializer.Save(call, input);
			var output = serializer.Load<List<int?>>(call);
		}

		private class MultipleArrays
		{
			public int[] array1 = { 1, 2 };
			//public int[] array2 = { 3, 4 };
		}

		[Test, Description("ArrayMultipleTest")]
		public void ArrayMultipleTest()
		{
			var arrays = new MultipleArrays();
			serializer.Save(call, arrays);
			var output = serializer.Load<MultipleArrays>(call);
		}


		[Test, Description("ArrayTest")]
		public void ArrayTest()
		{
			int[] array1 = { };
			int[] array2 = { };

			var idxObjectToIndex = new Dictionary<object, int>(); // for saving, not filled in for loading
			idxObjectToIndex[array1] = idxObjectToIndex.Count;

			if (idxObjectToIndex.ContainsKey(array2))
			{
				Debug.Assert(true);
			}
			else
			{
				idxObjectToIndex[array2] = idxObjectToIndex.Count;
			}
		}

		[Test, Description("Serialize Objects")]
		public void SerializeObjects()
		{
			var input = new Objects();

			serializer.Save(call, input);
			var output = serializer.Load<Objects>(call);
		}

		[Test, Description("Serialize Properties")]
		public void SerializeProperties()
		{
			var input = new Properties()
			{
				uintTest = 5,
				doubleTest = 2.5,
				stringTest = "abc",
			};

			serializer.Save(call, input);
			var output = serializer.Load<Properties>(call);

			Assert.AreEqual(output.uintTest, input.uintTest);
			Assert.AreEqual(output.doubleTest, input.doubleTest);
			Assert.AreEqual(output.stringTest, input.stringTest);
		}

		[Test, Description("Serialize Properties")]
		public void SerializeFieldInterfaceList()
		{
			var input = new FieldInterfaceList();
			input.list = new List<uint> { 1, 2, 3 };

			serializer.Save(call, input);
			var output = serializer.Load<FieldInterfaceList>(call);

			Assert.AreEqual(output.list, input.list);
		}

		[Test, Description("Serialize Field Subclass")]
		public void SerializeFieldSubclass()
		{
			var input = new DerivedClassReference();
			input.baseClass = new DerivedClass();

			serializer.Save(call, input);
			DerivedClassReference output = serializer.Load<DerivedClassReference>(call);

			Assert.AreEqual(output.baseClass.a, input.baseClass.a);
		}

		[Test, Description("Serialize Type Dictionary")]
		public void SerializeTypeDictionary()
		{
			var input = new Dictionary<Type, string>();
			input[typeof(int)] = "integer";

			serializer.Save(call, input);
			var output = serializer.Load<Dictionary<Type, string>>(call);

			Assert.IsTrue(output.ContainsKey(typeof(int)));
			Assert.IsTrue(output.ContainsValue("integer"));
		}

		[Test, Description("Serialize Circular Dependency")]
		public void SerializeCircular()
		{
			var input = new Circular();
			input.self = input;

			serializer.Save(call, input);
			Circular output = serializer.Load<Circular>(call);

			Assert.AreEqual(output.self, output);
		}

		[Test, Description("Serialize Parent Child")]
		public void SerializeParentChild()
		{
			var parent = new Parent();
			var child = new Child();
			parent.child = child;
			child.parent = parent;

			serializer.Save(call, parent);
			Parent loaded = serializer.Load<Parent>(call);

			Assert.AreEqual(loaded.child.parent, loaded);
		}

		[Test, Description("Serialize String List")]
		public void SerializeStringList()
		{
			var input = new List<string>()
			{
				"abc",
				"123"
			};

			serializer.Save(call, input);
			var output = serializer.Load<List<string>>(call);

			Assert.AreEqual(input[0], output[0]);
			Assert.AreEqual(input[1], output[1]);
		}

		[Test, Description("Serialize String Dictionary")]
		public void SerializeStringDictionary()
		{
			var input = new Dictionary<string, string>();
			input["a"] = "1";
			input["b"] = "2";

			serializer.Save(call, input);
			var output = serializer.Load<Dictionary<string, string>>(call);

			Assert.AreEqual(input["a"], output["a"]);
			Assert.AreEqual(input["b"], output["b"]);
		}

		[Test, Description("Serialize Dictionary Circular References")]
		public void SerializeDictionaryCircularReferences()
		{
			var input = new DictionaryTest();

			serializer.Save(call, input);
			DictionaryTest output = serializer.Load<DictionaryTest>(call);

			//Assert.AreEqual(input, output);
		}

		[Test, Description("Serialize Array")]
		public void SerializeArray()
		{
			int[] input = { 1, 2 };
			input[0] = 5;

			serializer.Save(call, input);
			int[] output = serializer.Load<int[]>(call);

			Assert.AreEqual(2, output.Length);
			Assert.AreEqual(5, output[0]);
			Assert.AreEqual(2, output[1]);
		}

		[Test, Description("Serialize HashSet")]
		public void SerializeHashSet()
		{
			var input = new HashSet<string>();
			input.Add("test");

			serializer.Save(call, input);
			var output = serializer.Load<HashSet<string>>(call);

			Assert.AreEqual(input.Count, output.Count);
			Assert.True(output.Contains("test"));
		}

		public class SelectedItem
		{
			public string label;
			public bool pinned;
		}

		public class TabInstanceConfiguration
		{
			public HashSet<SelectedItem> selected = new HashSet<SelectedItem>();
			public int? SplitterDistance;
			public int NumColumns;
		}

		[Test, Description("Serialize HashSet")]
		public void SerializeHashSetObject()
		{
			var input = new HashSet<SelectedItem>();
			var inputItem = new SelectedItem()
			{
				label = "abc",
				pinned = true,
			};
			input.Add(inputItem);

			serializer.Save(call, input);
			var output = serializer.Load<HashSet<SelectedItem>>(call);

			Assert.AreEqual(input.Count, output.Count);
			//Assert.True(output.Contains("test"));
		}

		[Test, Description("Serialize Attribute NonSerialized")]
		public void SerializeAttributeNonSerialized()
		{
			var input = new NonSerializedTest()
			{
				nonSerialized = 5,
				serialized = 10,
			};

			serializer.Save(call, input);
			var output = serializer.Load<NonSerializedTest>(call);

			Assert.AreEqual(output.nonSerialized, 1);
			Assert.AreEqual(output.serialized, 10);
		}
		
		public class NullablePropertyPrimitives
		{
			public uint? uintTest { get; set; } = 1;
			public double? doubleTest { get; set; } = 2.3;
		}

		public class NullableFieldPrimitives
		{
			public uint? uintTest = 1;
			public double? doubleTest = 2.3;
		}

		public class Primitives
		{
			public uint uintTest = 1;
			public double doubleTest = 2.3;
			public string stringTest = "mystring";
		}

		public class Properties
		{
			public uint uintTest { get; set; } = 1;
			public double doubleTest { get; set; } = 2.3;
			public string stringTest { get; set; } = "mystring";
			public Type type { get; set; } = null;
		}

		public class FieldInterfaceList
		{
			public IList list;
		}

		public class Objects
		{
			public object obj = 2;
		}

		public class BaseClass
		{
			public int a = 1;
		}

		public class DerivedClass : BaseClass
		{
		}

		public class DerivedClassReference
		{
			public BaseClass baseClass;
		}

		public class Circular
		{
			public Circular self;
		}

		public class Parent
		{
			public Child child;
		}

		public class Child
		{
			public Parent parent;
		}

		public class DictionaryTest
		{
			public Dictionary<Parent, Child> items = new Dictionary<Parent, Child>();

			public DictionaryTest()
			{
				Parent parent = new Parent();
				Child child = new Child();
				parent.child = child;
				child.parent = parent;
				items[parent] = child;
			}
		}

		public enum MyEnum
		{
			a,
			b
		}

		public class EnumTest
		{
			public MyEnum testEnum = MyEnum.a;
		}


		public class NonSerializedTest
		{
			[NonSerialized]
			public int nonSerialized = 1;
			public int serialized = 2;
		}

		
		[Test, Description("Serialize List Containing Subclass of Type")]
		public void SerializeSubClassContainer()
		{
			var input = new SubClassContainer();

			serializer.Save(call, input);
			var output = serializer.Load<SubClassContainer>(call);

			Assert.AreEqual(output.subSclass.a, input.subSclass.a);
		}

		[Test, Description("Serialize List Containing Subclass of Type")]
		public void SerializeListContainingSubclassOfType()
		{
			var input = new List<Base>();

			input.Add(new SubClass() { a = 5 });
			serializer.Save(call, input);
			List<Base> output = serializer.Load<List<Base>>(call);

			Assert.AreEqual(output[0].a, 5);
			//Assert.AreEqual(input, output); // only works on primitives
		}

		[Test, Description("Serialize Dictionary Containing Subclass of Type")]
		public void SerializeDictionaryContainingSubclassOfType()
		{
			var input = new Dictionary<Base, Base>();

			Base b = new Base();
			SubClass s = new SubClass();
			s.b = 3;
			input[s] = b;
			serializer.Save(call, input);
			var output = serializer.Load<Dictionary<Base, Base>>(call);

			Assert.AreEqual(s.b, 3);
		}

		public class Base
		{
			public int a { get; set; } = 1;
		}

		public class SubClass : Base
		{
			public int b { get; set; } = 2;
		}

		public class SubClassContainer
		{
			public SubClass subSclass = new SubClass()
			{
				a = 3
			};
		}

		[Test, Description("Serialize Dictionary Containing Subclass of Type")]
		public void SerializeDictionaryOfObjects()
		{
			var input = new Dictionary<string, object>();
			input.Add("default", true);

			serializer.Save(call, input);
			var output = serializer.Load<Dictionary<string, object>>(call);

			Assert.AreEqual(true, output["default"]);
		}
	}
}
