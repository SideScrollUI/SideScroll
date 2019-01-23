using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using System.Collections;
using Atlas.Core;

namespace Atlas.Serialize.Test
{
	[Category("Serialize")]
	public class SerializeTypes : TestSerializeBase
	{
		private SerializerFile interFace;
		private Log log;
		
		[OneTimeSetUp]
		public void BaseSetup()
		{
			Initialize("Serialize");
			log = call.log;

			string basePath = Paths.Combine(TestPath, "Serialize");

			Directory.CreateDirectory(basePath);

			string filePath = Paths.Combine(basePath, "Data.atlas");
			interFace = new SerializerFile(filePath);
		}

		[Test, Description("Serialize Primitives")]
		public void SerializePrimitives()
		{
			Primitives input = new Primitives();
			input.uintTest = 5;
			input.doubleTest = 2.5;
			input.stringTest = "abc";

			interFace.Save(call, input);
			Primitives output = interFace.Load<Primitives>(call);

			Assert.AreEqual(output.uintTest, input.uintTest);
			Assert.AreEqual(output.doubleTest, input.doubleTest);
			Assert.AreEqual(output.stringTest, input.stringTest);
		}

		[Test, Description("Serialize Nullable int")]
		public void SerializeNullableInt()
		{
			int? input = 1;

			interFace.Save(call, input);
			int? output = interFace.Load<int?>(call);

			Assert.AreEqual(output, input);
		}

		[Test, Description("Serialize Nullable Primitive")]
		public void SerializeNullablePrimitive()
		{
			int? input = 5;

			interFace.Save(call, input);
			int? output = interFace.Load<int?>(call);

			Assert.AreEqual(output, input);
		}

		[Test, Description("Serialize Nullable Field Primitives")]
		public void SerializeNullableFieldPrimitives()
		{
			NullableFieldPrimitives input = new NullableFieldPrimitives();
			input.uintTest = 5;
			input.doubleTest = 2.5;

			interFace.Save(call, input);
			NullableFieldPrimitives output = interFace.Load<NullableFieldPrimitives>(call);

			Assert.AreEqual(output.uintTest, input.uintTest);
			Assert.AreEqual(output.doubleTest, input.doubleTest);
		}

		[Test, Description("Serialize Nullable Properties Primitive")]
		public void SerializeNullablePropertyPrimitives()
		{
			NullablePropertyPrimitives input = new NullablePropertyPrimitives();
			input.uintTest = 5;
			input.doubleTest = 2.5;

			interFace.Save(call, input);
			NullablePropertyPrimitives output = interFace.Load<NullablePropertyPrimitives>(call);

			Assert.AreEqual(output.uintTest, input.uintTest);
			Assert.AreEqual(output.doubleTest, input.doubleTest);
		}

		[Test, Description("Serialize int")]
		public void SerializeInt()
		{
			int input = 5;

			interFace.Save(call, input);
			int output = interFace.Load<int>(call);

			Assert.AreEqual(input, output);
		}

		[Test, Description("Serialize Enum")]
		public void SerializeEnum()
		{
			EnumTest input = new EnumTest();
			input.testEnum = MyEnum.b;

			interFace.Save(call, input);
			EnumTest output = interFace.Load<EnumTest>(call);

			Assert.AreEqual(output.testEnum, input.testEnum);
		}

		[Test, Description("Serialize Nullable Enum")]
		public void SerializeNullableEnum()
		{
			MyEnum? input = MyEnum.b;

			interFace.Save(call, input);
			MyEnum? output = interFace.Load<MyEnum?>(call);

			Assert.AreEqual(output, input);
		}

		[Test, Description("Serialize Type")]
		public void SerializeType()
		{
			Type type = typeof(string);

			interFace.Save(call, type);
			Type output = interFace.Load<Type>(call);

			Assert.AreEqual(type, output);
		}

