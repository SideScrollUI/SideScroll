using Atlas.Core;
using NUnit.Framework;
using System;

namespace Atlas.Serialize.Test
{
	[Category("Serialize")]
	public class TestSerializeSecurity : TestSerializeBase
	{
		private SerializerMemory _serializer;

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

		public class NonSerializedTest
		{
			[NonSerialized]
			public int NonSerialized = 1;
			public int Serialized = 2;
		}

		[Test, Description("Serialize Attribute NonSerialized")]
		public void SerializeAttributeNonSerialized()
		{
			var input = new NonSerializedTest()
			{
				NonSerialized = 5,
				Serialized = 10,
			};

			_serializer.Save(Call, input);
			var output = _serializer.Load<NonSerializedTest>(Call);

			Assert.AreEqual(output.NonSerialized, 1);
			Assert.AreEqual(output.Serialized, 10);
		}

		[Unserialized]
		public class UnserializedClass
		{
			public int Value = 1;
		}

		public class UnserializedPropertyClass
		{
			public UnserializedClass UnserializedField = new UnserializedClass();
			public UnserializedClass UnserializedProperty { get; set; } = new UnserializedClass();
		}

		[Test, Description("Serialize Field and Property with [Unserialized] classes")]
		public void SerializeUnserializedPropertyClass()
		{
			var input = new UnserializedPropertyClass();
			input.UnserializedField.Value = 42;
			input.UnserializedProperty.Value = 42;

			_serializer.Save(Call, input);
			var output = _serializer.Load<UnserializedPropertyClass>(Call);

			Assert.IsNotNull(output.UnserializedField);
			Assert.IsNotNull(output.UnserializedProperty);

			Assert.AreEqual(1, output.UnserializedField.Value);
			Assert.AreEqual(1, output.UnserializedProperty.Value);
		}
	}
}
