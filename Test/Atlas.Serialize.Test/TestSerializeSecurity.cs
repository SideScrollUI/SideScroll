using Atlas.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Atlas.Serialize.Test
{
	[Category("Serialize")]
	public class TestSerializeSecurity : TestSerializeBase
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
			serializer = new SerializerMemoryAtlas();
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

			serializer.Save(Call, input);
			var output = serializer.Load<NonSerializedTest>(Call);

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

			serializer.Save(Call, input);
			var output = serializer.Load<UnserializedPropertyClass>(Call);

			Assert.IsNotNull(output.UnserializedField);
			Assert.IsNotNull(output.UnserializedProperty);

			Assert.AreEqual(1, output.UnserializedField.Value);
			Assert.AreEqual(1, output.UnserializedProperty.Value);
		}

		private PrivateDataContainer privateDataContainer = new PrivateDataContainer()
		{
			PrivateField = new PrivateClass()
			{
				Confidential = "secrets",
			},
			PrivateProperty = new PrivateClass()
			{
				Confidential = "more secrets",
			},
			PublicData = "test",
		};

		[Test, Description("Serialize [PrivateData]")]
		public void SerializePrivateData()
		{
			var input = privateDataContainer;

			serializer.PublicOnly = true;
			serializer.Save(Call, input);
			var output = serializer.Load<PrivateDataContainer>(Call);

			Assert.IsNull(output.PrivateField);
			Assert.IsNull(output.PrivateProperty);
			Assert.AreEqual("test", output.PublicData);
		}

		// Test changing serialized field to public in internal model
		[Test, Description("Serialize [PrivateData]")]
		public void SerializePrivateDataPublicLoading()
		{
			var input = privateDataContainer;

			serializer.Save(Call, input);
			serializer.PublicOnly = true;
			var output = serializer.Load<PrivateDataContainer>(Call);

			Assert.IsNull(output.PrivateField);
			Assert.IsNull(output.PrivateProperty);
			Assert.AreEqual("test", output.PublicData);
		}

		[PublicData]
		public class PrivateDataContainer
		{
			public PrivateClass PrivateField;
			public PrivateClass PrivateProperty { get; set; }
			public string PublicData;
		}

		[PrivateData]
		public class PrivateClass
		{
			public string Confidential { get; set; }
		}

		public class DerivedPrivateClass : PrivateClass
		{
		}

		[Test, Description("Serialize [PublicData]")]
		public void SerializePublicData()
		{
			var input = new PublicContainer()
			{
				PublicField = new PublicClass()
				{
					PublicData = "cats",
				},
				PublicProperty = new PublicClass()
				{
					PublicData = "more cats",
				},
				NonSecure = "test",
			};

			serializer.PublicOnly = false;
			serializer.Save(Call, input);
			var output = serializer.Load<PublicContainer>(Call);

			Assert.IsNotNull(output.PublicField);
			Assert.IsNotNull(output.PublicProperty);
			Assert.AreEqual(output.NonSecure, "test");
		}

		public class PublicContainer
		{
			public PublicClass PublicField;
			public PublicClass PublicProperty { get; set; }
			public string NonSecure;
		}

		[PublicData]
		public class PublicClass
		{
			public string PublicData { get; set; }
			public object RestrictedData { get; set; } // Only allows [PublicData]
		}

		[Test]
		public void SerializeDerivedNonPrivateClass()
		{
			var input = new DerivedPrivateClass()
			{
				Confidential = "secrets",
			};

			serializer.Save(Call, input);
			serializer.PublicOnly = true;
			var output = serializer.Load<PrivateClass>(Call);

			Assert.IsNull(output);
		}

		[PublicData]
		public class PrivatePropertyClass
		{
			[PrivateData]
			public string Confidential { get; set; }
		}

		[Test]
		public void SerializePrivateProperty()
		{
			var input = new PrivatePropertyClass()
			{
				Confidential = "secrets",
			};

			serializer.PublicOnly = true;
			serializer.Save(Call, input);
			var output = serializer.Load<PrivatePropertyClass>(Call);

			Assert.IsNull(output.Confidential);
		}

		[PublicData]
		public class PrivateFieldClass
		{
			[PrivateData]
			public string Confidential;
		}

		[Test]
		public void SerializePrivateField()
		{
			var input = new PrivateFieldClass()
			{
				Confidential = "secrets",
			};

			serializer.PublicOnly = true;
			serializer.Save(Call, input);
			var output = serializer.Load<PrivateFieldClass>(Call);

			Assert.IsNull(output.Confidential);
		}
	}
}
