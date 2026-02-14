using SideScroll.Attributes;
using SideScroll.Extensions;
using System.Collections;
using System.Reflection;

namespace SideScroll.Utilities;

/// <summary>
/// Provides utilities for working with objects, including data key and value extraction
/// </summary>
public static class ObjectUtils
{
	/// <summary>
	/// Gets a unique identifier string for an object, using DataKey or DataValue attributes if available
	/// </summary>
	/// <returns>A unique string identifier for the object</returns>
	public static string? GetObjectId(object? obj)
	{
		string? id = GetDataKey(obj);
		if (id == null)
		{
			object? dataValue = GetDataValue(obj);
			if (dataValue != null)
			{
				id = GetDataKey(dataValue);
			}
		}
		return id ?? obj.ToUniqueString();
	}

	/// <summary>
	/// Gets the data key value for an object by finding the first property or field with a [DataKey] attribute
	/// </summary>
	/// <returns>The unique string representation of the data key value, or null if no data key is found</returns>
	public static string? GetDataKey(object? obj)
	{
		if (obj == null) return null;

		Type type = obj.GetType();
		var keyProperties = type.GetPropertiesWithAttribute<DataKeyAttribute>();
		if (keyProperties.FirstOrDefault() is PropertyInfo propertyInfo)
		{
			return propertyInfo.GetValue(obj)?.ToUniqueString();
		}

		var keyFields = type.GetFieldsWithAttribute<DataKeyAttribute>();
		if (keyFields.FirstOrDefault() is FieldInfo fieldInfo)
		{
			return fieldInfo.GetValue(obj)?.ToUniqueString();
		}
		return null;
	}

	/// <summary>
	/// Gets the data value for an object by finding the first property or field with a [DataValue] attribute,
	/// or returns the object itself if the type has a [PublicData] attribute
	/// </summary>
	/// <returns>The data value object, the object itself if it has [PublicData] attribute, or null if no data value is found</returns>
	public static object? GetDataValue(object? obj)
	{
		if (obj == null) return null;

		Type type = obj.GetType();

		var valueProperties = type.GetPropertiesWithAttribute<DataValueAttribute>();
		if (valueProperties.FirstOrDefault() is PropertyInfo propertyInfo)
		{
			return propertyInfo.GetValue(obj);
		}

		var valueFields = type.GetFieldsWithAttribute<DataValueAttribute>();
		if (valueFields.FirstOrDefault() is FieldInfo fieldInfo)
		{
			return fieldInfo.GetValue(obj);
		}

		if (type.GetCustomAttribute<PublicDataAttribute>() != null)
			return obj;

		return null;
	}

	/// <summary>
	/// Determines whether two objects are equal, with support for comparing lists
	/// </summary>
	/// <param name="maxDepth">Maximum recursion depth for comparing nested collections (default is 3)</param>
	/// <returns>True if the objects are equal; otherwise, false</returns>
	public static bool AreEqual(object? obj1, object? obj2, int maxDepth = 3)
	{
		if (obj1 == null) return obj2 == null;
		if (obj2 == null) return false;

		if (obj1 is IList list1 && obj2 is IList list2)
		{
			return AreListsEqual(list1, list2, maxDepth);
		}

		Type type = obj1.GetType();
		object convertedObject = Convert.ChangeType(obj2, type);
		return obj1.Equals(convertedObject);
	}

	private static bool AreListsEqual(IList list1, IList list2, int maxDepth)
	{
		if (list1.Count != list2.Count) return false;

		maxDepth--;
		if (maxDepth < 0) throw new TaggedException("Max depth exceeded");

		for (int i = 0; i < list1.Count; i++)
		{
			if (!AreEqual(list1[i], list2[i], maxDepth)) return false;
		}
		return true;
	}
}
