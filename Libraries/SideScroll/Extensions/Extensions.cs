using SideScroll.Attributes;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Xml;

namespace SideScroll.Extensions;

/// <summary>
/// General extension methods for XML, lists, objects, versions, and arrays
/// </summary>
public static class SideScrollExtensions
{
	/// <summary>
	/// Returns all child XML nodes with the specified name
	/// </summary>
	public static XmlNode[] Elements(this XmlNode xmlNode, string name)
	{
		return xmlNode.ChildNodes
			.OfType<XmlNode>()
			.Where(childNode => childNode.Name == name)
			.ToArray();
	}

	/// <summary>
	/// Merges items from newList into iList, adding only items that don't already exist
	/// </summary>
	public static void Merge(this IList list, IList newItems)
	{
		foreach (object item in newItems)
		{
			if (!list.Contains(item))
			{
				list.Add(item);
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
	/// Per-type cache of the first <c>[InnerValue]</c>-decorated member (property or field), or
	/// <c>null</c> when the type has none. Computed once; subsequent calls do a single dictionary
	/// lookup instead of iterating all properties and fields and calling <c>GetCustomAttribute</c> on each.
	/// </summary>
	private static readonly ConcurrentDictionary<Type, MemberInfo?> InnerValueMembers = new();

	private static MemberInfo? ComputeInnerValueMember(Type type)
	{
		foreach (PropertyInfo propertyInfo in type.GetProperties())
		{
			if (propertyInfo.GetCustomAttribute<InnerValueAttribute>() != null)
				return propertyInfo;
		}

		foreach (FieldInfo fieldInfo in type.GetFields())
		{
			if (fieldInfo.GetCustomAttribute<InnerValueAttribute>() != null)
				return fieldInfo;
		}

		return null;
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
		switch (InnerValueMembers.GetOrAdd(type, ComputeInnerValueMember))
		{
			case PropertyInfo innerProperty:
				return innerProperty.GetValue(value).GetInnerValue();
			case FieldInfo innerField:
				return innerField.GetValue(value).GetInnerValue();
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
