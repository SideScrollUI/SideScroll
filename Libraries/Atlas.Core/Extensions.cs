using Atlas.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Atlas.Extensions // rename to Core?
{
	public static class AtlasExtensions
	{
		private static HashSet<Type> NumericTypes = new HashSet<Type>
		{
			typeof(byte), typeof(sbyte),
			typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong),
			typeof(float), typeof(double), typeof(decimal),
		};

		private static HashSet<Type> DecimalTypes = new HashSet<Type>
		{
			typeof(float), typeof(double), typeof(decimal),
		};

		public static XmlNode[] Elements(this XmlDocument xmlDoc, string name)
		{
			List<XmlNode> list = new List<XmlNode>();
			foreach (XmlNode xmlNode in xmlDoc.ChildNodes)
			{
				if (xmlNode.Name == name)
					list.Add(xmlNode);
			}
			return list.ToArray();
		}

		public static XmlNode[] Elements(this XmlNode xmlNode, string name)
		{
			List<XmlNode> list = new List<XmlNode>();
			foreach (XmlNode childNode in xmlNode.ChildNodes)
			{
				if (childNode.Name == name)
					list.Add(childNode);
			}
			return list.ToArray();
		}

		public static IEnumerable<int> GetAllIndexes(this string source, string matchString)
		{
			matchString = Regex.Escape(matchString);
			foreach (Match match in Regex.Matches(source, matchString))
			{
				yield return match.Index;
			}
		}

		public static List<int> AllIndexesOf(this string str, string value)
		{
			if (String.IsNullOrEmpty(value))
				throw new ArgumentException("the string to find may not be empty", "value");
			List<int> indexes = new List<int>();
			for (int index = 0; ; index += value.Length)
			{
				index = str.IndexOf(value, index);
				if (index == -1)
					return indexes;
				indexes.Add(index);
			}
		}

		public static IEnumerable<int> AllIndexesOfYield(this string str, string value)
		{
			if (String.IsNullOrEmpty(value))
				throw new ArgumentException("the string to find may not be empty", "value");
			for (int index = 0; ; index += value.Length)
			{
				index = str.IndexOf(value, index);
				if (index == -1)
					break;
				yield return index;
			}
		}

		public static bool CaseInsensitiveContains(this string text, string value, StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
		{
			return text.IndexOf(value, stringComparison) >= 0;
		}

		public static string Reverse(this string input)
		{
			char[] chars = input.ToCharArray();
			Array.Reverse(chars);
			return new String(chars);
		}

		public static string Trim(this string input, string prefix)
		{
			return input.Substring(prefix.Length);
		}

		public static string TrimEnd(this string input, string postfix)
		{
			return input.Substring(0, input.Length - postfix.Length);
		}

		public static string Range(this string input, int start, int end)
		{
			end++;
			end = Math.Min(end, input.Length);
			if (end < start)
				return "";
			return input.Substring(start, end - start);
		}

		public static string Range(this string input, int start)
		{
			if (input.Length < start)
				return "";
			return input.Substring(start, input.Length - start);
		}

		public static string AddSpacesBetweenWords(this string text)
		{
			if (string.IsNullOrWhiteSpace(text))
				return "";
			StringBuilder newText = new StringBuilder(text.Length * 2);
			newText.Append(char.ToUpper(text[0]));
			char prevChar = newText[0];
			for (int i = 1; i < text.Length; i++)
			{
				char c = text[i];
				if (c == '_')
				{
					c = ' ';
				}
				else if (prevChar != ' ')
				{
					if (char.IsUpper(c) && !char.IsUpper(prevChar))
						newText.Append(' ');
					else if (char.IsNumber(c) && !char.IsNumber(prevChar))
						newText.Append(' ');
				}
				newText.Append(c);
				prevChar = c;
			}
			return newText.ToString();
		}

		public static bool IsNumeric(this Type type)
		{
			type = GetNonNullableType(type);
			return NumericTypes.Contains(type);
		}

		public static bool IsDecimal(this Type type)
		{
			type = GetNonNullableType(type);
			return DecimalTypes.Contains(type);
		}

		public static Type GetNonNullableType(this Type type)
		{
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
				return Nullable.GetUnderlyingType(type);
			return type;
		}

		public static bool IsAssignableToGenericType(this Type givenType, Type genericType)
		{
			var interfaceTypes = givenType.GetInterfaces();

			foreach (var it in interfaceTypes)
			{
				if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
					return true;
			}

			if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
				return true;

			Type baseType = givenType.BaseType;
			if (baseType == null)
				return false;

			return IsAssignableToGenericType(baseType, genericType);
		}

		public static Type GetElementTypeForAll(this Type type)
		{
			if (type.HasElementType)
				return type.GetElementType();
			else if (type.GenericTypeArguments.Length > 0)
				return type.GenericTypeArguments[0];
			else if (type.BaseType != null)
				return GetElementTypeForAll(type.BaseType);

			return null;
		}

		public static void Merge(this IList iList, IList newList)
		{
			foreach (object item in newList)
			{
				if (!iList.Contains(item))
					iList.Add(item);
			}
		}

		public static void Merge(this object obj, object newObject)
		{
			Type type = obj.GetType();
			PropertyInfo[] propertyInfos = type.GetProperties();
			foreach (PropertyInfo propertyInfo in propertyInfos)
			{
				if (!propertyInfo.CanWrite)
					continue;

				object existingValue = propertyInfo.GetValue(obj);
				if (existingValue != null)
					continue;
				object newValue = propertyInfo.GetValue(newObject);
				propertyInfo.SetValue(obj, newValue);
			}

			FieldInfo[] fieldInfos = type.GetFields();
		}

		public static string ObjectToString(this object obj, int maxLength = 100)
		{
			// don't override cell style formatting for numbers
			if (obj == null)
				return null;

			Type type = obj.GetType();

			if (type.Equals(typeof(string)))
				return (string)obj;

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
					return dateTime.ToString("yyyy-MM-dd HH:mm:ss.FFFFFF");
				// use any ToString() that overrides the base
				MethodInfo toStringMethod = type.GetMethod("ToString", Type.EmptyTypes);
				if (toStringMethod.DeclaringType != typeof(Object) && toStringMethod.DeclaringType != typeof(ValueType))
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

			if (typeof(ICollection).IsAssignableFrom(type))
			{
				return "[" + ((ICollection)obj).Count.ToString("N0") + "]";
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
			if (type == typeof(DictionaryEntry))
				return ((DictionaryEntry)obj).Key.ToString();

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

		private static string ObjectToUniqueStringAll(this object obj)
		{
			if (obj == null)
				return null;

			Type type = obj.GetType();

			if (type.Equals(typeof(string)))
				return (string)obj;

			if (type.IsPrimitive == false)
			{
				if (obj is DateTime dateTime)
					return dateTime.ToString("yyyy-MM-dd HH:mm:ss.FFFFFF");
				// use any ToString() that overrides the base
				MethodInfo toStringMethod = type.GetMethod("ToString", Type.EmptyTypes);
				if (toStringMethod.DeclaringType != typeof(Object) && toStringMethod.DeclaringType != typeof(ValueType))
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
			if (type == typeof(DictionaryEntry))
				return ((DictionaryEntry)obj).Key.ToString();

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

		public static List<PropertyInfo> GetVisibleProperties(this Type type)
		{
			List<PropertyInfo> visibleProperties = new List<PropertyInfo>();
			// Properties are returned in a random order, so sort them by the MetadataToken to get the original order
			PropertyInfo[] propertyInfos = type.GetProperties().OrderBy(x => x.MetadataToken).ToArray();
			foreach (PropertyInfo propertyInfo in propertyInfos)
			{
				if (propertyInfo.GetCustomAttribute(typeof(HiddenColumnAttribute)) != null)
					continue;

				visibleProperties.Add(propertyInfo);
			}
			return visibleProperties;
		}

		public static PropertyInfo GetPropertyWithAttribute<T>(this Type type)
		{
			List<PropertyInfo> matchingProperties = GetPropertiesWithAttribute<T>(type);
			Debug.Assert(matchingProperties.Count == 1);
			return matchingProperties[0];
		}

		public static List<PropertyInfo> GetPropertiesWithAttribute<T>(this Type type)
		{
			List<PropertyInfo> matchingProperties = new List<PropertyInfo>();
			// Properties are returned in a random order, so sort them by the MetadataToken to get the original order
			PropertyInfo[] propertyInfos = type.GetProperties().OrderBy(x => x.MetadataToken).ToArray();
			foreach (PropertyInfo propertyInfo in propertyInfos)
			{
				if (propertyInfo.GetCustomAttribute(typeof(T)) != null)
					matchingProperties.Add(propertyInfo);
			}
			return matchingProperties;
		}

		public static List<FieldInfo> GetFieldsWithAttribute<T>(this Type type)
		{
			List<FieldInfo> matchingFields = new List<FieldInfo>();
			// Fields are returned in a random order, so sort them by the MetadataToken to get the original order
			FieldInfo[] fieldInfos = type.GetFields().OrderBy(x => x.MetadataToken).ToArray();
			foreach (FieldInfo fieldInfo in fieldInfos)
			{
				if (fieldInfo.GetCustomAttribute(typeof(T)) != null)
					matchingFields.Add(fieldInfo);
			}
			return matchingFields;
		}

		public static object GetInnerValue(this object value)
		{
			if (value == null)
				return null;
			Type type = value.GetType();
			foreach (PropertyInfo propertyInfo in type.GetProperties())
			{
				if (propertyInfo.GetCustomAttribute(typeof(InnerValueAttribute)) != null)
				{
					value = propertyInfo.GetValue(value);
					return GetInnerValue(value);
				}
			}
			foreach (FieldInfo fieldInfo in type.GetFields())
			{
				if (fieldInfo.GetCustomAttribute(typeof(InnerValueAttribute)) != null)
				{
					value = fieldInfo.GetValue(value);
					return GetInnerValue(value);
				}
			}
			if (value is DictionaryEntry dictionaryEntry)
			{
				if (dictionaryEntry.Key is string)
					return dictionaryEntry.Value;
			}
			return value;
		}

		public static DateTime Trim(this DateTime date, long ticks)
		{
			return new DateTime(date.Ticks - (date.Ticks % ticks), date.Kind);
		}

		/*public static bool IsAction(object obj)
		{
			if (obj == null)
				return false;

			Type[] types = new[]
			{
				typeof(Action), typeof(Action<>), typeof(Action<,>),
				typeof(Func<>), typeof(Func<,>), typeof(Func<,,>),
			};
			//return types.Contains(value.GetType());
			Type type = obj.GetType();
			//if (type.IsAssignableFrom(typeof(MethodInfo)))
			if (obj is MethodInfo)
				return true;
			return (types.Contains(type) || (type.IsGenericType && types.Contains(type.GetGenericTypeDefinition())));
		}*/
	}
}
/*
*/
