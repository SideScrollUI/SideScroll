using NUnit.Framework;
using SideScroll.Attributes;
using SideScroll.Serialize.Atlas;

namespace SideScroll.Serialize.Test;

[Category("Serialize")]
public class TestSerializePermissions : TestSerializeBase
{
	private SerializerMemory _serializer = new SerializerMemoryAtlas();

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

	private readonly PrivateDataContainer _privateDataContainer = new()
	{
		PrivateField = new PrivateClass
		{
			Confidential = "secrets",
		},
		PrivateProperty = new PrivateClass
		{
			Confidential = "more secrets",
		},
		PublicData = "test",
	};

	[Test, Description("Serialize [PrivateData]")]
	public void SerializePrivateData()
	{
		var input = _privateDataContainer;

		_serializer.PublicOnly = true;
		_serializer.Save(Call, input);
		var output = _serializer.Load<PrivateDataContainer>(Call);

		Assert.That(output.PrivateField.Confidential, Is.Null);
		Assert.That(output.PrivateProperty.Confidential, Is.Null);
		Assert.That(output.PublicData, Is.EqualTo("test"));
	}

	// Test changing serialized field to public in internal model
	[Test, Description("Serialize [PrivateData]")]
	public void SerializePrivateDataPublicLoading()
	{
		var input = _privateDataContainer;

		_serializer.Save(Call, input);
		_serializer.PublicOnly = true;
		var output = _serializer.Load<PrivateDataContainer>(Call);

		Assert.That(output.PrivateField.Confidential, Is.Null);
		Assert.That(output.PrivateProperty.Confidential, Is.Null);
		Assert.That(output.PublicData, Is.EqualTo("test"));
	}

	[PublicData]
	public class PrivateDataContainer
	{
		public PrivateClass PrivateField = new();
		public PrivateClass PrivateProperty { get; set; } = new();
		public string? PublicData;
	}

	[PrivateData]
	public class PrivateClass
	{
		public string? Confidential { get; set; }
	}

	public class DerivedPrivateClass : PrivateClass;

	[Test, Description("Serialize [PublicData]")]
	public void SerializePublicData()
	{
		var input = new PublicContainer
		{
			PublicField = new PublicClass
			{
				PublicData = "cats",
			},
			PublicProperty = new PublicClass
			{
				PublicData = "more cats",
			},
			NonSecure = "test",
		};

		_serializer.PublicOnly = false;
		_serializer.Save(Call, input);
		var output = _serializer.Load<PublicContainer>(Call);

		Assert.That(output.PublicField, Is.Not.Null);
		Assert.That(output.PublicProperty, Is.Not.Null);
		Assert.That(output.NonSecure, Is.EqualTo("test"));
	}

	public class PublicContainer
	{
		public PublicClass? PublicField;
		public PublicClass? PublicProperty { get; set; }
		public string? NonSecure;
	}

	[PublicData]
	public class PublicClass
	{
		public string? PublicData { get; set; }
		public object? RestrictedData { get; set; } // Only allows [PublicData]
	}

	[Test]
	public void SerializeDerivedNonPrivateClass()
	{
		var input = new DerivedPrivateClass
		{
			Confidential = "secrets",
		};

		_serializer.Save(Call, input);
		_serializer.PublicOnly = true;

		Assert.That(() => _serializer.Load<PrivateClass>(Call), Throws.Exception.TypeOf<Exception>());
	}

	[PublicData]
	public class PrivatePropertyClass
	{
		[PrivateData]
		public string? Confidential { get; set; }
	}

	[Test]
	public void SerializePrivateProperty()
	{
		var input = new PrivatePropertyClass
		{
			Confidential = "secrets",
		};

		_serializer.PublicOnly = true;
		_serializer.Save(Call, input);
		var output = _serializer.Load<PrivatePropertyClass>(Call);

		Assert.That(output.Confidential, Is.Null);
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
		var input = new PrivateFieldClass
		{
			Confidential = "secrets",
		};

		_serializer.PublicOnly = true;
		_serializer.Save(Call, input);
		var output = _serializer.Load<PrivateFieldClass>(Call);

		Assert.That(output.Confidential, Is.EqualTo("default"));
	}

	public class PublicDefaultsContainer
	{
		public PublicClass? PublicField;
		public PublicClass? PublicProperty { get; set; }
		public string? NonSecure;
	}

	[ProtectedData]
	public class ProtectedPropertiesClass
	{
		[PublicData]
		public string? PublicProperty { get; set; } // Should save

		public string? NormalProperty { get; set; } // Shouldn't save
	}

	[Test]
	public void SerializeProtectedProperties()
	{
		var input = new ProtectedPropertiesClass
		{
			PublicProperty = "publicData",
			NormalProperty = "secrets",
		};

		_serializer.PublicOnly = true;
		_serializer.Save(Call, input);
		var output = _serializer.Load<ProtectedPropertiesClass>(Call);

		Assert.That(output.PublicProperty, Is.EqualTo(input.PublicProperty));
		Assert.That(output.NormalProperty, Is.Null);
	}

	[ProtectedData]
	public class ProtectedFieldsClass
	{
		[PublicData]
		public string? PublicField; // Should save

		public string? NormalField; // Shouldn't save
	}

	[Test]
	public void SerializeProtectedFields()
	{
		var input = new ProtectedFieldsClass
		{
			PublicField = "publicData",
			NormalField = "secrets",
		};

		_serializer.PublicOnly = true;
		_serializer.Save(Call, input);
		var output = _serializer.Load<ProtectedFieldsClass>(Call);

		Assert.That(output.PublicField, Is.EqualTo(input.PublicField));
		Assert.That(output.NormalField, Is.Null);
	}
}
