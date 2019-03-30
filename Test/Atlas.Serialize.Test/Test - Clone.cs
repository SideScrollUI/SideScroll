using System;
using System.Collections.Generic;
using System.Diagnostics;
using Atlas.Core;
using NUnit.Framework;

namespace Atlas.Serialize.Test
{
	[Category("Clone")]
	public class CloneTest : TestSerializeBase
	{
		private Log log;
		
		[OneTimeSetUp]
		public void BaseSetup()
		{
			Initialize("Clone");
			log = call.log;
			
			string basePath = Paths.Combine(TestPath, "Clone");
		}

		class TestLog
		{
			public enum LogType
			{
				Debug,
				Tab,
				Call,
				Info,
				Warn,
				Error,
				Alert
			}
			//public event PropertyChangedEventHandler PropertyChanged;
			//private Settings settings;
			// Change everything to tags? const for created/message/childLog? harder to use then
			//public DateTime Created;// { get; set; }
			public LogType Type { get; set; }
			//public string Text;// { get; set; }
			public int Entries { get; set; }
		}

		[Test, Description("Clone Test Log Big")]
		public void CloneTestLogBig()
		{
			TestLogBig testLog = new TestLogBig();
			testLog.Child("test");

			Serializer serializer = new Serializer();
			TestLogBig output = serializer.Clone<TestLogBig>(log, testLog);
		}

		[Test, Description("Clone Test Log")]
		public void CloneTestLog()
		{
			TestLog testLog = new TestLog();
			Serializer serializer = new Serializer();
			TestLog output = serializer.Clone<TestLog>(log, testLog);
		}

		[Test, Description("Clone Log Timer 2")]
		public void CloneLogTimer2()
		{
			Log testLog = new Log();
			using (testLog.Timer("timing"))
				testLog.Add("child");
			Serializer serializer = new Serializer();
			Log output = serializer.Clone<Log>(log, testLog);
		}

		[Test, Description("Clone Log")]
		public void CloneLog()
		{
			Log testLog = new Log();
			Serializer serializer = new Serializer();
			Log output = serializer.Clone<Log>(log, testLog);
		}

		[Test, Description("Clone Log Child")]
		public void CloneLogChild()
		{
			Log testLog = new Log();
			testLog.Call("test");

			Serializer serializer = new Serializer();
			Log output = serializer.Clone<Log>(log, testLog);
		}

