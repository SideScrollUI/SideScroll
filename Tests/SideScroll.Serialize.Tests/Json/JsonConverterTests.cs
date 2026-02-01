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
}
