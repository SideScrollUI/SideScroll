using Atlas.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
			var list = new List<XmlNode>();
			foreach (XmlNode xmlNode in xmlDoc.ChildNodes)
			{
				if (xmlNode.Name == name)
					list.Add(xmlNode);
			}
			return list.ToArray();
		}

		public static XmlNode[] Elements(this XmlNode xmlNode, string name)
		{
			var list = new List<XmlNode>();
			foreach (XmlNode childNode in xmlNode.ChildNodes)
			{
				if (childNode.Name == name)
					list.Add(childNode);
			}
			return list.ToArray();
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
			var visibleProperties = new List<PropertyInfo>();
			// Properties are returned in a random order, so sort them by the MetadataToken to get the original order
			PropertyInfo[] propertyInfos = type.GetProperties().OrderBy(x => x.MetadataToken).ToArray();
			foreach (PropertyInfo propertyInfo in propertyInfos)
			{
				if (propertyInfo.GetCustomAttribute<HiddenAttribute>() != null)
					continue;

				if (propertyInfo.GetCustomAttribute<HiddenColumnAttribute>() != null)
					continue;

				if (propertyInfo.GetIndexParameters().Any())
					continue;

				visibleProperties.Add(propertyInfo);
			}
			return visibleProperties;
		}

		public static PropertyInfo GetPropertyWithAttribute<T>(this Type type) where T : Attribute
		{
			return GetPropertiesWithAttribute<T>(type).FirstOrDefault();
		}

		public static List<PropertyInfo> GetPropertiesWithAttribute<T>(this Type type) where T : Attribute
		{
			// Properties are returned in a random order, so sort them by the MetadataToken to get the original order
			return type.GetProperties()
				.Where(p => p.GetCustomAttribute<T>() != null)
				.OrderBy(p => p.MetadataToken)
				.ToList();
		}

		public static List<FieldInfo> GetFieldsWithAttribute<T>(this Type type) where T : Attribute
		{
			// Fields are returned in a random order, so sort them by the MetadataToken to get the original order
			return type.GetFields()
				.Where(f => f.GetCustomAttribute<T>() != null)
				.OrderBy(f => f.MetadataToken)
				.ToList();
		}

		// Returns value of first property or field that sets [InnerValue]
		public static object GetInnerValue(this object value)
		{
			if (value == null)
				return null;

			// will be loaded later
			if (value is ILoadAsync)
				return value;

			Type type = value.GetType();
			foreach (PropertyInfo propertyInfo in type.GetProperties())
			{
				if (propertyInfo.GetCustomAttribute<InnerValueAttribute>() != null)
				{
					value = propertyInfo.GetValue(value);
					return GetInnerValue(value);
				}
			}

			foreach (FieldInfo fieldInfo in type.GetFields())
			{
				if (fieldInfo.GetCustomAttribute<InnerValueAttribute>() != null)
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

		public static string Formatted(this Version version)
		{
			return version.ToString().TrimEnd(".0").TrimEnd(".0").TrimEnd(".0");
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
