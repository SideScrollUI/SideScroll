using SideScroll.Attributes;
using System.Collections;
using System.Reflection;
using System.Xml;

namespace SideScroll.Extensions;

public static class SideScrollExtensions
{
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

			object? existingValue = propertyInfo.GetValue(obj);
			if (existingValue != null)
				continue;

			object? newValue = propertyInfo.GetValue(newObject);
			propertyInfo.SetValue(obj, newValue);
		}

		// todo:
		//FieldInfo[] fieldInfos = type.GetFields();
	}

	// Returns value of first property or field that sets [InnerValue]
	public static object? GetInnerValue(this object? value)
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
				object? propertyValue = propertyInfo.GetValue(value);
				return GetInnerValue(propertyValue);
			}
		}

		foreach (FieldInfo fieldInfo in type.GetFields())
		{
			if (fieldInfo.GetCustomAttribute<InnerValueAttribute>() != null)
			{
				object? fieldValue = fieldInfo.GetValue(value);
				return GetInnerValue(fieldValue);
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
		return version.ToString()
			.TrimEnd(".0")
			.TrimEnd(".0")
			.TrimEnd(".0");
	}

	public static T[] SubArray<T>(this T[] array, int offset, int length)
	{
		T[] result = new T[length];
		Array.Copy(array, offset, result, 0, length);
		return result;
	}
}
