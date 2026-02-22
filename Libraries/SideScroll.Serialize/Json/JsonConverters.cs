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
			IncludeFields = true,
			WriteIndented = true,
			TypeInfoResolver = new DefaultJsonTypeInfoResolver
			{
				Modifiers =
				{
					IgnoreUnserializedAttributeModifier,
					IgnorePrivateDataAttributeModifier,
					IgnoreProtectedDataAttributeModifier,
				}
			}
		};

		jsonSerializerOptions.Converters.Add(new PrivateDataJsonConverterFactory());
		jsonSerializerOptions.Converters.Add(new TypeJsonConverter());
		jsonSerializerOptions.Converters.Add(new TimeZoneInfoJsonConverter());
		jsonSerializerOptions.Converters.Add(new ObjectJsonConverterFactory());

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

	private static void IgnoreProtectedDataAttributeModifier(JsonTypeInfo typeInfo)
	{
		if (typeInfo.Kind != JsonTypeInfoKind.Object)
			return;

		// Check if the class itself has ProtectedDataAttribute
		bool isProtectedClass = typeInfo.Type.IsDefined(typeof(ProtectedDataAttribute), inherit: true);
		
		if (!isProtectedClass)
			return;

		// For ProtectedData classes, only serialize members explicitly marked with PublicData
		foreach (JsonPropertyInfo property in typeInfo.Properties)
		{
			bool hasPublicDataAttribute = property.AttributeProvider?.IsDefined(typeof(PublicDataAttribute), inherit: true) == true;
			
			// Also check the actual field/property on the type (for fields especially)
			if (!hasPublicDataAttribute)
			{
				var memberInfo = typeInfo.Type.GetMember(property.Name, 
					System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | 
					System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static).FirstOrDefault();
				
				if (memberInfo != null)
				{
					hasPublicDataAttribute = memberInfo.IsDefined(typeof(PublicDataAttribute), inherit: true);
				}
			}
			
			if (!hasPublicDataAttribute)
			{
				property.ShouldSerialize = (_, _) => false;
			}
		}
	}
}

/// <summary>
/// JSON converter factory for types marked with PrivateDataAttribute
/// </summary>
public class PrivateDataJsonConverterFactory : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert)
	{
		return typeToConvert.IsDefined(typeof(PrivateDataAttribute), inherit: true);
	}

	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		return (JsonConverter?)Activator.CreateInstance(
			typeof(PrivateDataJsonConverter<>).MakeGenericType(typeToConvert));
	}
}

/// <summary>
/// JSON converter that serializes types marked with PrivateDataAttribute as null
/// </summary>
public class PrivateDataJsonConverter<T> : JsonConverter<T>
{
	public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		// Skip the value and return null/default
		reader.Skip();
		return default;
	}

	public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
	{
		// Write null for private data types
		writer.WriteNullValue();
	}
}

/// <summary>
/// JSON converter factory for object type that validates whether the runtime type is allowed to be serialized
/// </summary>
public class ObjectJsonConverterFactory : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert)
	{
		return typeToConvert == typeof(object) || typeToConvert.IsInterface;
	}

	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		return new ObjectJsonConverter();
	}
}

/// <summary>
/// JSON converter for object type that validates whether the runtime type is allowed to be serialized
/// This is the wrapper version that adds $type/$value at the top level
/// </summary>
public class ObjectJsonConverter : JsonConverter<object>
{
	/// <summary>
	/// Types that are allowed to be serialized when stored in object members
	/// </summary>
	public static HashSet<Type> PublicTypes { get; set; } =
	[
		typeof(string),
		typeof(DateTime),
		typeof(DateTimeOffset),
		typeof(TimeSpan),
		typeof(TimeZoneInfo),
		typeof(Type),
		typeof(Version),
		typeof(Uri),
		typeof(Guid),
		typeof(decimal),
	];