		// DateTime has no set operators and relies on constructor
		[Test, Description("Serialize DateTime")]
		public void SerializeDateTime()
		{
			DateTime input = DateTime.Now;

			interFace.Save(call, input);
			DateTime output = interFace.Load<DateTime>(call);

			Assert.AreEqual(input, output);
		}

		[Test, Description("Serialize Byte Array")]
		public void SerializeByteArray()
		{
			byte[] input = new byte[1000];
			for (int i = 0; i < input.Length; i++)
				input[i] = 1;
			interFace.Save(call, input);
			byte[] output = interFace.Load<byte[]>(call);
		}

		[Test, Description("Serialize Nullable List")]
		public void SerializeNullableList()
		{
			List<int?> input = new List<int?>();
			input.Add(null);
			input.Add(1);
			input.Add(null);
			input.Add(2);
			interFace.Save(call, input);
			List<int?> output = interFace.Load<List<int?>>(call);
		}

		private class MultipleArrays
		{
			public int[] array1 = { 1, 2 };
			//public int[] array2 = { 3, 4 };
		}

		[Test, Description("ArrayMultipleTest")]
		public void ArrayMultipleTest()
		{
			MultipleArrays arrays = new MultipleArrays();
			interFace.Save(call, arrays);
			MultipleArrays output = interFace.Load<MultipleArrays>(call);
		}


		[Test, Description("ArrayTest")]
		public void ArrayTest()
		{
			int[] array1 = { };
			int[] array2 = { };

			Dictionary<object, int> idxObjectToIndex = new Dictionary<object, int>(); // for saving, not filled in for loading
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
			Objects input = new Objects();

			interFace.Save(call, input);
			Objects output = interFace.Load<Objects>(call);
		}

		[Test, Description("Serialize Properties")]
		public void SerializeProperties()
		{
			Properties input = new Properties();
			input.uintTest = 5;
			input.doubleTest = 2.5;
			input.stringTest = "abc";

			interFace.Save(call, input);
			Properties output = interFace.Load<Properties>(call);

			Assert.AreEqual(output.uintTest, input.uintTest);
			Assert.AreEqual(output.doubleTest, input.doubleTest);
			Assert.AreEqual(output.stringTest, input.stringTest);
		}

		[Test, Description("Serialize Properties")]
		public void SerializeFieldInterfaceList()
		{
			FieldInterfaceList input = new FieldInterfaceList();
			input.list = new List<uint> { 1, 2, 3 };

			interFace.Save(call, input);
			FieldInterfaceList output = interFace.Load<FieldInterfaceList>(call);

			Assert.AreEqual(output.list, input.list);
		}

		[Test, Description("Serialize Field Subclass")]
		public void SerializeFieldSubclass()
		{
			DerivedClassReference input = new DerivedClassReference();
			input.baseClass = new DerivedClass();

			interFace.Save(call, input);
			DerivedClassReference output = interFace.Load<DerivedClassReference>(call);

			Assert.AreEqual(output.baseClass.a, input.baseClass.a);
		}

		[Test, Description("Serialize Type Dictionary")]
		public void SerializeTypeDictionary()
		{
			Dictionary<Type, string> input = new Dictionary<Type, string>();
			input[typeof(int)] = "integer";

			interFace.Save(call, input);
			Dictionary<Type, string> output = interFace.Load<Dictionary<Type, string>>(call);

			Assert.IsTrue(output.ContainsKey(typeof(int)));
			Assert.IsTrue(output.ContainsValue("integer"));
		}

		[Test, Description("Serialize Circular Dependency")]
		public void SerializeCircular()
		{
			Circular input = new Circular();
			input.self = input;

			interFace.Save(call, input);
			Circular output = interFace.Load<Circular>(call);

			Assert.AreEqual(output.self, output);
		}

		[Test, Description("Serialize Parent Child")]
		public void SerializeParentChild()
		{
			Parent parent = new Parent();
			Child child = new Child();
			parent.child = child;
			child.parent = parent;

			interFace.Save(call, parent);
			Parent loaded = interFace.Load<Parent>(call);

			Assert.AreEqual(loaded.child.parent, loaded);
		}

