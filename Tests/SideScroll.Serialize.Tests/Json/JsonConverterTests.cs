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

	[PublicData]
	public class UserData
	{
		public string? Username { get; set; }
		public int Score { get; set; }
		public DateTime LastLogin { get; set; }
	}

	[PublicData]
	public class ObjectDictionaryContainer
	{
		public Dictionary<string, object?>? ObjectData { get; set; }
	}

	[Test, Description("Test Dictionary<string, object?> with [PublicData] types")]
	public void SerializeDictionaryWithPublicDataObjectValues()
	{
		var input = new ObjectDictionaryContainer
		{
			ObjectData = new Dictionary<string, object?>
			{
				{ "user1", new UserData { Username = "alice", Score = 100, LastLogin = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc) } },
				{ "user2", new UserData { Username = "bob", Score = 250, LastLogin = new DateTime(2024, 2, 20, 14, 45, 0, DateTimeKind.Utc) } },
				{ "count", 42 },
				{ "message", "Hello World" },
				{ "nullValue", null }
			}
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		
		// Verify JSON contains type information for UserData objects
		Assert.That(json, Contains.Substring("$type"));
		Assert.That(json, Contains.Substring("$value"));
		Assert.That(json, Contains.Substring("UserData"));
		
		var output = JsonSerializer.Deserialize<ObjectDictionaryContainer>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.ObjectData, Is.Not.Null);
		Assert.That(output.ObjectData, Has.Count.EqualTo(5));
		
		// Verify UserData object 1
		Assert.That(output.ObjectData!["user1"], Is.InstanceOf<UserData>());
		var user1 = (UserData)output.ObjectData["user1"]!;
		Assert.That(user1.Username, Is.EqualTo("alice"));
		Assert.That(user1.Score, Is.EqualTo(100));
		Assert.That(user1.LastLogin.ToUniversalTime(), Is.EqualTo(new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc)));
		
		// Verify UserData object 2
		Assert.That(output.ObjectData["user2"], Is.InstanceOf<UserData>());
		var user2 = (UserData)output.ObjectData["user2"]!;
		Assert.That(user2.Username, Is.EqualTo("bob"));
		Assert.That(user2.Score, Is.EqualTo(250));
		Assert.That(user2.LastLogin.ToUniversalTime(), Is.EqualTo(new DateTime(2024, 2, 20, 14, 45, 0, DateTimeKind.Utc)));
		
		// Verify primitive values
		Assert.That(output.ObjectData["count"], Is.Not.Null);
		Assert.That(output.ObjectData["count"]!.ToString(), Is.EqualTo("42"));
		
		Assert.That(output.ObjectData["message"], Is.Not.Null);
		Assert.That(output.ObjectData["message"]!.ToString(), Is.EqualTo("Hello World"));
		
		// Verify null value
		Assert.That(output.ObjectData["nullValue"], Is.Null);
	}

	[Test, Description("Test Dictionary<string, object?> with mixed [PublicData] and primitive types")]
	public void SerializeDictionaryWithMixedObjectTypes()
	{
		var input = new ObjectDictionaryContainer
		{
			ObjectData = new Dictionary<string, object?>
			{
				{ "data", new UserData { Username = "charlie", Score = 500, LastLogin = DateTime.UtcNow } },
				{ "number", 123 },
				{ "text", "Sample text" },
				{ "flag", true },
				{ "list", new List<int> { 1, 2, 3 } },
				{ "decimal", 99.99m }
			}
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<ObjectDictionaryContainer>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.ObjectData, Is.Not.Null);
		Assert.That(output.ObjectData, Has.Count.EqualTo(6));
		
		// Verify [PublicData] type
		Assert.That(output.ObjectData!["data"], Is.InstanceOf<UserData>());
		var userData = (UserData)output.ObjectData["data"]!;
		Assert.That(userData.Username, Is.EqualTo("charlie"));
		Assert.That(userData.Score, Is.EqualTo(500));
		
		// Verify primitives and collections
		Assert.That(output.ObjectData["number"]!.ToString(), Is.EqualTo("123"));
		Assert.That(output.ObjectData["text"]!.ToString(), Is.EqualTo("Sample text"));
		Assert.That(output.ObjectData["flag"]!.ToString(), Is.EqualTo("True"));
		Assert.That(output.ObjectData["list"], Is.InstanceOf<List<int>>());
		Assert.That(output.ObjectData["decimal"]!.ToString(), Is.EqualTo("99.99"));
	}

	// Unregistered class without [PublicData] attribute
	public class UnregisteredUserData
	{
		public string? Username { get; set; }
		public int Score { get; set; }
	}

	[Test, Description("Test Dictionary<string, object?> blocks unregistered types without [PublicData]")]
	public void SerializeDictionaryBlocksUnregisteredTypes()
	{
		var input = new ObjectDictionaryContainer
		{
			ObjectData = new Dictionary<string, object?>
			{
				{ "unregistered", new UnregisteredUserData { Username = "blocked", Score = 999 } },
				{ "allowed", new UserData { Username = "allowed", Score = 100, LastLogin = DateTime.UtcNow } },
				{ "primitive", 42 }
			}
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<ObjectDictionaryContainer>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.ObjectData, Is.Not.Null);
		Assert.That(output.ObjectData, Has.Count.EqualTo(3));
		
		// Unregistered type should be serialized as null
		Assert.That(output.ObjectData!["unregistered"], Is.Null);
		
		// [PublicData] type should serialize correctly
		Assert.That(output.ObjectData["allowed"], Is.InstanceOf<UserData>());
		var userData = (UserData)output.ObjectData["allowed"]!;
		Assert.That(userData.Username, Is.EqualTo("allowed"));
		
		// Primitive should work
		Assert.That(output.ObjectData["primitive"]!.ToString(), Is.EqualTo("42"));
	}

	[Test, Description("Test Dictionary<string, object?> blocks raw object instances")]
	public void SerializeDictionaryBlocksRawObjects()
	{
		var input = new ObjectDictionaryContainer
		{
			ObjectData = new Dictionary<string, object?>
			{
				{ "rawObject", new object() },
				{ "allowed", new UserData { Username = "test", Score = 50, LastLogin = DateTime.UtcNow } }
			}
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<ObjectDictionaryContainer>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.ObjectData, Is.Not.Null);
		Assert.That(output.ObjectData, Has.Count.EqualTo(2));
		
		// Raw object should be serialized as null
		Assert.That(output.ObjectData!["rawObject"], Is.Null);
		
		// [PublicData] type should work
		Assert.That(output.ObjectData["allowed"], Is.InstanceOf<UserData>());
	}

	[Test, Description("Test Dictionary<string, object?> with all unregistered types returns dictionary with nulls")]
	public void SerializeDictionaryWithAllUnregisteredTypes()
	{
		var input = new ObjectDictionaryContainer
		{
			ObjectData = new Dictionary<string, object?>
			{
				{ "unregistered1", new UnregisteredUserData { Username = "user1", Score = 100 } },
				{ "unregistered2", new UnregisteredClass { Data = "data", Value = 42 } },
				{ "rawObject", new object() }
			}
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<ObjectDictionaryContainer>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.ObjectData, Is.Not.Null);
		Assert.That(output.ObjectData, Has.Count.EqualTo(3));
		
		// All unregistered types should be null
		Assert.That(output.ObjectData!["unregistered1"], Is.Null);
		Assert.That(output.ObjectData["unregistered2"], Is.Null);
		Assert.That(output.ObjectData["rawObject"], Is.Null);
	}

	public interface IUnregisteredInterface
	{
		string Name { get; set; }
	}

	// Implementation without [PublicData]
	public class UnregisteredImplementation : IUnregisteredInterface
	{
		public string Name { get; set; } = "";
		public int Value { get; set; }
	}

	[Test, Description("Test Dictionary<string, object?> blocks interface implementations without [PublicData]")]
	public void SerializeDictionaryBlocksUnregisteredInterfaceImplementations()
	{
		var input = new ObjectDictionaryContainer
		{
			ObjectData = new Dictionary<string, object?>
			{
				{ "unregisteredImpl", new UnregisteredImplementation { Name = "blocked", Value = 999 } },
				{ "registeredImpl", new Dog { Name = "Buddy", Breed = "Labrador" } }
			}
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<ObjectDictionaryContainer>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.ObjectData, Is.Not.Null);
		Assert.That(output.ObjectData, Has.Count.EqualTo(2));
		
		// Unregistered implementation should be null
		Assert.That(output.ObjectData!["unregisteredImpl"], Is.Null);
		
		// Registered [PublicData] implementation should work
		Assert.That(output.ObjectData["registeredImpl"], Is.InstanceOf<Dog>());
		var dog = (Dog)output.ObjectData["registeredImpl"]!;
		Assert.That(dog.Name, Is.EqualTo("Buddy"));
		Assert.That(dog.Breed, Is.EqualTo("Labrador"));
	}

	#endregion

	#region Interface Serialization Tests

	public interface IAnimal
	{
		string Name { get; set; }
		string Sound { get; }
	}

	[PublicData]
	public class Dog : IAnimal
	{
		public string Name { get; set; } = "";
		public string Sound => "Woof";
		public string Breed { get; set; } = "";
	}

	[PublicData]
	public class Cat : IAnimal
	{
		public string Name { get; set; } = "";
		public string Sound => "Meow";
		public int Lives { get; set; } = 9;
	}

	[PublicData]
	public class AnimalContainer
	{
		public IAnimal? Pet { get; set; }
		public IAnimal[]? Animals { get; set; }
	}

	[Test, Description("Test interface property with Dog implementation")]
	public void SerializeInterfaceWithDogImplementation()
	{
		var input = new AnimalContainer
		{
			Pet = new Dog { Name = "Buddy", Breed = "Golden Retriever" }
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		
		// Verify JSON contains type information for interface
		Assert.That(json, Contains.Substring("$type"));
		Assert.That(json, Contains.Substring("$value"));
		Assert.That(json, Contains.Substring("Dog"));
		
		var output = JsonSerializer.Deserialize<AnimalContainer>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.Pet, Is.Not.Null);
		Assert.That(output.Pet, Is.InstanceOf<Dog>());
		
		var dog = (Dog)output.Pet!;
		Assert.That(dog.Name, Is.EqualTo("Buddy"));
		Assert.That(dog.Breed, Is.EqualTo("Golden Retriever"));
		Assert.That(dog.Sound, Is.EqualTo("Woof"));
	}

	[Test, Description("Test interface property with Cat implementation")]
	public void SerializeInterfaceWithCatImplementation()
	{
		var input = new AnimalContainer
		{
			Pet = new Cat { Name = "Whiskers", Lives = 7 }
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		
		// Verify JSON contains type information for interface
		Assert.That(json, Contains.Substring("$type"));
		Assert.That(json, Contains.Substring("$value"));
		Assert.That(json, Contains.Substring("Cat"));
		
		var output = JsonSerializer.Deserialize<AnimalContainer>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.Pet, Is.Not.Null);
		Assert.That(output.Pet, Is.InstanceOf<Cat>());
		
		var cat = (Cat)output.Pet!;
		Assert.That(cat.Name, Is.EqualTo("Whiskers"));
		Assert.That(cat.Lives, Is.EqualTo(7));
		Assert.That(cat.Sound, Is.EqualTo("Meow"));
	}

	[Test, Description("Test interface array with multiple implementations")]
	public void SerializeInterfaceArrayWithMixedImplementations()
	{
		var input = new AnimalContainer
		{
			Animals = new IAnimal[]
			{
				new Dog { Name = "Max", Breed = "Labrador" },
				new Cat { Name = "Luna", Lives = 8 },
				new Dog { Name = "Charlie", Breed = "Beagle" },
			}
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		
		// Verify JSON contains type information for each array element
		Assert.That(json, Contains.Substring("Dog"));
		Assert.That(json, Contains.Substring("Cat"));
		
		var output = JsonSerializer.Deserialize<AnimalContainer>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.Animals, Is.Not.Null);
		Assert.That(output.Animals, Has.Length.EqualTo(3));
		
		// Verify first Dog
		Assert.That(output.Animals![0], Is.InstanceOf<Dog>());
		var dog1 = (Dog)output.Animals[0];
		Assert.That(dog1.Name, Is.EqualTo("Max"));
		Assert.That(dog1.Breed, Is.EqualTo("Labrador"));
		
		// Verify Cat
		Assert.That(output.Animals[1], Is.InstanceOf<Cat>());
		var cat = (Cat)output.Animals[1];
		Assert.That(cat.Name, Is.EqualTo("Luna"));
		Assert.That(cat.Lives, Is.EqualTo(8));
		
		// Verify second Dog
		Assert.That(output.Animals[2], Is.InstanceOf<Dog>());
		var dog2 = (Dog)output.Animals[2];
		Assert.That(dog2.Name, Is.EqualTo("Charlie"));
		Assert.That(dog2.Breed, Is.EqualTo("Beagle"));
	}

	[Test, Description("Test null interface property")]
	public void SerializeNullInterfaceProperty()
	{
		var input = new AnimalContainer
		{
			Pet = null,
			Animals = null
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		var output = JsonSerializer.Deserialize<AnimalContainer>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.Pet, Is.Null);
		Assert.That(output.Animals, Is.Null);
	}

	public interface IShape
	{
		double Area { get; }
	}

	[PublicData]
	public class Circle : IShape
	{
		public double Radius { get; set; }
		public double Area => Math.PI * Radius * Radius;
	}

	[PublicData]
	public class Rectangle : IShape
	{
		public double Width { get; set; }
		public double Height { get; set; }
		public double Area => Width * Height;
	}

	[PublicData]
	public class ShapeCollection
	{
		public List<IShape>? Shapes { get; set; }
	}

	[Test, Description("Test List of interfaces with different implementations")]
	public void SerializeListOfInterfaces()
	{
		var input = new ShapeCollection
		{
			Shapes = new List<IShape>
			{
				new Circle { Radius = 5.0 },
				new Rectangle { Width = 4.0, Height = 6.0 },
				new Circle { Radius = 3.0 },
			}
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		
		// Verify JSON contains type information for each element
		Assert.That(json, Contains.Substring("Circle"));
		Assert.That(json, Contains.Substring("Rectangle"));
		
		var output = JsonSerializer.Deserialize<ShapeCollection>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.Shapes, Is.Not.Null);
		Assert.That(output.Shapes, Has.Count.EqualTo(3));
		
		// Verify first Circle
		Assert.That(output.Shapes![0], Is.InstanceOf<Circle>());
		var circle1 = (Circle)output.Shapes[0];
		Assert.That(circle1.Radius, Is.EqualTo(5.0));
		Assert.That(circle1.Area, Is.EqualTo(Math.PI * 25).Within(0.0001));
		
		// Verify Rectangle
		Assert.That(output.Shapes[1], Is.InstanceOf<Rectangle>());
		var rectangle = (Rectangle)output.Shapes[1];
		Assert.That(rectangle.Width, Is.EqualTo(4.0));
		Assert.That(rectangle.Height, Is.EqualTo(6.0));
		Assert.That(rectangle.Area, Is.EqualTo(24.0));
		
		// Verify second Circle
		Assert.That(output.Shapes[2], Is.InstanceOf<Circle>());
		var circle2 = (Circle)output.Shapes[2];
		Assert.That(circle2.Radius, Is.EqualTo(3.0));
	}

	public interface INotification
	{
		string Message { get; set; }
	}

	[PublicData]
	public class EmailNotification : INotification
	{
		public string Message { get; set; } = "";
		public string EmailAddress { get; set; } = "";
	}

	[PublicData]
	public class SmsNotification : INotification
	{
		public string Message { get; set; } = "";
		public string PhoneNumber { get; set; } = "";
	}

	[PublicData]
	public class NotificationQueue
	{
		public Dictionary<string, INotification>? Notifications { get; set; }
	}

	[Test, Description("Test Dictionary with interface values")]
	public void SerializeDictionaryWithInterfaceValues()
	{
		var input = new NotificationQueue
		{
			Notifications = new Dictionary<string, INotification>
			{
				{ "user1", new EmailNotification { Message = "Welcome!", EmailAddress = "user1@example.com" } },
				{ "user2", new SmsNotification { Message = "Alert!", PhoneNumber = "+1234567890" } },
			}
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		
		// Verify JSON contains type information
		Assert.That(json, Contains.Substring("EmailNotification"));
		Assert.That(json, Contains.Substring("SmsNotification"));
		
		var output = JsonSerializer.Deserialize<NotificationQueue>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.Notifications, Is.Not.Null);
		Assert.That(output.Notifications, Has.Count.EqualTo(2));
		
		// Verify EmailNotification
		Assert.That(output.Notifications!["user1"], Is.InstanceOf<EmailNotification>());
		var email = (EmailNotification)output.Notifications["user1"];
		Assert.That(email.Message, Is.EqualTo("Welcome!"));
		Assert.That(email.EmailAddress, Is.EqualTo("user1@example.com"));
		
		// Verify SmsNotification
		Assert.That(output.Notifications["user2"], Is.InstanceOf<SmsNotification>());
		var sms = (SmsNotification)output.Notifications["user2"];
		Assert.That(sms.Message, Is.EqualTo("Alert!"));
		Assert.That(sms.PhoneNumber, Is.EqualTo("+1234567890"));
	}

	public interface IVehicle
	{
		int Speed { get; set; }
	}

	[PublicData]
	public class NestedContainer
	{
		public string? Name { get; set; }
		public IVehicle? Vehicle { get; set; }
	}

	[PublicData]
	public class Car : IVehicle
	{
		public int Speed { get; set; }
		public int Doors { get; set; }
	}

	[PublicData]
	public class OuterContainer
	{
		public string? Title { get; set; }
		public IAnimal? Animal { get; set; }
		public NestedContainer? Nested { get; set; }
	}

	[Test, Description("Test nested interfaces at multiple levels")]
	public void SerializeNestedInterfaces()
	{
		var input = new OuterContainer
		{
			Title = "Test",
			Animal = new Dog { Name = "Rex", Breed = "German Shepherd" },
			Nested = new NestedContainer
			{
				Name = "Inner",
				Vehicle = new Car { Speed = 120, Doors = 4 }
			}
		};

		string json = JsonSerializer.Serialize(input, JsonConverters.PublicSerializerOptions);
		
		// Verify JSON contains type information for both interfaces
		Assert.That(json, Contains.Substring("Dog"));
		Assert.That(json, Contains.Substring("Car"));
		
		var output = JsonSerializer.Deserialize<OuterContainer>(json, JsonConverters.PublicSerializerOptions);

		Assert.That(output, Is.Not.Null);
		Assert.That(output!.Title, Is.EqualTo("Test"));
		
		// Verify Animal interface
		Assert.That(output.Animal, Is.Not.Null);
		Assert.That(output.Animal, Is.InstanceOf<Dog>());
		var dog = (Dog)output.Animal!;
		Assert.That(dog.Name, Is.EqualTo("Rex"));
		Assert.That(dog.Breed, Is.EqualTo("German Shepherd"));
		
		// Verify nested interface
		Assert.That(output.Nested, Is.Not.Null);
		Assert.That(output.Nested!.Name, Is.EqualTo("Inner"));
		Assert.That(output.Nested.Vehicle, Is.Not.Null);
		Assert.That(output.Nested.Vehicle, Is.InstanceOf<Car>());
		var car = (Car)output.Nested.Vehicle!;
		Assert.That(car.Speed, Is.EqualTo(120));
		Assert.That(car.Doors, Is.EqualTo(4));
	}

	#endregion
}