	/// <summary>
	/// Generic type definitions that are allowed to be serialized when stored in object members
	/// </summary>
	public static HashSet<Type> PublicGenericTypes { get; set; } =
	[
		typeof(List<>),
		typeof(Dictionary<,>),
		typeof(SortedDictionary<,>),
		typeof(HashSet<>),
		typeof(Nullable<>),
	];

	public override bool CanConvert(Type typeToConvert)
	{
		return typeToConvert == typeof(object) || typeToConvert.IsInterface;
	}

	public static bool IsAllowedType(Type type)
	{
		// Allow primitives
		if (type.IsPrimitive)
			return true;

		// Allow registered public types
		if (PublicTypes.Contains(type))
			return true;

		// Allow types marked with PublicData
		if (type.IsDefined(typeof(PublicDataAttribute), inherit: true))
			return true;

		// Allow types marked with ProtectedData
		if (type.IsDefined(typeof(ProtectedDataAttribute), inherit: true))
			return true;

		// Allow generic collections of allowed types
		if (type.IsGenericType)
		{
			Type genericTypeDef = type.GetGenericTypeDefinition();
			if (PublicGenericTypes.Contains(genericTypeDef))
			{
				return true;
			}
		}

		// Block everything else (including unregistered custom classes)
		return false;
	}

	public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		// For deserialization, check if there's type information to deserialize to the correct type
		using var jsonDoc = JsonDocument.ParseValue(ref reader);
		
		// Check for type information in objects
		if (jsonDoc.RootElement.ValueKind == JsonValueKind.Object && 
		    jsonDoc.RootElement.TryGetProperty("$type", out var typeProperty) &&
		    jsonDoc.RootElement.TryGetProperty("$value", out var valueProperty))
		{
			string? typeName = typeProperty.GetString();
			if (typeName != null)
			{
				Type? actualType = Type.GetType(typeName, throwOnError: false);
				if (actualType != null && IsAllowedType(actualType))
				{
					try
					{
						// Deserialize the $value property as the actual type
						return JsonSerializer.Deserialize(valueProperty.GetRawText(), actualType, options);
					}
					catch
					{
						// If deserialization fails, fall through to default behavior
					}
				}
			}
		}
		
		// Default deserialization without type information
		return jsonDoc.RootElement.ValueKind switch
		{
			JsonValueKind.Null => null,
			JsonValueKind.True => true,
			JsonValueKind.False => false,
			JsonValueKind.Number => jsonDoc.RootElement.TryGetInt32(out int i) ? i :
			                        jsonDoc.RootElement.TryGetInt64(out long l) ? l :
			                        jsonDoc.RootElement.TryGetDouble(out double d) ? d :
			                        (object)jsonDoc.RootElement.GetDecimal(),
			JsonValueKind.String => jsonDoc.RootElement.GetString(),
			JsonValueKind.Array => jsonDoc.RootElement.Deserialize<List<object?>>(options),
			JsonValueKind.Object => jsonDoc.RootElement.Deserialize<Dictionary<string, object?>>(options),
			_ => null
		};
	}

	public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}

		Type runtimeType = value.GetType();

		// Check if type is allowed
		if (!IsAllowedType(runtimeType))
		{
			// Block unregistered types - write null instead
			writer.WriteNullValue();
			return;
		}

		// For primitives and simple types that don't need type information, serialize directly
		if (runtimeType.IsPrimitive || 
		    runtimeType == typeof(string) || 
		    runtimeType == typeof(decimal) ||
		    runtimeType == typeof(DateTime) ||
		    runtimeType == typeof(DateTimeOffset) ||
		    runtimeType == typeof(TimeSpan) ||
		    runtimeType == typeof(Guid))
		{
			JsonSerializer.Serialize(writer, value, runtimeType, options);
			return;
		}

		// For complex types (custom classes, collections, etc.), include type information
		// Serialize with type information wrapper
		writer.WriteStartObject();
		writer.WriteString("$type", runtimeType.GetAssemblyQualifiedShortName());
		writer.WritePropertyName("$value");
		// Serialize the value with its actual runtime type
		JsonSerializer.Serialize(writer, value, runtimeType, options);
		writer.WriteEndObject();
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
