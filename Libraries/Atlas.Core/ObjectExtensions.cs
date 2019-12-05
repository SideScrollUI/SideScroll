using Atlas.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Atlas.Core
{
	public static class ObjectExtensions
	{

		// Needs new name
		public static string ObjectToString(this object obj, int maxLength = 255)
		{
			// don't override cell style formatting for numbers
			if (obj == null)
				return null;

			Type type = obj.GetType();

			if (obj is string text)
				return text;

			// handle decimal here: a decimal is considered a primitive
			if (type.IsNumeric())
			{
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
					return timeSpan.ToString("g");
				// use any ToString() that overrides the base
				MethodInfo toStringMethod = type.GetMethod("ToString", Type.EmptyTypes);
				if (toStringMethod.DeclaringType != typeof(object) && toStringMethod.DeclaringType != typeof(ValueType))
				{
					string toString = obj.ToString();
					if (toString != null && toString.Length > maxLength)
						return toString.Substring(0, maxLength);
					return toString;
				}
			}

			//string toString = obj.ToString();
			//if (toString != null && !toString.StartsWith("("))
			//	return toString;

			if (obj is IDictionary dictionary)
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
				return "[" + collection.Count.ToString("N0") + "]";
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
				return valueString.Substring(0, maxLength);
			//description = description.Replace('+', '.');

			return valueString;

			/*
			string label = type.Name;
			if (!type.IsGenericType)
			{
				if (type.IsGenericType)
				{
					label = label.Split('`')[0]; // Dictionary`1
					label += '<';
					label += type.GenericTypeArguments[0].Name;
					label += '>';
				}
			}
			return "( " + label + " )";*/
		}

		public static string EnumerableToString(this IEnumerable enumerable)
		{
			var strings = new List<string>();
			foreach (var item in enumerable)
				strings.Add(item.ToString());
			string joined = string.Join(", ", strings);
			return "[" + joined + "]";
		}

		public static string CollectionToString(this ICollection collection)
		{
			var strings = new List<string>();
			foreach (var item in collection)
				strings.Add(item.ToString());
			string joined = string.Join(", ", strings);
			return "[" + joined + "]";
		}

		private static string ObjectToUniqueStringAll(this object obj)
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

			//string toString = obj.ToString();
			//if (toString != null && !toString.StartsWith("("))
			//	return toString;

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

			PropertyInfo[] properties = type.GetProperties();
			foreach (PropertyInfo propertyInfo in properties)
			{
				object propertyValue = propertyInfo.GetValue(obj);
				if (propertyValue != null)
				{
					string toString = ObjectToUniqueStringAll(propertyValue);
					if (toString != null)
						return toString;
				}
			}

			FieldInfo[] fields = type.GetFields();
			foreach (FieldInfo fieldInfo in fields)
			{
				object fieldValue = fieldInfo.GetValue(obj);
				if (fieldValue != null)
				{
					string toString = ObjectToUniqueStringAll(fieldValue);
					if (toString != null)
						return toString;
				}
			}

			return null;
		}


		public static string ObjectToUniqueString(this object obj, int maxLength = 100)
		{
			string text = ObjectToUniqueStringAll(obj);
			if (text != null && text.Length > maxLength)
				return text.Substring(0, maxLength);
			return text;
		}
	}
}
