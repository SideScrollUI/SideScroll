using Atlas.Extensions;
using System;
using System.Reflection;

namespace Atlas.Core;

public static class ObjectUtils
{
	public static string GetObjectId(object obj)
	{
		string id = GetDataKey(obj);
		if (id == null)
		{
			object dataValue = GetDataValue(obj);
			if (dataValue != null)
				id = GetDataKey(dataValue);
		}
		return id ?? obj.ToUniqueString();
	}

	public static string GetDataKey(object obj)
	{
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
	public static object GetDataValue(object obj)
	{
		Type type = obj.GetType();
		if (type.GetCustomAttribute<DataKeyAttribute>() != null)
			return obj;

		var valueProperties = type.GetPropertiesWithAttribute<DataValueAttribute>();
		var valueFields = type.GetFieldsWithAttribute<DataValueAttribute>();
		if (valueProperties.Count > 0)
		{
			return valueProperties[0].GetValue(obj);
		}
		else if (valueFields.Count > 0)
		{
			return valueFields[0].GetValue(obj);
		}
		return null;
	}

	public static bool IsEqual(object obj1, object obj2)
	{
		if (obj1 == null) return obj2 == null;
		if (obj2 == null) return false;

		Type type = obj1.GetType();
		object covertedObject = Convert.ChangeType(obj2, type);
		return obj1.Equals(covertedObject);
	}
}
