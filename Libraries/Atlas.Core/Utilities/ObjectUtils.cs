using Atlas.Extensions;
using System.Collections;
using System.Reflection;

namespace Atlas.Core;

public static class ObjectUtils
{
	public static string? GetObjectId(object? obj)
	{
		string? id = GetDataKey(obj);
		if (id == null)
		{
			object? dataValue = GetDataValue(obj);
			if (dataValue != null)
				id = GetDataKey(dataValue);
		}
		return id ?? obj.ToUniqueString();
	}

	// Returns the Value.ToUniqueString() for the first property or field that has a [DataKey]
	public static string? GetDataKey(object? obj)
	{
		if (obj == null) return null;

		Type type = obj.GetType();
		var keyProperties = type.GetPropertiesWithAttribute<DataKeyAttribute>();
		var keyFields = type.GetFieldsWithAttribute<DataKeyAttribute>();
		if (keyProperties.Count > 0)
		{
			return keyProperties[0].GetValue(obj)?.ToUniqueString();
		}
		else if (keyFields.Count > 0)
		{
			return keyFields[0].GetValue(obj)?.ToUniqueString();
		}
		return null;
	}

	// Get's the [DataValue] member that will be imported with an Imported Bookmark
	public static object? GetDataValue(object? obj)
	{
		if (obj == null) return null;

		Type type = obj.GetType();
		if (type.GetCustomAttribute<DataKeyAttribute>() != null)
			return obj;

		var valueProperties = type.GetPropertiesWithAttribute<DataValueAttribute>();
		if (valueProperties.Count > 0)
		{
			return valueProperties[0].GetValue(obj);
		}

		var valueFields = type.GetFieldsWithAttribute<DataValueAttribute>();
		if (valueFields.Count > 0)
		{
			return valueFields[0].GetValue(obj);
		}
		return null;
	}

	public static bool AreEqual(object? obj1, object? obj2, int maxDepth = 3)
	{
		if (obj1 == null) return obj2 == null;
		if (obj2 == null) return false;

		if (obj1 is IList list1 && obj2 is IList list2)
		{
			return AreListsEqual(list1, list2, maxDepth);
		}

		Type type = obj1.GetType();
		object covertedObject = Convert.ChangeType(obj2, type);
		return obj1.Equals(covertedObject);
	}

	private static bool AreListsEqual(IList list1, IList list2, int maxDepth)
	{
		if (list1.Count != list2.Count) return false;

		maxDepth--;
		if (maxDepth < 0) throw new Exception("Max depth exceeded");

		for (int i = 0; i < list1.Count; i++)
		{
			if (!AreEqual(list1[i], list2[i], maxDepth)) return false;
		}
		return true;
	}
}
