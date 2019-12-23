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
			bool upperCase = true;
			char prevChar = ' ';
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				if (upperCase)
				{
					upperCase = false;
					c = char.ToUpper(c);
				}
				if (c == '_')
				{
					c = ' ';
				}
				else if (c == '|')
				{
					newText.Append(" |");
					c = ' ';
					upperCase = true;
				}
				else if (prevChar != ' ')
				{
					if (char.IsUpper(c) && !char.IsUpper(prevChar))
						newText.Append(' ');
					else if (char.IsNumber(c) && !char.IsNumber(prevChar))
						newText.Append(' ');
					else if (char.IsUpper(prevChar) && char.IsUpper(c) && i + 1 < text.Length && char.IsLower(text[i + 1]))
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

			// todo:
			//FieldInfo[] fieldInfos = type.GetFields();
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
			if (matchingProperties.Count > 0)
				return matchingProperties[0];
			return null;
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

		public static DateTimeOffset Trim(this DateTimeOffset dateTimeOffset, long ticks)
		{
			DateTime dateTime = dateTimeOffset.UtcDateTime;
			return new DateTimeOffset(dateTime.Trim(ticks));
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
