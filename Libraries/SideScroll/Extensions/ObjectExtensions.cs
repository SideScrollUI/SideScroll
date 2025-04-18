using SideScroll.Attributes;
using System.Collections;
using System.Reflection;

namespace SideScroll.Extensions;

public static class ObjectExtensions
{
	public static int DefaultMaxLength { get; set; } = 500;

	public static string? Formatted(this object? obj, int? maxLength = null)
	{
		if (obj == null)
			return null;

		Type type = obj.GetType();

		if (type.IsNumeric())
		{
			if (obj is double d)
			{
				// todo: Make more generic
				if (Math.Abs(d) < 0.001)
				{
					return d.ToString("#,0.######");
				}
				else
				{
					return d.ToString("#,0.###");
				}
			}

			MethodInfo toStringMethod = type.GetMethod("ToString", [typeof(string)])!;
			string format = type.IsDecimal() ? "G" : "N0";
			object? result = toStringMethod.Invoke(obj, [format]);
			return (string?)result;
		}

		int maxFormatLength = maxLength ?? DefaultMaxLength;

		if (type.IsPrimitive == false)
		{
			if (obj is DateTime dateTime)
			{
				return dateTime.Format();
			}

			if (obj is TimeSpan timeSpan)
			{
				if (timeSpan.TotalSeconds < 1)
				{
					return timeSpan.Trim(TimeSpan.FromMilliseconds(1)).ToString("g");
				}
				else
				{
					return timeSpan.FormattedDecimal();
				}
			}

			if (type.IsEnum)
			{
				return obj.ToString().WordSpaced();
			}

			// use any ToString() that overrides the base
			MethodInfo toStringMethod = type.GetMethod("ToString", Type.EmptyTypes)!;
			if (toStringMethod.DeclaringType != typeof(object) && toStringMethod.DeclaringType != typeof(ValueType))
			{
				string? toString = obj.ToString();
				if (toString != null && toString.Length > maxFormatLength)
				{
					return toString[..maxFormatLength];
				}
				return toString;
			}
		}

		if (obj is string text)
		{
			if (text.Length > maxFormatLength)
			{
				return text[..maxFormatLength];
			}

			return text;
		}
		else if (obj is IDictionary dictionary)
		{
			return "{ " + dictionary.Count.ToString("N0") + " }";
		}
		else if (obj is ICollection collection)
		{
			Type? elementType = type.GetElementTypeForAll();
			if (elementType?.GetCustomAttribute<ToStringAttribute>() != null)
			{
				return CollectionToString(collection);
			}
			return collection.Count.ToString("N0");
		}

		if (typeof(IEnumerable).IsAssignableFrom(type))
		{
			PropertyInfo? countProp = type.GetProperty("Count");
			if (countProp != null)
			{
				int count = (int)countProp.GetValue(obj, null)!;
				return count.ToString("N0");
			}
		}

		if (obj is Stream stream)
		{
			try
			{
				// can throw exception if stream has been closed
				return "[" + stream.Length.ToString("N0") + "]";
			}
			catch (Exception)
			{
			}
		}

		if (obj is DictionaryEntry dictionaryEntry)
		{
			return dictionaryEntry.Key.ToString();
		}

		string? valueString = obj.ToString();
		if (valueString == type.ToString())
		{
			return '(' + type.Name + ')';
		}

		if (valueString!.Length > maxFormatLength)
		{
			return valueString[..maxFormatLength];
		}

		return valueString;
	}

	public static string EnumerableToString(this IEnumerable enumerable)
	{
		var strings = new List<string?>();
		foreach (var item in enumerable)
		{
			strings.Add(item.ToString());
		}

		string joined = string.Join(", ", strings);
		return joined;
		//return "[" + joined + "]";
	}

	public static string CollectionToString(this ICollection collection)
	{
		return EnumerableToString(collection);
	}

	// .Net 9 adds .Index() support to replace this
	public static IEnumerable<(int index, T item)> WithIndex<T>(this IEnumerable<T> self)
		=> self.Select((item, index) => (index, item));

	public static string? ToUniqueString(this object? obj)
	{
		if (obj == null)
			return null;

		Type type = obj.GetType();

		if (obj is string text)
			return text;

		if (type.IsPrimitive == false)
		{
			if (obj is DateTime dateTime)
			{
				return dateTime.FormatId();
			}

			// use any ToString() that overrides the base
			MethodInfo toStringMethod = type.GetMethod("ToString", Type.EmptyTypes)!;
			if (toStringMethod.DeclaringType != typeof(object) && toStringMethod.DeclaringType != typeof(ValueType))
			{
				return obj.ToString();
			}
		}

		if (type.IsNumeric())
		{
			MethodInfo toStringMethod = type.GetMethod("ToString", [typeof(string)])!;
			string format = type.IsDecimal() ? "N" : "N0";
			object? result = toStringMethod.Invoke(obj, [format]);
			return (string?)result;
		}

		if (obj is DictionaryEntry dictionaryEntry)
			return dictionaryEntry.Key.ToString();

		string? valueString = obj.ToString();
		if (valueString != type.ToString())
			return valueString;

		// it's using the base to string
		// No unique identifier found yet, start looking in the properties and fields

		// Return first non-null property value
		PropertyInfo[] properties = type.GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			object? propertyValue = propertyInfo.GetValue(obj);
			if (propertyValue != null)
			{
				string? toString = ToUniqueString(propertyValue);
				if (toString != null)
					return toString;
			}
		}

		// Return first non-null field value
		FieldInfo[] fields = type.GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			object? fieldValue = fieldInfo.GetValue(obj);
			if (fieldValue != null)
			{
				string? toString = ToUniqueString(fieldValue);
				if (toString != null)
					return toString;
			}
		}

		return null;
	}
}
