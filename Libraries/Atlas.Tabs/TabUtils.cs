using Atlas.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Atlas.Tabs;

public interface IHasLinks
{
	bool HasLinks { get; }
}

public static class TabUtils
{
	public static List<Type> IgnoreHighlightTypes { get; set; } = new();

	public static bool ObjectHasLinks(object obj, bool ignoreEmpty = false)
	{
		if (obj == null)
			return false;

		if (obj is IHasLinks hasLinks)
			return hasLinks.HasLinks;

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
			type == typeof(string) ||
			type == typeof(decimal) ||
			type == typeof(DateTime) ||
			type == typeof(TimeSpan))
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
}
