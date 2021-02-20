using Atlas.Core;
using Atlas.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Atlas.Tabs
{
	public class TabUtils
	{
		public static List<Type> IgnoreHighlightTypes { get; set; } = new List<Type>();

		public static bool ObjectHasLinks(object obj, bool ignoreEmpty = false)
		{
			if (obj == null)
				return false;

			object value = obj.GetInnerValue();
			if (value == null)
				return false;

			if (value is ListItem listItem)
				value = listItem.Value;
			if (value is ListMember listMember)
				value = listMember.Value;

			Type type = value.GetType();
			if (type.IsPrimitive ||
				type.IsEnum ||
				type.Equals(typeof(string)) ||
				type.Equals(typeof(decimal)) ||
				type.Equals(typeof(DateTime)) ||
				type.Equals(typeof(TimeSpan)))
			{
				return false;
			}

			if (ignoreEmpty)
			{
				if (value is ICollection collection)
				{
					if (collection.Count == 0)
						return false;

					Type elementType = collection.GetType().GetElementTypeForAll();
					if (elementType != null && elementType.IsPrimitive)
						return false;
				}

				foreach (Type ignoreType in IgnoreHighlightTypes)
				{
					if (ignoreType.IsAssignableFrom(type))
						return false;
				}
			}

			return true;
		}

		public static string GetItemId(object obj)
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
				return keyProperties[0].GetValue(obj)?.ToString();
			}
			else if (keyFields.Count > 0)
			{
				return keyFields[0].GetValue(obj)?.ToString();
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
	}
}
