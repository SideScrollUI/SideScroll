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
			//ReferenceHandler = new CircularReferenceHandler(),
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
			IncludeFields = true,
			IgnoreReadOnlyFields = true,
			IgnoreReadOnlyProperties = true, // This doesn't work for collections :(
		};

		jsonSerializerOptions.Converters.Add(new TypeJsonConverter());
		jsonSerializerOptions.Converters.Add(new TimeZoneInfoJsonConverter());
		jsonSerializerOptions.Converters.Add(new PermissionsJsonConverterFactory(publicOnly));

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

public class NullWritingConverter<T> : JsonConverter<T>
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

public class PermissionsJsonConverterFactory(bool publicOnly) : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert) => true; // Apply to all types

	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		if (typeToConvert.GetCustomAttribute<UnserializedAttribute>(true) != null ||
			(publicOnly && typeToConvert.GetCustomAttribute<PrivateDataAttribute>(true) != null))
		{
			return (JsonConverter)Activator.CreateInstance(
				typeof(NullWritingConverter<>).MakeGenericType(typeToConvert))!;
		}

		JsonConverter converter = (JsonConverter)Activator.CreateInstance(
			typeof(PermissionsAttributeConverter<>).MakeGenericType(typeToConvert), [publicOnly])!;
		return converter;
	}

	private class PermissionsAttributeConverter<T>(bool publicOnly) : JsonConverter<T>
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

			if (typeToConvert == typeof(DateTime))
			{
				return (T?)(object?)reader.GetDateTime();
			}

			if (typeToConvert == typeof(DateTimeOffset))
			{
				return (T?)(object?)reader.GetDateTimeOffset();
			}

			if (typeToConvert.IsPrimitive)
			{
				return ReadPrimitive(ref reader, typeToConvert, options);
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

		public T? ReadPrimitive(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (typeToConvert == typeof(bool))
			{
				return (T?)(object?)reader.GetBoolean();
			}

			if (typeToConvert == typeof(int))
			{
				return (T?)(object?)reader.GetInt32();
			}

			if (typeToConvert == typeof(long))
			{
				return (T?)(object?)reader.GetInt64();
			}

			if (typeToConvert == typeof(decimal))
			{
				return (T?)(object?)reader.GetDecimal();
			}

			if (typeToConvert == typeof(uint))
			{
				return (T?)(object?)reader.GetUInt32();
			}

			if (typeToConvert == typeof(ulong))
			{
				return (T?)(object?)reader.GetUInt64();
			}

			if (typeToConvert == typeof(sbyte))
			{
				return (T?)(object?)reader.GetSByte();
			}

			if (typeToConvert == typeof(byte))
			{
				return (T?)(object?)reader.GetByte();
			}

			throw new JsonException("Unhandled primitive value");
		}

		public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
		{
			if (value is string text)
			{
				writer.WriteStringValue(text);
				return;
			}
			else if (value is DateTime dateTime)
			{
				writer.WriteStringValue(dateTime.ToString("o"));
				return;
			}

			if (value is bool boolean)
			{
				writer.WriteBooleanValue(boolean);
				return;
			}

			Type valueType = value!.GetType();
			if (valueType.IsNumeric())
			{
				writer.WriteNumberValue((decimal)(dynamic)value);
				return;
			}

			writer.WriteStartObject();
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
				if (memberType.GetCustomAttribute<PrivateDataAttribute>(true) != null ||
					memberType.GetCustomAttribute<UnserializedAttribute>(true) != null)
					continue;

				// Default Json serializer has an odd implementation where it outputs readonly property collections even when not needed
				// https://stackoverflow.com/questions/76061797/how-to-prevent-serialization-of-read-only-collection-properties-using-system-tex
				// These needs to be more sophosticated for custom constructors
				if (memberInfo is PropertyInfo propertyInfo && !propertyInfo.CanWrite)
				{
					continue;
				}

				string memberName = memberInfo.Name;
				writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(memberName) ?? memberName);

				if (memberValue is string textValue)
				{
					writer.WriteStringValue(textValue);
				}
				else if (memberValue is DateTime dateTime)
				{
					writer.WriteStringValue(dateTime.ToString("o"));
				}
				else if (memberType.IsNumeric())
				{
					writer.WriteNumberValue((decimal)(dynamic)memberValue);
				}
				else if (memberValue is bool b)
				{
					writer.WriteBooleanValue(b);
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

		private List<MemberInfo> GetSerializableMembers(Type type)
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

		private bool IsPropertySerialized(PropertyInfo propertyInfo)
		{
			if (propertyInfo.GetCustomAttribute<UnserializedAttribute>(true) != null ||
				propertyInfo.PropertyType.GetCustomAttribute<UnserializedAttribute>(true) != null ||
				propertyInfo.GetIndexParameters().Length > 0)
			{
				return false;
			}

			if (!publicOnly) return true;

			if (propertyInfo.GetCustomAttribute<PrivateDataAttribute>(true) != null ||
				propertyInfo.PropertyType.GetCustomAttribute<PrivateDataAttribute>(true) != null)
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

		private bool IsFieldSerialized(FieldInfo fieldInfo)
		{
			if (fieldInfo.GetCustomAttribute<NonSerializedAttribute>(true) != null || 
				fieldInfo.GetCustomAttribute<UnserializedAttribute>(true) != null ||
				fieldInfo.FieldType.GetCustomAttribute<UnserializedAttribute>(true) != null)
			{
				return false;
			}

			if (!publicOnly) return true;

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
}

// https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/preserve-references
// Can't access internal reference handler, and the PermissionsJsonConverterFactory breaks the default circular reference handling
class CircularReferenceHandler : ReferenceHandler
{
	public CircularReferenceHandler() => Reset();
	private ReferenceResolver? _rootedResolver;
	public override ReferenceResolver CreateResolver() => _rootedResolver!;
	public void Reset() => _rootedResolver = new CircularReferenceResolver();
}

class CircularReferenceResolver : ReferenceResolver
{
	private uint _referenceCount;
	private readonly Dictionary<string, object> _referenceIdToObjectMap = [];
	private readonly Dictionary<object, string> _objectToReferenceIdMap = new(ReferenceEqualityComparer.Instance);

	public override void AddReference(string referenceId, object value)
	{
		if (!_referenceIdToObjectMap.TryAdd(referenceId, value))
		{
			throw new JsonException();
		}
	}

	public override string GetReference(object value, out bool alreadyExists)
	{
		if (_objectToReferenceIdMap.TryGetValue(value, out string? referenceId))
		{
			alreadyExists = true;
		}
		else
		{
			_referenceCount++;
			referenceId = _referenceCount.ToString();
			_objectToReferenceIdMap.Add(value, referenceId);
			alreadyExists = false;
		}

		return referenceId;
	}

	public override object ResolveReference(string referenceId)
	{
		if (!_referenceIdToObjectMap.TryGetValue(referenceId, out object? value))
		{
			throw new JsonException();
		}

		return value;
	}
}