		[Test, Description("Clone Log Timer")]
		public void CloneLogTimer()
		{
			LogTimer testLog = new LogTimer();

			Serializer serializer = new Serializer();
			LogTimer output = serializer.Clone<LogTimer>(log, testLog);
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
			Serializer serializer = new Serializer();
			MultipleArrays output = serializer.Clone<MultipleArrays>(log, arrays);
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

		[Test, Description("Clone Log Timer Child")]
		public void CloneLogTimerChild()
		{
			Log testLog = new Log();
			using (testLog.Timer("test")) { }

			Serializer serializer = new Serializer();
			Log output = serializer.Clone<Log>(log, testLog);
		}

		[Test, Description("Clone Properties")]
		public void CloneProperties()
		{
			Properties input = new Properties()
			{
				uintTest = 5,
				doubleTest = 2.5,
				stringTest = "abc"
			};
			Serializer serializer = new Serializer();
			Properties output = serializer.Clone<Properties>(log, input);

			Assert.AreEqual(output.uintTest, input.uintTest);
			Assert.AreEqual(output.doubleTest, input.doubleTest);
			Assert.AreEqual(output.stringTest, input.stringTest);
		}

		[Test, Description("Clone Primitives")]
		public void ClonePrimitives()
		{
			Primitives input = new Primitives()
			{
				uintTest = 5,
				doubleTest = 2.5,
				stringTest = "abc"
			};
			Serializer serializer = new Serializer();
			Primitives output = serializer.Clone<Primitives>(log, input);

			Assert.AreEqual(output.uintTest, input.uintTest);
			Assert.AreEqual(output.doubleTest, input.doubleTest);
			Assert.AreEqual(output.stringTest, input.stringTest);
		}

		public struct StructTest
		{
			public int value;
		}

		[Test, Description("Clone Struct")]
		public void CloneStruct()
		{
			StructTest input = new StructTest()
			{
				value = 5
			};

			Serializer serializer = new Serializer();
			StructTest output = serializer.Clone<StructTest>(log, input);

			Assert.AreEqual(input, output);

			output.value = 2;

			Assert.AreNotEqual(input, output);
		}

		// DateTime has no set operators and relies on constructor
		[Test, Description("Clone DateTime Local")]
		public void CloneDateTimeLocal()
		{
			DateTime input = DateTime.Now;

			Serializer serializer = new Serializer();
			DateTime output = serializer.Clone<DateTime>(log, input);

			Assert.AreEqual(input, output);
		}

		// DateTime has no set operators and relies on constructor
		[Test, Description("Clone DateTime UTC")]
		public void CloneDateTimeUtc()
		{
			DateTime input = DateTime.UtcNow;

			Serializer serializer = new Serializer();
			DateTime output = serializer.Clone<DateTime>(log, input);

			Assert.AreEqual(input, output);
		}

		[Test, Description("Clone DateTime")]
		public void CloneNullableInt()
		{
			int? input = 1;

			Serializer serializer = new Serializer();
			int? output = serializer.Clone<int?>(log, input);

			Assert.AreEqual(output, input);
		}

		[Test, Description("Clone Nullable Primitive Properties")]
		public void CloneNullablePrimitiveProperties()
		{
			NullablePrimitiveProperties input = new NullablePrimitiveProperties()
			{
				uintTest = 5,
				doubleTest = 2.5
			};
			Serializer serializer = new Serializer();
			NullablePrimitiveProperties output = serializer.Clone<NullablePrimitiveProperties>(log, input);

			Assert.AreEqual(output.uintTest, input.uintTest);
			Assert.AreEqual(output.doubleTest, input.doubleTest);
		}

		[Test, Description("Clone Nullable Primitive")]
		public void CloneNullablePrimitive()
		{
			int? input = 5;

			Serializer serializer = new Serializer();
			int? output = serializer.Clone<int?>(log, input);

			Assert.AreEqual(output, input);
		}

		[Test, Description("Clone Nullable Primitives")]
		public void CloneNullablePrimitives()
		{
			NullablePrimitives input = new NullablePrimitives()
			{
				uintTest = 5,
				doubleTest = 2.5
			};
			Serializer serializer = new Serializer();
			NullablePrimitives output = serializer.Clone<NullablePrimitives>(log, input);

			Assert.AreEqual(output.uintTest, input.uintTest);
			Assert.AreEqual(output.doubleTest, input.doubleTest);
		}

		[Test, Description("Clone int")]
		public void CloneInt()
		{
			int input = 5;
			
			Serializer serializer = new Serializer();
			int output = serializer.Clone<int>(log, input);

			Assert.AreEqual(input, output);
		}

		[Test, Description("Clone Enum")]
		public void CloneEnum()
		{
			EnumTest input = new EnumTest();
			input.testEnum = EnumTest.MyEnum.b;
			
			Serializer serializer = new Serializer();
			EnumTest output = serializer.Clone<EnumTest>(log, input);

			Assert.AreEqual(output.testEnum, input.testEnum);
		}

		[Test, Description("Clone Type")]
		public void CloneType()
		{
			Type type = typeof(string);
			
			Serializer serializer = new Serializer();
			Type output = serializer.Clone<Type>(log, type);

			Assert.AreEqual(type, output);
		}

		[Test, Description("Clone Type Dictionary")]
		public void CloneTypeDictionary()
		{
			Dictionary<Type, string> input = new Dictionary<Type, string>();
			input[typeof(int)] = "integer";
			
			Serializer serializer = new Serializer();
			Dictionary<Type, string> output = serializer.Clone<Dictionary<Type, string>>(log, input);

			Assert.IsTrue(output.ContainsKey(typeof(int)));
			Assert.IsTrue(output.ContainsValue("integer"));
		}

		[Test, Description("Clone Circular Dependency")]
		public void CloneCircular()
		{
			Circular input = new Circular();
			input.self = input;
			
			Serializer serializer = new Serializer();
			Circular output = serializer.Clone<Circular>(log, input);

			Assert.AreEqual(output.self, output);
		}

		[Test, Description("Clone Parent Child")]
		public void CloneParentChild()
		{
			Parent parent = new Parent();
			Child child = new Child();
			parent.child = child;
			child.parent = parent;
			
			Serializer serializer = new Serializer();
			Parent loaded = serializer.Clone<Parent>(log, parent);

			Assert.AreEqual(loaded.child.parent, loaded);
		}

		[Test, Description("Clone Dictionary")]
		public void CloneDictionary()
		{
			DictionaryTest input = new DictionaryTest();
			
			Serializer serializer = new Serializer();
			DictionaryTest output = serializer.Clone<DictionaryTest>(log, input);

			//Assert.AreEqual(input, output);
		}

		[Test, Description("Clone Array")]
		public void CloneArray()
		{
			int[] input = { 1, 2 };
			input[0] = 5;
			
			Serializer serializer = new Serializer();
			int[] output = serializer.Clone<int[]>(log, input);

			Assert.AreEqual(2, output.Length);
			Assert.AreEqual(5, output[0]);
			Assert.AreEqual(2, output[1]);
		}

		[Test, Description("Clone HashSet")]
		public void CloneHashSet()
		{
			HashSet<string> input = new HashSet<string>();
			input.Add("test");
			
			Serializer serializer = new Serializer();
			HashSet<string> output = serializer.Clone<HashSet<string>>(log, input);

			Assert.AreEqual(input.Count, output.Count);
			Assert.True(output.Contains("test"));
		}

		[Test, Description("Clone Attribute NonSerialized")]
		public void CloneAttributeNonSerialized()
		{
			NonSerializedTest input = new NonSerializedTest();
			input.nonSerialized = 5;
			input.serialized = 10;
			
			Serializer serializer = new Serializer();
			NonSerializedTest output = serializer.Clone<NonSerializedTest>(log, input);

			Assert.AreEqual(output.nonSerialized, 1);
			Assert.AreEqual(output.serialized, 10);
		}

		public class NullablePrimitiveProperties
		{
			public uint? uintTest { get; set; } = 1;
			public double? doubleTest { get; set; } = 2.3;
		}

		public class NullablePrimitives
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

		public class EnumTest
		{
			public enum MyEnum
			{
				a,
				b
			}

			public MyEnum testEnum = MyEnum.a;
		}


		public class NonSerializedTest
		{
			[NonSerialized]
			public int nonSerialized = 1;
			public int serialized = 2;
		}

		
		[Test, Description("Clone List Containing Subclass of Type")]
		public void CloneSubClassContainer()
		{
			SubClassContainer input = new SubClassContainer();
			
			Serializer serializer = new Serializer();
			SubClassContainer output = serializer.Clone<SubClassContainer>(log, input);

			Assert.AreEqual(output.subSclass.A, input.subSclass.A);
		}

		[Test, Description("Clone List Containing Subclass of Type")]
		public void CloneListContainingSubclassOfType()
		{
			List<Base> input = new List<Base>();

			input.Add(new SubClass() { A = 5 });
			Serializer serializer = new Serializer();
			List<Base> output = serializer.Clone<List<Base>>(log, input);

			Assert.AreEqual(output[0].A, 5);
			//Assert.AreEqual(input, output); // only works on primitives
		}

		[Test, Description("Clone Dictionary Containing Subclass of Type")]
		public void CloneDictionaryContainingSubclassOfType()
		{
			Dictionary<Base, Base> input = new Dictionary<Base, Base>();

			Base b = new Base();
			SubClass s = new SubClass();
			s.B = 3;
			input[s] = b;
			
			Serializer serializer = new Serializer();
			Dictionary<Base, Base> output = serializer.Clone<Dictionary<Base, Base>>(log, input);

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
	}
}
/*
	
*/
