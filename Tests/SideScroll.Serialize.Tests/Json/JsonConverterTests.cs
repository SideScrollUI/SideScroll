using NUnit.Framework;
using SideScroll.Attributes;
using SideScroll.Serialize.Json;
using System.Text.Json;

namespace SideScroll.Serialize.Tests.Json;

[Category("Json")]
public class JsonConverterTests : SerializeBaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("JsonConverters");
	}

	#region PrivateData Tests

	[PublicData]
	public class PrivateDataContainer
	{
		public string? PublicData { get; set; }

		[PrivateData]
		public string? PrivateProperty { get; set; }

		[PrivateData]
		public string PrivateField = "default";
	}

	[Test, Description("Test [PrivateData] attribute blocks serialization")]
	public void SerializePrivateDataAttribute()
	{
		var input = new PrivateDataContainer
		{
			PublicData = "visible",
			PrivateProperty = "hidden property",
			PrivateField = "hidden field",
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<PrivateDataContainer>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.PublicData, Is.EqualTo("visible"));
		Assert.That(output.PrivateProperty, Is.Null);
		Assert.That(output.PrivateField, Is.EqualTo("default"));
	}

	[PrivateData]
	public class PrivateClass
	{
		public string? Confidential { get; set; }
	}

	[PublicData]
	public class ContainerWithPrivateClass
	{
		public string? PublicData { get; set; }
		public PrivateClass? PrivateObject { get; set; }
	}

	[Test, Description("Test [PrivateData] on class blocks serialization")]
	public void SerializePrivateDataClass()
	{
		var input = new PrivateClass
		{
			Confidential = "secrets",
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<PrivateClass>(json, JsonConverters.PublicSerializerOptions);

		// The [PrivateData] attribute on a class blocks all properties from serialization
		// When deserialized, this results in null since no data was serialized
		Assert.That(json, Is.EqualTo("null"));
		Assert.That(output, Is.Null);
	}

	[Test, Description("Test [PrivateData] on member class blocks serialization")]
	public void SerializePrivateDataMemberClass()
	{
		var input = new ContainerWithPrivateClass
		{
			PublicData = "visible",
			PrivateObject = new PrivateClass
			{
				Confidential = "secrets",
			},
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<ContainerWithPrivateClass>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.PublicData, Is.EqualTo("visible"));
		Assert.That(output.PrivateObject, Is.Null);
	}

	#endregion

	#region ProtectedData Tests

	[ProtectedData]
	public class ProtectedPropertiesClass
	{
		[PublicData]
		public string? PublicProperty { get; set; }

		public string? NormalProperty { get; set; }
	}

	[Test, Description("Test [ProtectedData] class with [PublicData] properties")]
	public void SerializeProtectedProperties()
	{
		var input = new ProtectedPropertiesClass
		{
			PublicProperty = "publicData",
			NormalProperty = "secrets",
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<ProtectedPropertiesClass>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.PublicProperty, Is.EqualTo("publicData"));
		Assert.That(output.NormalProperty, Is.Null);
	}

	[ProtectedData]
	public class ProtectedFieldsClass
	{
		[PublicData]
		public string? PublicField;

		public string? NormalField;
	}

	[Test, Description("Test [ProtectedData] class with [PublicData] fields")]
	public void SerializeProtectedFields()
	{
		var input = new ProtectedFieldsClass
		{
			PublicField = "publicData",
			NormalField = "secrets",
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<ProtectedFieldsClass>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.PublicField, Is.EqualTo("publicData"));
		Assert.That(output.NormalField, Is.Null);
	}

	[ProtectedData]
	public class ProtectedMixedClass
	{
		[PublicData]
		public string? PublicProperty { get; set; }

		[PublicData]
		public string? PublicField;

		public string? PrivateProperty { get; set; }

		public string? PrivateField;
	}

	[Test, Description("Test [ProtectedData] class with mixed members")]
	public void SerializeProtectedMixedMembers()
	{
		var input = new ProtectedMixedClass
		{
			PublicProperty = "visible property",
			PublicField = "visible field",
			PrivateProperty = "hidden property",
			PrivateField = "hidden field",
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<ProtectedMixedClass>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.PublicProperty, Is.EqualTo("visible property"));
		Assert.That(output.PublicField, Is.EqualTo("visible field"));
		Assert.That(output.PrivateProperty, Is.Null);
		Assert.That(output.PrivateField, Is.Null);
	}

	[PublicData]
	public class ContainerWithProtectedClass
	{
		public string? PublicData { get; set; }
		public ProtectedPropertiesClass? ProtectedObject { get; set; }
	}

	[Test, Description("Test [ProtectedData] class as member")]
	public void SerializeProtectedDataMemberClass()
	{
		var input = new ContainerWithProtectedClass
		{
			PublicData = "visible",
			ProtectedObject = new ProtectedPropertiesClass
			{
				PublicProperty = "public in protected",
				NormalProperty = "secrets",
			},
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<ContainerWithProtectedClass>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.PublicData, Is.EqualTo("visible"));
		Assert.That(output.ProtectedObject, Is.Not.Null);
		Assert.That(output.ProtectedObject!.PublicProperty, Is.EqualTo("public in protected"));
		Assert.That(output.ProtectedObject.NormalProperty, Is.Null);
	}

	#endregion

	#region Unserialized Tests

	[PublicData]
	public class UnserializedContainer
	{
		public string? SerializedData { get; set; }

		[Unserialized]
		public string? TransientProperty { get; set; }

		[Unserialized]
		public string TransientField = "default";
	}

	[Test, Description("Test [Unserialized] attribute blocks serialization")]
	public void SerializeUnserializedAttribute()
	{
		var input = new UnserializedContainer
		{
			SerializedData = "saved",
			TransientProperty = "not saved property",
			TransientField = "not saved field",
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<UnserializedContainer>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.SerializedData, Is.EqualTo("saved"));
		Assert.That(output.TransientProperty, Is.Null);
		Assert.That(output.TransientField, Is.EqualTo("default"));
	}

	#endregion

	#region TypeJsonConverter Tests

	[PublicData]
	public class TypeContainer
	{
		public Type? TypeProperty { get; set; }
		public Type[]? TypeArray { get; set; }
	}

	[Test, Description("Test TypeJsonConverter serialization")]
	public void SerializeType()
	{
		var input = new TypeContainer
		{
			TypeProperty = typeof(string),
			TypeArray = [typeof(int), typeof(List<string>), typeof(Dictionary<string, object>)],
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<TypeContainer>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.TypeProperty, Is.EqualTo(typeof(string)));
		Assert.That(output.TypeArray, Is.Not.Null);
		Assert.That(output.TypeArray, Has.Length.EqualTo(3));
		Assert.That(output.TypeArray![0], Is.EqualTo(typeof(int)));
		Assert.That(output.TypeArray[1], Is.EqualTo(typeof(List<string>)));
		Assert.That(output.TypeArray[2], Is.EqualTo(typeof(Dictionary<string, object>)));
	}

	[Test, Description("Test TypeJsonConverter with null")]
	public void SerializeNullType()
	{
		var input = new TypeContainer
		{
			TypeProperty = null,
			TypeArray = null,
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<TypeContainer>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.TypeProperty, Is.Null);
		Assert.That(output.TypeArray, Is.Null);
	}

	[Test, Description("Test TypeJsonConverter with generic types")]
	public void SerializeGenericType()
	{
		var input = new TypeContainer
		{
			TypeProperty = typeof(Dictionary<int, List<string>>),
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<TypeContainer>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.TypeProperty, Is.EqualTo(typeof(Dictionary<int, List<string>>)));
	}

	#endregion

	#region TimeZoneInfoJsonConverter Tests

	[PublicData]
	public class TimeZoneContainer
	{
		public TimeZoneInfo? TimeZone { get; set; }
		public TimeZoneInfo[]? TimeZones { get; set; }
	}

	[Test, Description("Test TimeZoneInfoJsonConverter serialization")]
	public void SerializeTimeZoneInfo()
	{
		var input = new TimeZoneContainer
		{
			TimeZone = TimeZoneInfo.Utc,
			TimeZones = [
				TimeZoneInfo.Local,
				TimeZoneInfo.Utc,
				TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"),
			],
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<TimeZoneContainer>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.TimeZone, Is.Not.Null);
		Assert.That(output.TimeZone!.Id, Is.EqualTo(TimeZoneInfo.Utc.Id));
		Assert.That(output.TimeZones, Is.Not.Null);
		Assert.That(output.TimeZones, Has.Length.EqualTo(3));
		Assert.That(output.TimeZones![0].Id, Is.EqualTo(TimeZoneInfo.Local.Id));
		Assert.That(output.TimeZones[1].Id, Is.EqualTo(TimeZoneInfo.Utc.Id));
		Assert.That(output.TimeZones[2].Id, Is.EqualTo("Pacific Standard Time"));
	}

	[Test, Description("Test TimeZoneInfoJsonConverter with null")]
	public void SerializeNullTimeZoneInfo()
	{
		var input = new TimeZoneContainer
		{
			TimeZone = null,
			TimeZones = null,
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<TimeZoneContainer>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.TimeZone, Is.Null);
		Assert.That(output.TimeZones, Is.Null);
	}

	[Test, Description("Test TimeZoneInfoJsonConverter with various time zones")]
	public void SerializeVariousTimeZones()
	{
		var timeZoneIds = new[]
		{
			"UTC",
			"Eastern Standard Time",
			"Central Standard Time",
			"Mountain Standard Time",
			"Pacific Standard Time",
		};

		var input = new TimeZoneContainer
		{
			TimeZones = timeZoneIds.Select(id => TimeZoneInfo.FindSystemTimeZoneById(id)).ToArray(),
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<TimeZoneContainer>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.TimeZones, Is.Not.Null);
		Assert.That(output.TimeZones, Has.Length.EqualTo(5));

		for (int i = 0; i < timeZoneIds.Length; i++)
		{
			Assert.That(output.TimeZones![i].Id, Is.EqualTo(timeZoneIds[i]));
		}
	}

	#endregion

	#region Uri and Guid Tests

	[PublicData]
	public class UriContainer
	{
		public Uri? UriProperty { get; set; }
		public Uri[]? UriArray { get; set; }
	}

	[Test, Description("Test Uri serialization")]
	public void SerializeUri()
	{
		var input = new UriContainer
		{
			UriProperty = new Uri("https://example.com/path?query=value"),
			UriArray = [
				new Uri("https://github.com"),
				new Uri("http://localhost:8080/api"),
				new Uri("/relative/path", UriKind.Relative)
			],
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<UriContainer>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.UriProperty, Is.Not.Null);
		Assert.That(output.UriProperty!.ToString(), Is.EqualTo("https://example.com/path?query=value"));
		Assert.That(output.UriArray, Is.Not.Null);
		Assert.That(output.UriArray, Has.Length.EqualTo(3));
		Assert.That(output.UriArray![0].ToString(), Is.EqualTo("https://github.com/"));
		Assert.That(output.UriArray[1].ToString(), Is.EqualTo("http://localhost:8080/api"));
		Assert.That(output.UriArray[2].ToString(), Is.EqualTo("/relative/path"));
	}

	[Test, Description("Test Uri with null values")]
	public void SerializeNullUri()
	{
		var input = new UriContainer
		{
			UriProperty = null,
			UriArray = null,
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<UriContainer>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.UriProperty, Is.Null);
		Assert.That(output.UriArray, Is.Null);
	}

	[PublicData]
	public class GuidContainer
	{
		public Guid GuidProperty { get; set; }
		public Guid? NullableGuidProperty { get; set; }
		public Guid[]? GuidArray { get; set; }
	}

	[Test, Description("Test Guid serialization")]
	public void SerializeGuid()
	{
		var guid1 = Guid.Parse("12345678-1234-1234-1234-123456789abc");
		var guid2 = Guid.Parse("87654321-4321-4321-4321-cba987654321");
		var guid3 = Guid.Empty;

		var input = new GuidContainer
		{
			GuidProperty = guid1,
			NullableGuidProperty = guid2,
			GuidArray = [guid1, guid2, guid3],
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<GuidContainer>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.GuidProperty, Is.EqualTo(guid1));
		Assert.That(output.NullableGuidProperty, Is.EqualTo(guid2));
		Assert.That(output.GuidArray, Is.Not.Null);
		Assert.That(output.GuidArray, Has.Length.EqualTo(3));
		Assert.That(output.GuidArray![0], Is.EqualTo(guid1));
		Assert.That(output.GuidArray[1], Is.EqualTo(guid2));
		Assert.That(output.GuidArray[2], Is.EqualTo(guid3));
	}

	[Test, Description("Test Guid with null nullable value")]
	public void SerializeNullableGuid()
	{
		var input = new GuidContainer
		{
			GuidProperty = Guid.NewGuid(),
			NullableGuidProperty = null,
			GuidArray = null,
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<GuidContainer>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.GuidProperty, Is.Not.EqualTo(Guid.Empty));
		Assert.That(output.NullableGuidProperty, Is.Null);
		Assert.That(output.GuidArray, Is.Null);
	}

	[Test, Description("Test Guid.Empty serialization")]
	public void SerializeEmptyGuid()
	{
		var input = new GuidContainer
		{
			GuidProperty = Guid.Empty,
			NullableGuidProperty = Guid.Empty,
			GuidArray = [Guid.Empty],
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<GuidContainer>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.GuidProperty, Is.EqualTo(Guid.Empty));
		Assert.That(output.NullableGuidProperty, Is.EqualTo(Guid.Empty));
		Assert.That(output.GuidArray, Is.Not.Null);
		Assert.That(output.GuidArray![0], Is.EqualTo(Guid.Empty));
	}

	#endregion

	#region Object Type Member Tests

	[Test, Description("Test serializing raw object instance is blocked")]
	public void SerializeRawObject()
	{
		object input = new object();

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<object>(json, JsonConverters.PublicSerializerOptions);

		// Raw object instances should be blocked from serialization
		Assert.That(json, Is.EqualTo("null"));
		Assert.That(output, Is.Null);
	}

	[Test, Description("Test container with object members containing raw object instances")]
	public void SerializeContainerWithRawObjectMembers()
	{
		var input = new ObjectContainerPrimitive
		{
			ObjectField = new object(),
			ObjectProperty = new object()
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<ObjectContainerPrimitive>(json, JsonConverters.PublicSerializerOptions);

		// Container should serialize, but raw object members should be null
		Assert.That(output, Is.Not.Null);
		Assert.That(output!.ObjectField, Is.Null);
		Assert.That(output.ObjectProperty, Is.Null);
	}

	public class UnregisteredClass
	{
		public string? Data { get; set; }
		public int Value { get; set; }
	}

	public class UnregisteredGenericClass<T>
	{
		public T? Value { get; set; }
		public string? Description { get; set; }
	}

	[PublicData]
	public class ObjectContainerUnregistered
	{
		public string? PublicData { get; set; }
		public object? UnregisteredObject { get; set; }
	}

	[Test, Description("Test object member containing raw object instance blocks serialization")]
	public void SerializeObjectMemberWithRawObject()
	{
		var input = new ObjectContainerUnregistered
		{
			PublicData = "visible",
			UnregisteredObject = new object()
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<ObjectContainerUnregistered>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.PublicData, Is.EqualTo("visible"));
		Assert.That(output.UnregisteredObject, Is.Null);
	}

	[Test, Description("Test object member containing unregistered class blocks serialization")]
	public void SerializeObjectWithUnregisteredClass()
	{
		var input = new ObjectContainerUnregistered
		{
			PublicData = "visible",
			UnregisteredObject = new UnregisteredClass
			{
				Data = "should not serialize",
				Value = 42
			}
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<ObjectContainerUnregistered>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.PublicData, Is.EqualTo("visible"));
		Assert.That(output.UnregisteredObject, Is.Null);
	}

	[Test, Description("Test object member containing unregistered generic class blocks serialization")]
	public void SerializeObjectWithUnregisteredGenericClass()
	{
		var input = new ObjectContainerUnregistered
		{
			PublicData = "visible",
			UnregisteredObject = new UnregisteredGenericClass<string>
			{
				Value = "should not serialize",
				Description = "custom generic type"
			}
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<ObjectContainerUnregistered>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.PublicData, Is.EqualTo("visible"));
		Assert.That(output.UnregisteredObject, Is.Null);
	}

	[PublicData]
	public class ObjectContainerPrimitive
	{
		public object? ObjectField;
		public object? ObjectProperty { get; set; }
	}

	[Test, Description("Test object members containing primitive int")]
	public void SerializeObjectWithInt()
	{
		var input = new ObjectContainerPrimitive
		{
			ObjectField = 42,
			ObjectProperty = 100
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<ObjectContainerPrimitive>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.ObjectField, Is.Not.Null);
		Assert.That(output.ObjectField!.ToString(), Is.EqualTo("42"));
		Assert.That(output.ObjectProperty, Is.Not.Null);
		Assert.That(output.ObjectProperty!.ToString(), Is.EqualTo("100"));
	}

	[Test, Description("Test object members containing primitive bool")]
	public void SerializeObjectWithBool()
	{
		var input = new ObjectContainerPrimitive
		{
			ObjectField = true,
			ObjectProperty = false
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<ObjectContainerPrimitive>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.ObjectField, Is.Not.Null);
		Assert.That(output.ObjectField!.ToString(), Is.EqualTo("True"));
		Assert.That(output.ObjectProperty, Is.Not.Null);
		Assert.That(output.ObjectProperty!.ToString(), Is.EqualTo("False"));
	}

	[Test, Description("Test object members containing primitive double")]
	public void SerializeObjectWithDouble()
	{
		var input = new ObjectContainerPrimitive
		{
			ObjectField = 3.14159,
			ObjectProperty = 2.71828
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<ObjectContainerPrimitive>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.ObjectField, Is.Not.Null);
		Assert.That(output.ObjectField!.ToString(), Is.EqualTo("3.14159"));
		Assert.That(output.ObjectProperty, Is.Not.Null);
		Assert.That(output.ObjectProperty!.ToString(), Is.EqualTo("2.71828"));
	}

	[PublicData]
	public class ObjectContainerPublicTypes
	{
		public object? StringObject;
		public object? DateTimeObject;
		public object? TimeSpanObject;
		public object? VersionObject;
	}

	[Test, Description("Test object members containing string")]
	public void SerializeObjectWithString()
	{
		var input = new ObjectContainerPublicTypes
		{
			StringObject = "Hello, World!"
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<ObjectContainerPublicTypes>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.StringObject, Is.Not.Null);
		Assert.That(output.StringObject!.ToString(), Is.EqualTo("Hello, World!"));
	}

	[Test, Description("Test object members containing DateTime")]
	public void SerializeObjectWithDateTime()
	{
		var inputDateTime = new DateTime(2024, 12, 25, 10, 30, 45, DateTimeKind.Utc);
		var input = new ObjectContainerPublicTypes
		{
			DateTimeObject = inputDateTime
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<ObjectContainerPublicTypes>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.DateTimeObject, Is.Not.Null);
		// DateTime gets serialized in ISO 8601 format, parse and convert to UTC for comparison
		var outputDateTime = DateTime.Parse(output.DateTimeObject.ToString()!).ToUniversalTime();
		Assert.That(outputDateTime, Is.EqualTo(inputDateTime));
	}

	[Test, Description("Test object members containing TimeSpan")]
	public void SerializeObjectWithTimeSpan()
	{
		var inputTimeSpan = TimeSpan.FromHours(2.5);
		var input = new ObjectContainerPublicTypes
		{
			TimeSpanObject = inputTimeSpan
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<ObjectContainerPublicTypes>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.TimeSpanObject, Is.Not.Null);
		Assert.That(output.TimeSpanObject!.ToString(), Is.EqualTo(inputTimeSpan.ToString()));
	}

	[Test, Description("Test object members containing Version")]
	public void SerializeObjectWithVersion()
	{
		var input = new ObjectContainerPublicTypes
		{
			VersionObject = new Version("2.4.1")
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<ObjectContainerPublicTypes>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.VersionObject, Is.Not.Null);
		Assert.That(output.VersionObject!.ToString(), Is.EqualTo("2.4.1"));
	}

	[PublicData]
	public class ObjectContainerGenericTypes
	{
		public object? ListObject;
		public object? DictionaryObject;
		public object? HashSetObject;
	}

	[Test, Description("Test object members containing List<int>")]
	public void SerializeObjectWithList()
	{
		var input = new ObjectContainerGenericTypes
		{
			ListObject = new List<int> { 1, 2, 3, 4, 5 }
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<ObjectContainerGenericTypes>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.ListObject, Is.Not.Null);
	}

	[Test, Description("Test object members containing Dictionary")]
	public void SerializeObjectWithDictionary()
	{
		var input = new ObjectContainerGenericTypes
		{
			DictionaryObject = new Dictionary<string, int>
			{
				{ "one", 1 },
				{ "two", 2 },
				{ "three", 3 }
			}
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<ObjectContainerGenericTypes>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.DictionaryObject, Is.Not.Null);
	}

	[Test, Description("Test object members containing HashSet")]
	public void SerializeObjectWithHashSet()
	{
		var input = new ObjectContainerGenericTypes
		{
			HashSetObject = new HashSet<string> { "red", "green", "blue" }
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<ObjectContainerGenericTypes>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.HashSetObject, Is.Not.Null);
	}

	[PublicData]
	public class ObjectContainerMixed
	{
		public object? Object1;
		public object? Object2;
		public object? Object3;
		public object? Object4;
	}

	[Test, Description("Test object members containing mixed types")]
	public void SerializeObjectWithMixedTypes()
	{
		var input = new ObjectContainerMixed
		{
			Object1 = 42,
			Object2 = "Hello",
			Object3 = new List<string> { "apple", "banana" },
			Object4 = new DateTime(2024, 1, 1)
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<ObjectContainerMixed>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.Object1, Is.Not.Null);
		Assert.That(output.Object2, Is.Not.Null);
		Assert.That(output.Object3, Is.Not.Null);
		Assert.That(output.Object4, Is.Not.Null);
	}

	#endregion

	#region Type Information Tests

	[PublicData]
	public class TypedObjectContainer
	{
		public object? ListObject { get; set; }
		public object? CustomObject { get; set; }
	}

	[PublicData]
	public class CustomTypeForRoundtrip
	{
		public string? Name { get; set; }
		public int Value { get; set; }
	}

	[Test, Description("Test object members with type information round-trip correctly")]
	public void SerializeObjectWithTypeInformationRoundtrip()
	{
		var customObj = new CustomTypeForRoundtrip
		{
			Name = "TestObject",
			Value = 123
		};

		var input = new TypedObjectContainer
		{
			ListObject = new List<int> { 1, 2, 3 },
			CustomObject = customObj
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		
		// Verify JSON contains type information for complex types
		Assert.That(json, Contains.Substring("$type"));
		Assert.That(json, Contains.Substring("$value"));
		
		var output = JsonSerializer.Deserialize<TypedObjectContainer>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.ListObject, Is.InstanceOf<List<int>>());
		Assert.That(output.CustomObject, Is.InstanceOf<CustomTypeForRoundtrip>());
		
		var outputList = (List<int>)output.ListObject!;
		Assert.That(outputList, Has.Count.EqualTo(3));
		Assert.That(outputList[0], Is.EqualTo(1));
		Assert.That(outputList[1], Is.EqualTo(2));
		Assert.That(outputList[2], Is.EqualTo(3));
		
		var outputCustom = (CustomTypeForRoundtrip)output.CustomObject!;
		Assert.That(outputCustom.Name, Is.EqualTo("TestObject"));
		Assert.That(outputCustom.Value, Is.EqualTo(123));
	}

	[Test, Description("Test simple types don't include type information")]
	public void SerializeObjectSimpleTypesWithoutTypeInfo()
	{
		var input = new ObjectContainerPrimitive
		{
			ObjectField = 42,
			ObjectProperty = "Hello"
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		
		// Verify JSON does NOT contain type information for simple types
		Assert.That(json, Does.Not.Contain("$type"));
		Assert.That(json, Does.Not.Contain("$value"));
		
		var output = JsonSerializer.Deserialize<ObjectContainerPrimitive>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.ObjectField!.ToString(), Is.EqualTo("42"));
		Assert.That(output.ObjectProperty!.ToString(), Is.EqualTo("Hello"));
	}

	[Test, Description("Test DateTime in object members doesn't include type information")]
	public void SerializeObjectDateTimeWithoutTypeInfo()
	{
		var testDate = new DateTime(2024, 6, 15, 12, 30, 0, DateTimeKind.Utc);
		var input = new ObjectContainerPublicTypes
		{
			DateTimeObject = testDate
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		
		// DateTime should serialize directly without type wrapper
		Assert.That(json, Does.Not.Contain("$type"));
		
		var output = JsonSerializer.Deserialize<ObjectContainerPublicTypes>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.DateTimeObject, Is.Not.Null);
		var outputDateTime = DateTime.Parse(output.DateTimeObject!.ToString()!).ToUniversalTime();
		Assert.That(outputDateTime, Is.EqualTo(testDate));
	}

	[PublicData]
	public class NestedObjectItem
	{
		public string? Name { get; set; }
		public int Value { get; set; }
	}

	[PublicData]
	public class ParentWithNestedObjects
	{
		public string? Title { get; set; }
		public object? NestedItem { get; set; }
	}

	[PublicData]
	public class ContainerWithObjectMember
	{
		public string? Description { get; set; }
		public object? DataValue { get; set; }
	}

	[Test, Description("Test that nested object properties don't get additional $type/$value wrappers")]
	public void SerializeNestedObjectsWithSingleLevelWrapping()
	{
		// Create a parent object with nested object? properties
		var nestedItem = new NestedObjectItem
		{
			Name = "Nested",
			Value = 42
		};

		var parent = new ParentWithNestedObjects
		{
			Title = "Parent",
			NestedItem = nestedItem
		};

		var input = new ContainerWithObjectMember
		{
			Description = "Container",
			DataValue = parent
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		
		// Verify JSON structure:
		// - DataValue should have $type/$value wrapper (top level object?)
		// - But NestedItem inside ParentWithNestedObjects should NOT have wrapper
		
		// Count occurrences of $type - should only be 2 (one for DataValue, one for NestedItem at its level)
		int typeCount = System.Text.RegularExpressions.Regex.Matches(json, @"\$type").Count;
		Assert.That(typeCount, Is.EqualTo(2), "Should have exactly 2 $type occurrences");
		
		// Verify the structure is correct
		Assert.That(json, Contains.Substring("\"DataValue\""));
		Assert.That(json, Contains.Substring("\"$type\""));
		Assert.That(json, Contains.Substring("\"$value\""));
		Assert.That(json, Contains.Substring("ParentWithNestedObjects"));
		Assert.That(json, Contains.Substring("NestedObjectItem"));
		
		// Verify NestedItem serializes cleanly within its parent
		// The JSON should show NestedItem with $type/$value at its level (since it's also object?)
		// but the properties inside NestedObjectItem (Name, Value) should be clean
		Assert.That(json, Contains.Substring("\"Name\": \"Nested\""));
		Assert.That(json, Contains.Substring("\"Value\": 42"));
		
		// Deserialize and verify round-trip
		var output = JsonSerializer.Deserialize<ContainerWithObjectMember>(json, JsonConverters.PublicSerializerOptions);
		
		Assert.That(output, Is.Not.Null);
		Assert.That(output!.Description, Is.EqualTo("Container"));
		Assert.That(output.DataValue, Is.InstanceOf<ParentWithNestedObjects>());
		
		var outputParent = (ParentWithNestedObjects)output.DataValue!;
		Assert.That(outputParent.Title, Is.EqualTo("Parent"));
		Assert.That(outputParent.NestedItem, Is.InstanceOf<NestedObjectItem>());
		
		var outputNested = (NestedObjectItem)outputParent.NestedItem!;
		Assert.That(outputNested.Name, Is.EqualTo("Nested"));
		Assert.That(outputNested.Value, Is.EqualTo(42));
	}

	#endregion
}