		[Test, Description("Serialize String List")]
		public void SerializeStringList()
		{
			List<string> input = new List<string>()
			{
				"abc",
				"123"
			};

			interFace.Save(call, input);
			List<string> output = interFace.Load<List<string>>(call);

			Assert.AreEqual(input[0], output[0]);
			Assert.AreEqual(input[1], output[1]);
		}

		[Test, Description("Serialize String Dictionary")]
		public void SerializeStringDictionary()
		{
			Dictionary<string, string> input = new Dictionary<string, string>();
			input["a"] = "1";
			input["b"] = "2";

			interFace.Save(call, input);
			Dictionary<string, string> output = interFace.Load<Dictionary<string, string>>(call);

			Assert.AreEqual(input["a"], output["a"]);
			Assert.AreEqual(input["b"], output["b"]);
		}

		[Test, Description("Serialize Dictionary Circular References")]
		public void SerializeDictionaryCircularReferences()
		{
			DictionaryTest input = new DictionaryTest();

			interFace.Save(call, input);
			DictionaryTest output = interFace.Load<DictionaryTest>(call);

			//Assert.AreEqual(input, output);
		}

		[Test, Description("Serialize Array")]
		public void SerializeArray()
		{
			int[] input = { 1, 2 };
			input[0] = 5;

			interFace.Save(call, input);
			int[] output = interFace.Load<int[]>(call);

			Assert.AreEqual(2, output.Length);
			Assert.AreEqual(5, output[0]);
			Assert.AreEqual(2, output[1]);
		}

		[Test, Description("Serialize HashSet")]
		public void SerializeHashSet()
		{
			HashSet<string> input = new HashSet<string>();
			input.Add("test");

			interFace.Save(call, input);
			HashSet<string> output = interFace.Load<HashSet<string>>(call);

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
			HashSet<SelectedItem> input = new HashSet<SelectedItem>();
			SelectedItem inputItem = new SelectedItem()
			{
				label = "abc",
				pinned = true,
			};
			input.Add(inputItem);

			interFace.Save(call, input);
			HashSet<SelectedItem> output = interFace.Load<HashSet<SelectedItem>>(call);

			Assert.AreEqual(input.Count, output.Count);
			//Assert.True(output.Contains("test"));
		}

		[Test, Description("Serialize Attribute NonSerialized")]
		public void SerializeAttributeNonSerialized()
		{
			NonSerializedTest input = new NonSerializedTest()
			{
				nonSerialized = 5,
				serialized = 10,
			};

			interFace.Save(call, input);
			NonSerializedTest output = interFace.Load<NonSerializedTest>(call);

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
			SubClassContainer input = new SubClassContainer();

			interFace.Save(call, input);
			SubClassContainer output = interFace.Load<SubClassContainer>(call);

			Assert.AreEqual(output.subSclass.a, input.subSclass.a);
		}

		[Test, Description("Serialize List Containing Subclass of Type")]
		public void SerializeListContainingSubclassOfType()
		{
			List<Base> input = new List<Base>();

			input.Add(new SubClass() { a = 5 });
			interFace.Save(call, input);
			List<Base> output = interFace.Load<List<Base>>(call);

			Assert.AreEqual(output[0].a, 5);
			//Assert.AreEqual(input, output); // only works on primitives
		}

		[Test, Description("Serialize Dictionary Containing Subclass of Type")]
		public void SerializeDictionaryContainingSubclassOfType()
		{
			Dictionary<Base, Base> input = new Dictionary<Base, Base>();

			Base b = new Base();
			SubClass s = new SubClass();
			s.b = 3;
			input[s] = b;
			interFace.Save(call, input);
			Dictionary<Base, Base> output = interFace.Load<Dictionary<Base, Base>>(call);

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
	}
}
/*
	
*/
