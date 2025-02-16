using SideScroll.Attributes;
using SideScroll.Extensions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SideScroll.Serialize.Json;

// Partial implementation, only used for viewing currently
public class JsonConverters
{
	public static JsonSerializerOptions PublicJsonSerializerOptions => _publicJsonSerializerOptions ??= CreateOptions(true);
	private static JsonSerializerOptions? _publicJsonSerializerOptions;

	public static JsonSerializerOptions PrivateJsonSerializerOptions => _privateJsonSerializerOptions ??= CreateOptions(false);
	private static JsonSerializerOptions? _privateJsonSerializerOptions;

	public static JsonSerializerOptions CreateOptions(bool publicOnly)
	{
		JsonSerializerOptions jsonSerializerOptions = new()
		{
			ReferenceHandler = ReferenceHandler.Preserve,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
			IncludeFields = true,
			IgnoreReadOnlyFields = true,
			IgnoreReadOnlyProperties = true, // This doesn't work for collections :(
		};

		jsonSerializerOptions.Converters.Add(new TypeJsonConverter());
		jsonSerializerOptions.Converters.Add(new TimeZoneInfoJsonConverter());
		if (publicOnly)
		{
			jsonSerializerOptions.Converters.Add(new IgnorePrivateJsonConverterFactory());
		}

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

public class IgnorePrivateJsonConverterFactory : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert) => true; // Apply to all types

	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		if (typeToConvert.GetCustomAttribute<PrivateDataAttribute>(true) != null)
		{
			return (JsonConverter)Activator.CreateInstance(
				typeof(NullWritingConverter<>).MakeGenericType(typeToConvert))!;
		}

		JsonConverter converter = (JsonConverter)Activator.CreateInstance(
			typeof(IgnorePrivateDataAttributeConverter<>).MakeGenericType(typeToConvert))!;
		return converter;
	}

	private class IgnorePrivateDataAttributeConverter<T> : JsonConverter<T>
	{
		public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
			{
				return default;
			}

			if (typeToConvert == typeof(string))
			{
				return (T?)(object?)reader.GetString();
			}

			if (typeToConvert.IsPrimitive || typeToConvert == typeof(decimal))
			{
				return JsonSerializer.Deserialize<T>(ref reader, options);
			}

			if (typeToConvert.IsClass || typeToConvert.IsValueType)
			{
				object? instance = Activator.CreateInstance(typeToConvert);

				// Deserialize the members of the class/struct
				if (instance == null)
				{
					reader.Skip();
					return default;
				}

				// Read the JSON object start
				using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
				{
					var rootElement = doc.RootElement;

					IEnumerable<MemberInfo> members = GetSerializableMembers(typeof(T));

					foreach (MemberInfo memberInfo in members)
					{
						if (rootElement.TryGetProperty(memberInfo.Name, out JsonElement fieldValue))
						{
							if (memberInfo is FieldInfo fieldInfo)
							{
								var value = JsonSerializer.Deserialize(fieldValue.GetRawText(), fieldInfo.FieldType, options);
								fieldInfo.SetValue(instance, value);
							}
							else if (memberInfo is PropertyInfo propertyInfo)
							{
								var value = JsonSerializer.Deserialize(fieldValue.GetRawText(), propertyInfo.PropertyType, options);
								propertyInfo.SetValue(instance, value);
							}
						}
					}
				}

				return (T?)instance;
			}

			throw new JsonException($"Unsupported type: {typeToConvert}");
		}

		public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			Type valueType = value!.GetType();
			IEnumerable<MemberInfo> members = GetSerializableMembers(valueType);

			foreach (MemberInfo memberInfo in members)
			{
				object? memberValue = memberInfo switch
				{
					PropertyInfo prop => prop.GetValue(value),
					FieldInfo field => field.GetValue(value),
					_ => null
				};

				if (memberValue == null) continue;

				Type memberType = memberValue.GetType();
				if (memberType.GetCustomAttribute<PrivateDataAttribute>(true) != null)
					continue;

				// https://stackoverflow.com/questions/76061797/how-to-prevent-serialization-of-read-only-collection-properties-using-system-tex
				if (memberInfo is PropertyInfo propertyInfo && !propertyInfo.CanWrite)
				{
					continue;
				}

				string memberName = memberInfo.Name;
				writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(memberName) ?? memberName);

				if (memberValue is string text)
				{
					writer.WriteStringValue(text);
				}
				else if (memberValue is DateTime dateTime)
				{
					writer.WriteStringValue(dateTime.ToString("o"));
				}
				else if (memberType.IsPrimitive)
				{
					JsonSerializer.Serialize(writer, memberValue, memberType, options);
				}
				else
				{
					Type type = memberInfo switch
					{
						PropertyInfo prop => prop.PropertyType,
						FieldInfo field => field.FieldType,
						_ => typeof(object)
					};
					JsonSerializer.Serialize(writer, memberValue, type, options);
				}
			}

			writer.WriteEndObject();
		}

		private static List<MemberInfo> GetSerializableMembers(Type type)
		{
			var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
			var properties = type
				.GetProperties(bindingFlags)
				.Where(IsPropertySerialized);

			var fields = type
				.GetFields(bindingFlags)
				.Where(IsFieldSerialized);

			return properties
				.Cast<MemberInfo>()
				.Concat(fields)
				.ToList();
		}
		private static bool IsPropertySerialized(PropertyInfo propertyInfo)
		{
			if (propertyInfo.GetCustomAttribute<PrivateDataAttribute>(true) != null ||
				propertyInfo.PropertyType.GetCustomAttribute<PrivateDataAttribute>(true) != null ||
				propertyInfo.GetIndexParameters().Length > 0)
			{
				return false;
			}

			if (propertyInfo.GetCustomAttribute<PublicDataAttribute>(true) != null)
			{
				return true;
			}

			if (propertyInfo.DeclaringType?.GetCustomAttribute<ProtectedDataAttribute>(true) != null)
			{
				return false;
			}

			return true;
		}
		private static bool IsFieldSerialized(FieldInfo fieldInfo)
		{
			if (fieldInfo.GetCustomAttribute<PrivateDataAttribute>(true) != null ||
				fieldInfo.FieldType.GetCustomAttribute<PrivateDataAttribute>(true) != null)
			{
				return false;
			}

			if (fieldInfo.GetCustomAttribute<PublicDataAttribute>(true) != null)
			{
				return true;
			}

			if (fieldInfo.DeclaringType?.GetCustomAttribute<ProtectedDataAttribute>(true) != null)
			{
				return false;
			}

			return true;
		}
	}

	private class NullWritingConverter<T> : JsonConverter<T>
	{
		public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			reader.Skip();
			return default;
		}

		public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
		{
			writer.WriteNullValue();
		}
	}
}
