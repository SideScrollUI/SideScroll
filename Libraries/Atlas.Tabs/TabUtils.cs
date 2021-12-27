using Atlas.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;

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
	}
}
