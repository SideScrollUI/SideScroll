using Atlas.Core;
using NUnit.Framework;

namespace Atlas.Serialize.Test;

[Category("Serialize")]
public class TestSerializePermissions : TestSerializeBase
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

	private readonly PrivateDataContainer privateDataContainer = new()
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

		_serializer.PublicOnly = true;
		_serializer.Save(Call, input);
		var output = _serializer.Load<PrivateDataContainer>(Call);

		Assert.IsNull(output.PrivateField.Confidential);
		Assert.IsNull(output.PrivateProperty.Confidential);
		Assert.AreEqual("test", output.PublicData);
	}

	// Test changing serialized field to public in internal model
	[Test, Description("Serialize [PrivateData]")]
	public void SerializePrivateDataPublicLoading()
	{
		var input = privateDataContainer;

		_serializer.Save(Call, input);
		_serializer.PublicOnly = true;
		var output = _serializer.Load<PrivateDataContainer>(Call);

		Assert.IsNull(output.PrivateField.Confidential);
		Assert.IsNull(output.PrivateProperty.Confidential);
		Assert.AreEqual("test", output.PublicData);
	}

	[PublicData]
	public class PrivateDataContainer
	{
		public PrivateClass PrivateField = new();
		public PrivateClass PrivateProperty { get; set; } = new();
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

		_serializer.PublicOnly = false;
		_serializer.Save(Call, input);
		var output = _serializer.Load<PublicContainer>(Call);

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

		_serializer.Save(Call, input);
		_serializer.PublicOnly = true;
		var output = _serializer.Load<PrivateClass>(Call);

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

		_serializer.PublicOnly = true;
		_serializer.Save(Call, input);
		var output = _serializer.Load<PrivatePropertyClass>(Call);

		Assert.IsNull(output.Confidential);
	}

	[PublicData]
	public class PrivateFieldClass
	{
		[PrivateData]
		public string Confidential = "default";
	}

	[Test]
	public void SerializePrivateField()
	{
		var input = new PrivateFieldClass()
		{
			Confidential = "secrets",
		};

		_serializer.PublicOnly = true;
		_serializer.Save(Call, input);
		var output = _serializer.Load<PrivateFieldClass>(Call);

		Assert.AreEqual("default", output.Confidential);
	}

	public class PublicDefaultsContainer
	{
		public PublicClass PublicField;
		public PublicClass PublicProperty { get; set; }
		public string NonSecure;
	}

	[ProtectedData]
	public class ProtectedPropertiesClass
	{
		[PublicData]
		public string PublicProperty { get; set; } // Should save

		public string NormalProperty { get; set; } // Shouldn't save
	}

	[Test]
	public void SerializeProtectedProperties()
	{
		var input = new ProtectedPropertiesClass()
		{
			PublicProperty = "publicData",
			NormalProperty = "secrets",
		};

		_serializer.PublicOnly = true;
		_serializer.Save(Call, input);
		var output = _serializer.Load<ProtectedPropertiesClass>(Call);

		Assert.AreEqual(input.PublicProperty, output.PublicProperty);
		Assert.IsNull(output.NormalProperty);
	}

	[ProtectedData]
	public class ProtectedFieldsClass
	{
		[PublicData]
		public string PublicField; // Should save

		public string NormalField; // Shouldn't save
	}

	[Test]
	public void SerializeProtectedFields()
	{
		var input = new ProtectedFieldsClass()
		{
			PublicField = "publicData",
			NormalField = "secrets",
		};

		_serializer.PublicOnly = true;
		_serializer.Save(Call, input);
		var output = _serializer.Load<ProtectedFieldsClass>(Call);

		Assert.AreEqual(input.PublicField, output.PublicField);
		Assert.IsNull(output.NormalField);
	}
}
