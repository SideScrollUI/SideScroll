using SideScroll.Attributes;
using System.Collections;
using System.Reflection;

namespace SideScroll.Extensions;

public static class ObjectExtensions
{
	/// <summary>
	/// Default maximum length for formatted strings
	/// </summary>
	public static int DefaultMaxFormattedLength { get; set; } = 500;

	/// <summary>
	/// Formats an object as a human-readable string with optional length limit. Handles various types intelligently:
	/// - Numbers: Formatted with thousand separators (N0 for integers, G for decimals)
	/// - DateTime: Uses Format() extension with current timezone
	/// - TimeSpan: Shows as decimal (e.g., "3.5 Hours") or trimmed milliseconds
	/// - Enums: Converts to word-spaced text (e.g., "MyValue" becomes "My Value")
	/// - Collections: Shows count or comma-separated items (for [ToString] types)
	/// - Strings: Truncated to maxLength if needed
	/// - Custom types: Uses ToString() if overridden, otherwise shows type name
	/// </summary>
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

		int maxFormatLength = maxLength ?? DefaultMaxFormattedLength;

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

	/// <summary>
	/// Converts an enumerable to a comma-separated string of its items
	/// </summary>
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

	/// <summary>
	/// Converts a collection to a comma-separated string of its items
	/// </summary>
	public static string CollectionToString(this ICollection collection)
	{
		return EnumerableToString(collection);
	}

	/// <summary>
	/// Adds an index to each item in an enumerable sequence (Note: .NET 9 adds .Index() support to replace this)
	/// </summary>
	public static IEnumerable<(int index, T item)> WithIndex<T>(this IEnumerable<T> self)
		=> self.Select((item, index) => (index, item));

	/// <summary>
	/// Converts an object to a unique string representation suitable for identification. Attempts to find the most meaningful identifier by checking in order:
	/// - Strings: Returns as-is
	/// - DateTime: Formats as UTC identifier (FormatId)
	/// - Custom ToString(): Uses if overridden from base Object/ValueType
	/// - Numbers: Formatted with thousand separators (N for decimals, N0 for integers)
	/// - DictionaryEntry: Uses the Key
	/// - Fallback: Recursively searches properties then fields for first non-null value
	/// Returns null if no unique identifier can be determined
	/// </summary>
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
