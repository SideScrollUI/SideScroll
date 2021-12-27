using Atlas.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Atlas.Extensions
{
	public static class ObjectExtensions
	{
		public static string Formatted(this object obj, int maxLength = 500)
		{
			if (obj == null)
				return null;

			Type type = obj.GetType();

			if (type.IsNumeric())
			{
				if (obj is double d)
					return d.ToString("#,0.###");

				MethodInfo toStringMethod = type.GetMethod("ToString", new Type[] { typeof(string) });
				string format = type.IsDecimal() ? "G" : "N0";
				object result = toStringMethod.Invoke(obj, new object[] { format });
				if (result == null)
					return null;

				return (string)result;
			}

			if (type.IsPrimitive == false)
			{
				if (obj is DateTime dateTime)
					return dateTime.ToString("yyyy-M-d H:mm:ss.FFFFFF");

				if (obj is TimeSpan timeSpan)
				{
					if (timeSpan.TotalSeconds < 1)
						return timeSpan.Trim(TimeSpan.FromMilliseconds(1)).ToString("g");
					else
						return timeSpan.FormattedDecimal();
				}

				// use any ToString() that overrides the base
				MethodInfo toStringMethod = type.GetMethod("ToString", Type.EmptyTypes);
				if (toStringMethod.DeclaringType != typeof(object) && toStringMethod.DeclaringType != typeof(ValueType))
				{
					string toString = obj.ToString();
					if (toString != null && toString.Length > maxLength)
						return toString[..maxLength];
					return toString;
				}
			}

			if (obj is string text)
			{
				if (text.Length > maxLength)
					return text[..maxLength];

				return text;
			}
			else if (obj is IDictionary dictionary)
			{
				return "{ " + dictionary.Count.ToString("N0") + " }";
			}
			else if (obj is ICollection collection)
			{
				Type elementType = type.GetElementTypeForAll();
				if (elementType != null && elementType.GetCustomAttribute<ToStringAttribute>() != null)
				{
					return CollectionToString(collection);
				}
				return collection.Count.ToString("N0");
				//return "[" + collection.Count.ToString("N0") + "]";
			}

			if (typeof(IEnumerable).IsAssignableFrom(type))
			{
				PropertyInfo countProp = type.GetProperty("Count");
				if (countProp != null)
				{
					int count = (int)countProp.GetValue(obj, null);
					return "[" + count.ToString("N0") + "]";
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
				return dictionaryEntry.Key.ToString();

			string valueString = obj.ToString();
			if (valueString == type.ToString())
			{
				return '(' + type.Name + ')';
			}

			if (valueString.Length > maxLength)
				return valueString[..maxLength];

			return valueString;
		}

		public static string FormattedDecimal(this double d)
		{
			return d.ToString("#,0.#");
		}

		public static string EnumerableToString(this IEnumerable enumerable)
		{
			var strings = new List<string>();
			foreach (var item in enumerable)
				strings.Add(item.ToString());

			string joined = string.Join(", ", strings);
			return joined;
			//return "[" + joined + "]";
		}

		public static string CollectionToString(this ICollection collection)
		{
			return EnumerableToString(collection);
		}

		public static string ToUniqueString(this object obj)
		{
			if (obj == null)
				return null;

			Type type = obj.GetType();

			if (obj is string text)
				return text;

			if (type.IsPrimitive == false)
			{
				if (obj is DateTime dateTime)
					return dateTime.ToString("yyyy-MM-dd H:mm:ss.FFFFFF");

				// use any ToString() that overrides the base
				MethodInfo toStringMethod = type.GetMethod("ToString", Type.EmptyTypes);
				if (toStringMethod.DeclaringType != typeof(object) && toStringMethod.DeclaringType != typeof(ValueType))
				{
					return obj.ToString();
				}
			}

			if (type.IsNumeric())
			{
				MethodInfo toStringMethod = type.GetMethod("ToString", new Type[] { typeof(string) });
				string format = type.IsDecimal() ? "N" : "N0";
				object result = toStringMethod.Invoke(obj, new object[] { format });
				if (result == null)
					return null;

				return (string)result;
			}

			if (obj is DictionaryEntry dictionaryEntry)
				return dictionaryEntry.Key.ToString();

			string valueString = obj.ToString();
			if (valueString != type.ToString())
				return valueString;

			// it's using the base to string
			// No unique identifier found yet, start looking in the properties and fields

			// Return first non-null property value
			PropertyInfo[] properties = type.GetProperties();
			foreach (PropertyInfo propertyInfo in properties)
			{
				object propertyValue = propertyInfo.GetValue(obj);
				if (propertyValue != null)
				{
					string toString = ToUniqueString(propertyValue);
					if (toString != null)
						return toString;
				}
			}

			// Return first non-null field value
			FieldInfo[] fields = type.GetFields();
			foreach (FieldInfo fieldInfo in fields)
			{
				object fieldValue = fieldInfo.GetValue(obj);
				if (fieldValue != null)
				{
					string toString = ToUniqueString(fieldValue);
					if (toString != null)
						return toString;
				}
			}

			return null;
		}
	}
}
