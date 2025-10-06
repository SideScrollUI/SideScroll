using SideScroll.Attributes;
using System.Collections;
using System.Reflection;
using System.Xml;

namespace SideScroll.Extensions;

public static class SideScrollExtensions
{
	/// <summary>
	/// Returns all child XML nodes with the specified name
	/// </summary>
	public static XmlNode[] Elements(this XmlNode xmlNode, string name)
	{
		return xmlNode.ChildNodes
			.OfType<XmlNode>()
			.Where(xmlNode => xmlNode.Name == name)
			.ToArray();
	}

	/// <summary>
	/// Merges items from newList into iList, adding only items that don't already exist
	/// </summary>
	public static void Merge(this IList iList, IList newList)
	{
		foreach (object item in newList)
		{
			if (!iList.Contains(item))
			{
				iList.Add(item);
			}
		}
	}

	/// <summary>
	/// Merges properties from newObject into obj, setting only null properties in obj
	/// </summary>
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

	/// <summary>
	/// Returns the value of the first property or field decorated with [InnerValue] attribute, recursively unwrapping nested inner values
	/// </summary>
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

	/// <summary>
	/// Formats a Version by removing trailing ".0" segments (e.g., "1.2.0.0" becomes "1.2")
	/// </summary>
	public static string Formatted(this Version version)
	{
		return version.ToString()
			.TrimEnd(".0")
			.TrimEnd(".0")
			.TrimEnd(".0");
	}

	/// <summary>
	/// Creates a new array containing a portion of the source array
	/// </summary>
	public static T[] SubArray<T>(this T[] array, int offset, int length)
	{
		var result = new T[length];
		Array.Copy(array, offset, result, 0, length);
		return result;
	}
}
