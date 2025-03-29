using SideScroll.Extensions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SideScroll.Serialize.Json;

// Partial implementation, only used for viewing currently
public class JsonConverters
{
	public static JsonSerializerOptions JsonSerializerOptions => _jsonSerializerOptions ??= CreateOptions();
	private static JsonSerializerOptions? _jsonSerializerOptions;

	public static JsonSerializerOptions CreateOptions()
	{
		JsonSerializerOptions jsonSerializerOptions = new()
		{
			ReferenceHandler = ReferenceHandler.IgnoreCycles,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
			IgnoreReadOnlyFields = true,
			IgnoreReadOnlyProperties = true,
		};

		jsonSerializerOptions.Converters.Add(new TypeJsonConverter());
		jsonSerializerOptions.Converters.Add(new TimeZoneInfoJsonConverter());

		return jsonSerializerOptions;
	}
}

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
