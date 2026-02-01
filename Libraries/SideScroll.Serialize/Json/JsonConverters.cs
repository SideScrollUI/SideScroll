using SideScroll.Attributes;
using SideScroll.Extensions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace SideScroll.Serialize.Json;

/// <summary>
/// Provides JSON serialization options and custom converters for serializing objects
/// </summary>
public static class JsonConverters
{
	/// <summary>
	/// Gets the default JSON serializer options configured for public data only, with read-only members ignored
	/// </summary>
	public static JsonSerializerOptions PublicSerializerOptions => _publicSerializerOptions ??= CreateOptions();
	private static JsonSerializerOptions? _publicSerializerOptions;

	/// <summary>
	/// Creates a new instance of JSON serializer options configured to serialize only public, writable members
	/// </summary>
	public static JsonSerializerOptions CreateOptions()
	{
		JsonSerializerOptions jsonSerializerOptions = new()
		{
			ReferenceHandler = ReferenceHandler.IgnoreCycles,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
			IgnoreReadOnlyFields = true,
			IgnoreReadOnlyProperties = true,
			WriteIndented = true,
			TypeInfoResolver = new DefaultJsonTypeInfoResolver
			{
				Modifiers =
				{
					IgnoreUnserializedAttributeModifier,
					IgnorePrivateDataAttributeModifier,
				}
			}
		};

		jsonSerializerOptions.Converters.Add(new TypeJsonConverter());
		jsonSerializerOptions.Converters.Add(new TimeZoneInfoJsonConverter());

		return jsonSerializerOptions;
	}

	private static void IgnorePrivateDataAttributeModifier(JsonTypeInfo typeInfo)
	{
		if (typeInfo.Kind != JsonTypeInfoKind.Object)
			return;

		foreach (JsonPropertyInfo property in typeInfo.Properties)
		{
			// Check if the property has PrivateDataAttribute
			if (property.AttributeProvider?.IsDefined(typeof(PrivateDataAttribute), inherit: true) == true)
			{
				property.ShouldSerialize = (_, _) => false;
				continue;
			}

			// Check if the property's type has PrivateDataAttribute
			if (property.PropertyType.IsDefined(typeof(PrivateDataAttribute), inherit: true))
			{
				property.ShouldSerialize = (_, _) => false;
			}
		}
	}

	private static void IgnoreUnserializedAttributeModifier(JsonTypeInfo typeInfo)
	{
		if (typeInfo.Kind != JsonTypeInfoKind.Object)
			return;

		foreach (JsonPropertyInfo property in typeInfo.Properties)
		{
			// Check if the property has UnserializedAttribute
			if (property.AttributeProvider?.IsDefined(typeof(UnserializedAttribute), inherit: true) == true)
			{
				property.ShouldSerialize = (_, _) => false;
			}
		}
	}
}

/// <summary>
/// JSON converter for System.Type that serializes types using their assembly qualified name
/// </summary>
public class TypeJsonConverter : JsonConverter<Type>
{
	public override Type? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.GetString() is string typeName)
		{
			return Type.GetType(typeName, throwOnError: false);
		}
		return null;
	}

	public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value?.GetAssemblyQualifiedShortName());
	}
}

/// <summary>
/// JSON converter for TimeZoneInfo that serializes using the time zone ID
/// </summary>
public class TimeZoneInfoJsonConverter : JsonConverter<TimeZoneInfo>
{
	public override TimeZoneInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.GetString() is string timeZoneId)
		{
			return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
		}
		return null;
	}

	public override void Write(Utf8JsonWriter writer, TimeZoneInfo value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value?.Id);
	}
}
