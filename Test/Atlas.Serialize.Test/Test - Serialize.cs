using Atlas.Core;
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
				UintTest = 5,
				DoubleTest = 2.5,
				StringTest = "abc",
			};

			serializer.Save(Call, input);
			var output = serializer.Load<Primitives>(Call);

			Assert.AreEqual(output.UintTest, input.UintTest);
			Assert.AreEqual(output.DoubleTest, input.DoubleTest);
			Assert.AreEqual(output.StringTest, input.StringTest);
		}

		[Test, Description("Serialize Nullable int")]
		public void SerializeNullableInt()
		{
			int? input = 1;

			serializer.Save(Call, input);
			int? output = serializer.Load<int?>(Call);

			Assert.AreEqual(output, input);
		}

		[Test, Description("Serialize Nullable Primitive")]
		public void SerializeNullablePrimitive()
		{
			int? input = 5;

			serializer.Save(Call, input);
			int? output = serializer.Load<int?>(Call);

			Assert.AreEqual(output, input);
		}

		[Test, Description("Serialize Nullable Field Primitives")]
		public void SerializeNullableFieldPrimitives()
		{
			var input = new NullableFieldPrimitives()
			{
				UintTest = 5,
				DoubleTest = 2.5,
			};

			serializer.Save(Call, input);
			var output = serializer.Load<NullableFieldPrimitives>(Call);

			Assert.AreEqual(output.UintTest, input.UintTest);
			Assert.AreEqual(output.DoubleTest, input.DoubleTest);
		}

		[Test, Description("Serialize Nullable Properties Primitive")]
		public void SerializeNullablePropertyPrimitives()
		{
			var input = new NullablePropertyPrimitives()
			{
				UintTest = 5,
				DoubleTest = 2.5,
			};

			serializer.Save(Call, input);
			var output = serializer.Load<NullablePropertyPrimitives>(Call);

			Assert.AreEqual(output.UintTest, input.UintTest);
			Assert.AreEqual(output.DoubleTest, input.DoubleTest);
		}

		[Test, Description("Serialize int")]
		public void SerializeInt()
		{
			int input = 5;

			serializer.Save(Call, input);
			int output = serializer.Load<int>(Call);

			Assert.AreEqual(input, output);
		}

		[Test, Description("Serialize Enum")]
		public void SerializeEnum()
		{
			var input = new EnumTest
			{
				testEnum = MyEnum.b
			};

			serializer.Save(Call, input);
			EnumTest output = serializer.Load<EnumTest>(Call);

			Assert.AreEqual(output.testEnum, input.testEnum);
		}

		[Test, Description("Serialize Nullable Enum")]
		public void SerializeNullableEnum()
		{
			MyEnum? input = MyEnum.b;

			serializer.Save(Call, input);
			MyEnum? output = serializer.Load<MyEnum?>(Call);

			Assert.AreEqual(output, input);
		}

		[Test, Description("Serialize Type")]
		public void SerializeType()
		{
			Type type = typeof(string);

			serializer.Save(Call, type);
			Type output = serializer.Load<Type>(Call);

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

			serializer.Save(Call, input);
			StructTest output = serializer.Load<StructTest>(Call);

			Assert.AreEqual(input, output);
		}

		// DateTime has no set operators and relies on constructor
		[Test, Description("Serialize DateTime Local")]
		public void SerializeDateTimeLocal()
		{
			DateTime input = DateTime.Now;

			serializer.Save(Call, input);
			DateTime output = serializer.Load<DateTime>(Call);

			Assert.AreEqual(input, output);
		}

		// DateTime has no set operators and relies on constructor
		[Test, Description("Serialize DateTime UTC")]
		public void SerializeDateTimeUtc()
		{
			DateTime input = DateTime.UtcNow;

			serializer.Save(Call, input);
			DateTime output = serializer.Load<DateTime>(Call);

			Assert.AreEqual(input, output);
		}

		// DateTimeOffset has no set operators and relies on constructor
		[Test, Description("Serialize DateTimeOffset Local")]
		public void SerializeDateTimeOffsetLocal()
		{
			DateTime dateTime = DateTime.Now;
			DateTimeOffset input = new DateTimeOffset(dateTime);

			serializer.Save(Call, input);
			DateTimeOffset output = serializer.Load<DateTimeOffset>(Call);

			Assert.AreEqual(input, output);
		}

		// DateTimeOffset has no set operators and relies on constructor
		[Test, Description("Serialize DateTimeOffset UTC")]
		public void SerializeDateTimeOffsetUtc()
		{
			DateTime dateTime = DateTime.UtcNow;
			DateTimeOffset input = new DateTimeOffset(dateTime);

			serializer.Save(Call, input);
			DateTimeOffset output = serializer.Load<DateTimeOffset>(Call);

			Assert.AreEqual(input, output);
		}

		// DateTime has no set operators and relies on constructor
		[Test, Description("Serialize TimeZoneInfo")]
		public void SerializeTimeZoneInfo()
		{
			TimeZoneInfo input = TimeZoneInfo.Local;

			serializer.Save(Call, input);
			TimeZoneInfo output = serializer.Load<TimeZoneInfo>(Call);

			Assert.AreEqual(input, output);
		}

		// DateTime has no set operators and relies on constructor
		[Test, Description("Serialize TimeZoneView")]
		public void SerializeTimeZoneView()
		{
			TimeZoneView input = TimeZoneView.Local;

			serializer.Save(Call, input);
			TimeZoneView output = serializer.Load<TimeZoneView>(Call);

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

			serializer.Save(Call, input);
			var output = serializer.Load<NullableDateTime>(Call);

			Assert.AreEqual(input.TimeStamp, output.TimeStamp);
		}

		[Test, Description("Serialize Byte Array")]
		public void SerializeByteArray()
		{
			byte[] input = new byte[1000];
			for (int i = 0; i < input.Length; i++)
				input[i] = 1;
			serializer.Save(Call, input);
			byte[] output = serializer.Load<byte[]>(Call);
			Assert.NotNull(output);
		}

		[Test, Description("Serialize Nullable List")]
		public void SerializeNullableList()
		{
			var input = new List<int?>
			{
				null,
				1,
				null,
				2
			};

			serializer.Save(Call, input);
			var output = serializer.Load<List<int?>>(Call);
			Assert.NotNull(output);
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
			serializer.Save(Call, arrays);
			var output = serializer.Load<MultipleArrays>(Call);
			Assert.NotNull(output);
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

			serializer.Save(Call, input);
			var output = serializer.Load<Objects>(Call);
			Assert.NotNull(output);
		}

		[Test, Description("Serialize Properties")]
		public void SerializeProperties()
		{
			var input = new Properties()
			{
				UintTest = 5,
				DoubleTest = 2.5,
				StringTest = "abc",
			};

			serializer.Save(Call, input);
			var output = serializer.Load<Properties>(Call);

			Assert.AreEqual(output.UintTest, input.UintTest);
			Assert.AreEqual(output.DoubleTest, input.DoubleTest);
			Assert.AreEqual(output.StringTest, input.StringTest);
		}

		[Test, Description("Serialize Properties")]
		public void SerializeFieldInterfaceList()
		{
			var input = new FieldInterfaceList
			{
				List = new List<uint> { 1, 2, 3 }
			};

			serializer.Save(Call, input);
			var output = serializer.Load<FieldInterfaceList>(Call);

			Assert.AreEqual(output.List, input.List);
		}

		[Test, Description("Serialize Field Subclass")]
		public void SerializeFieldSubclass()
		{
			var input = new DerivedClassReference()
			{
				baseClass = new DerivedClass(),
			};

			serializer.Save(Call, input);
			DerivedClassReference output = serializer.Load<DerivedClassReference>(Call);

			Assert.AreEqual(output.baseClass.A, input.baseClass.A);
		}

		[Test, Description("Serialize Type Dictionary")]
		public void SerializeTypeDictionary()
		{
			var input = new Dictionary<Type, string>
			{
				[typeof(int)] = "integer"
			};

			serializer.Save(Call, input);
			var output = serializer.Load<Dictionary<Type, string>>(Call);

			Assert.IsTrue(output.ContainsKey(typeof(int)));
			Assert.IsTrue(output.ContainsValue("integer"));
		}

		[Test, Description("Serialize Circular Dependency")]
		public void SerializeCircular()
		{
			var input = new Circular();
			input.self = input;

			serializer.Save(Call, input);
			Circular output = serializer.Load<Circular>(Call);

			Assert.AreEqual(output.self, output);
		}

		[Test, Description("Serialize Parent Child")]
		public void SerializeParentChild()
		{
			var parent = new Parent();
			var child = new Child();
			parent.child = child;
			child.parent = parent;

			serializer.Save(Call, parent);
			Parent loaded = serializer.Load<Parent>(Call);

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

			serializer.Save(Call, input);
			var output = serializer.Load<List<string>>(Call);

			Assert.AreEqual(input[0], output[0]);
			Assert.AreEqual(input[1], output[1]);
		}

		[Test, Description("Serialize String Dictionary")]
		public void SerializeStringDictionary()
		{
			var input = new Dictionary<string, string>
			{
				["a"] = "1",
				["b"] = "2"
			};

			serializer.Save(Call, input);
			var output = serializer.Load<Dictionary<string, string>>(Call);

			Assert.AreEqual(input["a"], output["a"]);
			Assert.AreEqual(input["b"], output["b"]);
		}

		[Test, Description("Serialize Dictionary Circular References")]
		public void SerializeDictionaryCircularReferences()
		{
			var input = new DictionaryTest();

			serializer.Save(Call, input);
			DictionaryTest output = serializer.Load<DictionaryTest>(Call);
			Assert.NotNull(output);

			//Assert.AreEqual(input, output);
		}

		[Test, Description("Serialize Array")]
		public void SerializeArray()
		{
			int[] input = { 1, 2 };
			input[0] = 5;

			serializer.Save(Call, input);
			int[] output = serializer.Load<int[]>(Call);

			Assert.AreEqual(2, output.Length);
			Assert.AreEqual(5, output[0]);
			Assert.AreEqual(2, output[1]);
		}

		[Test, Description("Serialize HashSet")]
		public void SerializeHashSet()
		{
			var input = new HashSet<string>
			{
				"test",
			};

			serializer.Save(Call, input);
			var output = serializer.Load<HashSet<string>>(Call);

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

			serializer.Save(Call, input);
			var output = serializer.Load<HashSet<SelectedItem>>(Call);

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

			serializer.Save(Call, input);
			var output = serializer.Load<NonSerializedTest>(Call);

			Assert.AreEqual(output.nonSerialized, 1);
			Assert.AreEqual(output.serialized, 10);
		}

		[Test, Description("Serialize [Secure]")]
		public void SerializeSecure()
		{
			var input = new SecureContainer()
			{
				SecureField = new SecureClass()
				{
					Confidential = "secrets",
				},
				SecureProperty = new SecureClass()
				{
					Confidential = "more secrets",
				},
				NonSecure = "test",
			};

			serializer.SaveSecure = false;
			serializer.Save(Call, input);
			var output = serializer.Load<SecureContainer>(Call);

			Assert.IsNull(output.SecureField);
			Assert.IsNull(output.SecureProperty);
			Assert.AreEqual(output.NonSecure, "test");
		}

		public class SecureContainer
		{
			public SecureClass SecureField;
			public SecureClass SecureProperty { get; set; }
			public string NonSecure;
		}

		[Secure]
		public class SecureClass
		{
			public string Confidential { get; set; }
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

		public class Primitives
		{
			public uint UintTest = 1;
			public double DoubleTest = 2.3;
			public string StringTest = "mystring";
		}

		public class Properties
		{
			public uint UintTest { get; set; } = 1;
			public double DoubleTest { get; set; } = 2.3;
			public string StringTest { get; set; } = "mystring";
			public Type Type { get; set; } = null;
		}

		public class FieldInterfaceList
		{
			public IList List;
		}

		public class Objects
		{
			public object Object = 2;
		}

		public class BaseClass
		{
			public int A = 1;
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

			serializer.Save(Call, input);
			var output = serializer.Load<SubClassContainer>(Call);

			Assert.AreEqual(output.subSclass.A, input.subSclass.A);
		}

		[Test, Description("Serialize List Containing Subclass of Type")]
		public void SerializeListContainingSubclassOfType()
		{
			var input = new List<Base>
			{
				new SubClass() { A = 5 }
			};
			serializer.Save(Call, input);
			List<Base> output = serializer.Load<List<Base>>(Call);

			Assert.AreEqual(output[0].A, 5);
			//Assert.AreEqual(input, output); // only works on primitives
		}

		[Test, Description("Serialize Dictionary Containing Subclass of Type")]
		public void SerializeDictionaryContainingSubclassOfType()
		{
			var input = new Dictionary<Base, Base>();

			Base b = new Base();
			SubClass s = new SubClass
			{
				B = 3
			};
			input[s] = b;
			serializer.Save(Call, input);
			var output = serializer.Load<Dictionary<Base, Base>>(Call);
			Assert.NotNull(output);

			Assert.AreEqual(s.B, 3);
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
			public SubClass subSclass = new SubClass()
			{
				A = 3
			};
		}

		[Test, Description("Serialize Dictionary Containing Subclass of Type")]
		public void SerializeDictionaryOfObjects()
		{
			var input = new Dictionary<string, object>
			{
				{ "default", true }
			};

			serializer.Save(Call, input);
			var output = serializer.Load<Dictionary<string, object>>(Call);

			Assert.AreEqual(true, output["default"]);
		}
	}
}
