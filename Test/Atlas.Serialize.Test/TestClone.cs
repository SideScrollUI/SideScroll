using Atlas.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Atlas.Serialize.Test
{
	[Category("Clone")]
	public class TestClone : TestSerializeBase
	{
		private Log log;
		private Serializer serializer;

		[OneTimeSetUp]
		public void BaseSetup()
		{
			Initialize("Clone");
			log = Call.Log;
		}

		[SetUp]
		public void Setup()
		{
			serializer = new Serializer();
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
			var testLog = new TestLogBig();
			testLog.Child("test");

			var output = serializer.Clone(log, testLog);
		}

		[Test, Description("Clone Test Log")]
		public void CloneTestLog()
		{
			var testLog = new TestLog();
			var output = serializer.Clone(log, testLog);
		}

		[Test, Description("Clone Log Timer 2")]
		public void CloneLogTimer2()
		{
			Log testLog = new Log();
			using (testLog.Timer("timing"))
				testLog.Add("child");
			Log output = serializer.Clone(log, testLog);
		}

		[Test, Description("Clone Log")]
		public void CloneLog()
		{
			Log testLog = new Log();
			Log output = serializer.Clone(log, testLog);
		}

		[Test, Description("Clone Log Child")]
		public void CloneLogChild()
		{
			Log testLog = new Log();
			testLog.Call("test");

			Log output = serializer.Clone(log, testLog);
		}

		[Test, Description("Clone Log Timer")]
		public void CloneLogTimer()
		{
			var testLog = new LogTimer();

			var output = serializer.Clone(log, testLog);
		}

		private class MultipleArrays
		{
			public int[] Array1 = { 1, 2 };
			//public int[] Array2 = { 3, 4 };
		}

		[Test, Description("ArrayMultipleTest")]
		public void ArrayMultipleTest()
		{
			var arrays = new MultipleArrays();
			var output = serializer.Clone(log, arrays);
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

		[Test, Description("Clone Log Timer Child")]
		public void CloneLogTimerChild()
		{
			Log testLog = new Log();
			using (testLog.Timer("test")) { }

			Log output = serializer.Clone(log, testLog);
		}

		[Test, Description("Clone Properties")]
		public void CloneProperties()
		{
			var input = new Properties()
			{
				UintTest = 5,
				DoubleTest = 2.5,
				StringTest = "abc"
			};
			var output = serializer.Clone(log, input);

			Assert.AreEqual(output.UintTest, input.UintTest);
			Assert.AreEqual(output.DoubleTest, input.DoubleTest);
			Assert.AreEqual(output.StringTest, input.StringTest);
		}

		[Test, Description("Clone Primitives")]
		public void ClonePrimitives()
		{
			var input = new Primitives()
			{
				UintTest = 5,
				DoubleTest = 2.5,
				StringTest = "abc"
			};
			var output = serializer.Clone(log, input);

			Assert.AreEqual(output.UintTest, input.UintTest);
			Assert.AreEqual(output.DoubleTest, input.DoubleTest);
			Assert.AreEqual(output.StringTest, input.StringTest);
		}

		public struct StructTest
		{
			public int value;
		}

		[Test, Description("Clone Struct")]
		public void CloneStruct()
		{
			var input = new StructTest()
			{
				value = 5
			};

			var output = serializer.Clone(log, input);

			Assert.AreEqual(input, output);

			output.value = 2;

			Assert.AreNotEqual(input, output);
		}

		// DateTime has no set operators and relies on constructor
		[Test, Description("Clone DateTime Local")]
		public void CloneDateTimeLocal()
		{
			DateTime input = DateTime.Now;

			DateTime output = serializer.Clone(log, input);

			Assert.AreEqual(input, output);
		}

		// DateTime has no set operators and relies on constructor
		[Test, Description("Clone DateTime UTC")]
		public void CloneDateTimeUtc()
		{
			DateTime input = DateTime.UtcNow;

			DateTime output = serializer.Clone(log, input);

			Assert.AreEqual(input, output);
		}

		[Test, Description("Clone DateTime")]
		public void CloneNullableInt()
		{
			int? input = 1;

			int? output = serializer.Clone(log, input);

			Assert.AreEqual(output, input);
		}

		[Test, Description("Clone Nullable Primitive Properties")]
		public void CloneNullablePrimitiveProperties()
		{
			var input = new NullablePrimitiveProperties()
			{
				UintTest = 5,
				DoubleTest = 2.5
			};
			var output = serializer.Clone(log, input);

			Assert.AreEqual(output.UintTest, input.UintTest);
			Assert.AreEqual(output.DoubleTest, input.DoubleTest);
		}

		[Test, Description("Clone Nullable Primitive")]
		public void CloneNullablePrimitive()
		{
			int? input = 5;

			int? output = serializer.Clone(log, input);

			Assert.AreEqual(output, input);
		}

		[Test, Description("Clone Nullable Primitives")]
		public void CloneNullablePrimitives()
		{
			var input = new NullablePrimitives()
			{
				UintTest = 5,
				DoubleTest = 2.5
			};
			var output = serializer.Clone(log, input);

			Assert.AreEqual(output.UintTest, input.UintTest);
			Assert.AreEqual(output.DoubleTest, input.DoubleTest);
		}

		[Test, Description("Clone int")]
		public void CloneInt()
		{
			int input = 5;
			
			int output = serializer.Clone(log, input);

			Assert.AreEqual(input, output);
		}

		[Test, Description("Clone Enum")]
		public void CloneEnum()
		{
			var input = new EnumTest()
			{
				TestEnum = EnumTest.MyEnum.b,
			};
			
			var output = serializer.Clone(log, input);

			Assert.AreEqual(output.TestEnum, input.TestEnum);
		}

		[Test, Description("Clone Type")]
		public void CloneType()
		{
			Type type = typeof(string);
			
			Type output = serializer.Clone(log, type);

			Assert.AreEqual(type, output);
		}

		[Test, Description("Clone Type Dictionary")]
		public void CloneTypeDictionary()
		{
			var input = new Dictionary<Type, string>();
			input[typeof(int)] = "integer";
			
			var output = serializer.Clone(log, input);

			Assert.IsTrue(output.ContainsKey(typeof(int)));
			Assert.IsTrue(output.ContainsValue("integer"));
		}

		[Test, Description("Clone Circular Dependency")]
		public void CloneCircular()
		{
			var input = new Circular();
			input.Self = input;
			
			Circular output = serializer.Clone(log, input);

			Assert.AreEqual(output.Self, output);
		}

		[Test, Description("Clone Parent Child")]
		public void CloneParentChild()
		{
			Parent parent = new Parent();
			Child child = new Child();
			parent.Child = child;
			child.Parent = parent;
			
			Parent loaded = serializer.Clone(log, parent);

			Assert.AreEqual(loaded.Child.Parent, loaded);
		}

		[Test, Description("Clone Dictionary")]
		public void CloneDictionary()
		{
			var input = new DictionaryTest();
			
			var output = serializer.Clone(log, input);

			//Assert.AreEqual(input, output);
		}

		[Test, Description("Clone Array")]
		public void CloneArray()
		{
			int[] input = { 1, 2 };
			input[0] = 5;
			
			int[] output = serializer.Clone(log, input);

			Assert.AreEqual(2, output.Length);
			Assert.AreEqual(5, output[0]);
			Assert.AreEqual(2, output[1]);
		}

		[Test, Description("Clone HashSet")]
		public void CloneHashSet()
		{
			var input = new HashSet<string>();
			input.Add("test");
			
			var output = serializer.Clone(log, input);

			Assert.AreEqual(input.Count, output.Count);
			Assert.True(output.Contains("test"));
		}

		[Test, Description("Clone Attribute NonSerialized")]
		public void CloneAttributeNonSerialized()
		{
			var input = new NonSerializedTest()
			{
				NonSerialized = 5,
				Serialized = 10,
			};
			
			var output = serializer.Clone(log, input);

			Assert.AreEqual(output.NonSerialized, 1);
			Assert.AreEqual(output.Serialized, 10);
		}

		public class NullablePrimitiveProperties
		{
			public uint? UintTest { get; set; } = 1;
			public double? DoubleTest { get; set; } = 2.3;
		}

		public class NullablePrimitives
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
		}

		public class Circular
		{
			public Circular Self;
		}

		public class Parent
		{
			public Child Child;
		}

		public class Child
		{
			public Parent Parent;
		}

		public class DictionaryTest
		{
			public Dictionary<Parent, Child> items = new Dictionary<Parent, Child>();

			public DictionaryTest()
			{
				Parent parent = new Parent();
				Child child = new Child();
				parent.Child = child;
				child.Parent = parent;
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

			public MyEnum TestEnum = MyEnum.a;
		}


		public class NonSerializedTest
		{
			[NonSerialized]
			public int NonSerialized = 1;
			public int Serialized = 2;
		}

		
		[Test, Description("Clone List Containing Subclass of Type")]
		public void CloneSubClassContainer()
		{
			var input = new SubClassContainer();
			
			var output = serializer.Clone(log, input);

			Assert.AreEqual(output.SubClass.A, input.SubClass.A);
		}

		[Test, Description("Clone List Containing Subclass of Type")]
		public void CloneListContainingSubclassOfType()
		{
			var input = new List<Base>();

			input.Add(new SubClass() { A = 5 });
			var output = serializer.Clone(log, input);

			Assert.AreEqual(output[0].A, 5);
			//Assert.AreEqual(input, output); // only works on primitives
		}

		[Test, Description("Clone Dictionary Containing Subclass of Type")]
		public void CloneDictionaryContainingSubclassOfType()
		{
			var input = new Dictionary<Base, Base>();

			Base b = new Base();
			SubClass s = new SubClass();
			s.B = 3;
			input[s] = b;
			
			var output = serializer.Clone(log, input);

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
			public SubClass SubClass = new SubClass()
			{
				A = 3
			};
		}
	}
}
